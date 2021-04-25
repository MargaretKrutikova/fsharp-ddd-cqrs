module Domain.Logic

open System
open Domain.Commands
open Domain.Types
open Domain.Events

module BorrowedStatusDetails =
    let create (dateTime: DateTime) (borrowerId: UserId) queue =
        { BorrowedBy = borrowerId
          BorrowedAt = dateTime
          RequestToBorrowQueue = queue }

    let private addUserToRequestQueue (requestedAt: DateTime) (userId: UserId) (queue: RequestToBorrowQueue) =
        queue
        @ [ { RequestedBy = userId
              RequestedDate = requestedAt } ]

    let createEmptyQueue (): RequestToBorrowQueue = List.empty

    let addUserToQueue (dateTime: DateTime) (userId: UserId) details =
        let queue =
            details.RequestToBorrowQueue
            |> addUserToRequestQueue dateTime userId

        { details with
              RequestToBorrowQueue = queue }

    let private hasUserRequestInQueue (userId: UserId) (queue: RequestToBorrowQueue) =
        queue
        |> Seq.map (fun entry -> entry.RequestedBy)
        |> Seq.contains userId

    let userCanPlaceRequest (details: BorrowedStatusDetails) (userId: UserId) =
        userId <> details.BorrowedBy
        && hasUserRequestInQueue userId details.RequestToBorrowQueue
           |> not

let private updateStatus (bookListing: BookListing) status = { bookListing with Status = status }

let publishBookListing (args: PublishBookListingArgs) =
    let listingToPublish =
        { Id = args.Id
          OwnerId = args.OwnerId
          Author = args.Author
          Title = args.Title
          Status = Available }

    (listingToPublish, BookPublished listingToPublish)

let placeRequestToBorrow (requestedBy: UserId) (dateTime: DateTime) (bookListing: BookListing) =
    match bookListing.Status with
    | Available -> BookHasNoRequestQueue bookListing.Id |> Error
    | Borrowed status when BorrowedStatusDetails.userCanPlaceRequest status requestedBy ->
        let updatedListing =
            status
            |> BorrowedStatusDetails.addUserToQueue dateTime requestedBy
            |> Borrowed
            |> updateStatus bookListing

        let event =
            UserAddedToRequestQueue
                { BookId = bookListing.Id
                  DateTime = dateTime
                  UserId = requestedBy }

        (updatedListing, event) |> Ok
    | Borrowed _ ->
        UserCantPlaceRequestToBorrow(bookListing.Id, requestedBy)
        |> Error

let returnBook (borrowerId: UserId) (dateTime: DateTime) (bookListing: BookListing) =
    match bookListing.Status with
    | Available -> BookIsNotBorrowed bookListing.Id |> Error
    | Borrowed status when status.BorrowedBy <> borrowerId ->
        BookIsNotBorrowedByUser(bookListing.Id, borrowerId)
        |> Error

    | Borrowed ({ RequestToBorrowQueue = [] }) ->
        let updatedListing = Available |> updateStatus bookListing

        let event =
            BookReturned
                { ReturnedBy = borrowerId
                  DateTime = dateTime
                  BookId = bookListing.Id }

        (updatedListing, event) |> Ok
    | Borrowed ({ RequestToBorrowQueue = firstEntry :: queue }) ->
        let updatedListing =
            BorrowedStatusDetails.create dateTime firstEntry.RequestedBy queue
            |> Borrowed
            |> updateStatus bookListing

        let event =
            BookRequestQueueAdvanced
                { BookId = bookListing.Id
                  ReturnedBy = borrowerId
                  NextBorrowerId = firstEntry.RequestedBy
                  DateTime = dateTime }

        (updatedListing, event) |> Ok

let borrowBook (borrowerId: UserId) (dateTime: DateTime) (bookListing: BookListing) =
    match bookListing.Status with
    | Available ->
        if borrowerId = bookListing.OwnerId then
            BorrowerCantBeTheSameAsOwner bookListing.Id |> Error
        else 
            let updatedListing =
                BorrowedStatusDetails.create dateTime borrowerId List.empty
                |> Borrowed
                |> updateStatus bookListing

            let event =
                BookBorrowed
                    { BookId = bookListing.Id
                      DateTime = dateTime
                      BorrowedBy = borrowerId }

            (updatedListing, event) |> Ok
    | Borrowed _ -> BookIsAlreadyBorrowed bookListing.Id |> Error
