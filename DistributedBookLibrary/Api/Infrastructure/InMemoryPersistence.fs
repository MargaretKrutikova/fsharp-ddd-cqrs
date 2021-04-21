module Api.Infrastructure.InMemoryPersistence

open System
open Api.Queries.Queries
open Api.Queries.QueryModels
open Application.Persistence
open Domain.Types

open FsToolkit.ErrorHandling

type InMemoryUser = { Id: Guid; UserName: string }

let create (): Persistence * ReadStorage =
    let mutable listings: BookListing list = List.empty

    let mutable users: InMemoryUser list =
        [ { Id = Guid.Parse("1e94bedb-416f-443f-ac0f-28622665552e")
            UserName = "Bob" }
          { Id = Guid.Parse("2c1366e2-f73c-4aee-8608-d510c27a9ad5")
            UserName = "Alice" } ]

    let createCommandPersistence () =
        let getBookById id =
            listings
            |> List.tryFind (fun l -> l.Id = id)
            |> Result.requireSome PersistenceError.BookNotFound
            |> Async.singleton

        let updateBook (book: BookListing) =
            listings <-
                listings
                |> List.map (fun l -> if l.Id = book.Id then book else l)

            Async.singleton (Ok())

        let addBook book =
            listings <- listings |> List.append [ book ]
            Async.singleton (Ok())

        { GetBookById = getBookById
          UpdateBook = updateBook
          AddBook = addBook }

    let createReadStorage (): ReadStorage =
        let findUser (UserId id) = users |> List.find (fun u -> u.Id = id)

        let toListingStatusModel (status: ListingStatus): ListingStatusModel =
            match status with
            | Available -> ListingStatusModel.Available
            | Borrowed details ->
                ListingStatusModel.Borrowed
                    { BorrowedByUserName = (findUser details.BorrowedBy).UserName
                      NumberOfUserInQueue = details.RequestToBorrowQueue.Length }

        let toListingQueryModel (listing: BookListing): PublishedListingModel =
            let owner = findUser listing.OwnerId
            let (ListingId id) = listing.Id

            { Id = id
              Author = listing.Author
              Title = listing.Title
              OwnerUserName = owner.UserName
              Status = toListingStatusModel listing.Status }

        let getPublishedListings () =
            listings
            |> List.map toListingQueryModel
            |> Ok
            |> Async.singleton

        let toUserListing type_ (listing: BookListing) =
            let (ListingId id) = listing.Id

            { ListingId = id
              Author = listing.Author
              Title = listing.Title
              Type = type_ }

        let getUserListingType (userId: UserId) (listing: BookListing) =
            if listing.OwnerId = userId then
                PublishedByUser |> Some
            else
                match listing.Status with
                | Borrowed details when details.BorrowedBy = userId -> BorrowedByUser |> Some
                | Borrowed details when details.RequestToBorrowQueue
                                        |> Seq.exists (fun entry -> entry.RequestedBy = userId) ->
                    WaitingInQueue |> Some
                | _ -> None

        let getUserListings id =
            let userId = UserId id

            listings
            |> List.choose (fun listing ->
                getUserListingType userId listing
                |> Option.map (fun type_ -> toUserListing type_ listing))
            |> Ok
            |> Async.singleton

        { getPublishedListings = getPublishedListings
          getUserListings = getUserListings }

    createCommandPersistence (), createReadStorage ()
