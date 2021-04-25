module Api.Infrastructure.InMemoryPersistence

open System
open Api.Queries.Queries
open Api.Queries.QueryModels
open Application.Persistence
open Domain.Types

open FsToolkit.ErrorHandling

type InMemoryUser = { Id: Guid; UserName: string }

let create (): Persistence * ReadStorage =
    let mutable listings: BookListing list =
        [ { Id = Guid.Parse("bfc20b5b-884f-4646-a2f7-632133e3e23b") |> ListingId 
            Author = "Adrian Tchaikovsky"
            Title = "Children of Time"
            Status = Available
            OwnerId = Guid.Parse("1e94bedb-416f-443f-ac0f-28622665552e") |> UserId }]

    let mutable users: InMemoryUser list =
        [ { Id = Guid.Parse("1e94bedb-416f-443f-ac0f-28622665552e")
            UserName = "Bob" }
          { Id = Guid.Parse("2c1366e2-f73c-4aee-8608-d510c27a9ad5")
            UserName = "Alice" }
          { Id = Guid.Parse("e4096feb-78c1-46d2-87b5-bc59ae34725b")
            UserName = "Charlie" } ]

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
        let findUser (UserId id) =
            match users |> List.tryFind (fun u -> u.Id = id) with
            | Some user -> Ok user
            | None -> Error RecordNotFound

        let toListingStatusModel (status: ListingStatus) =
            result {
                match status with
                | Available -> return ListingStatusModel.Available
                | Borrowed details ->
                    let! user = findUser details.BorrowedBy
                    return ListingStatusModel.Borrowed
                            { BorrowedByUserName = user.UserName
                              NumberOfUsersInQueue = details.RequestToBorrowQueue.Length }
            }

        let toListingQueryModel (listing: BookListing) =
            result {
                let! owner = findUser listing.OwnerId
                let (ListingId id) = listing.Id
                let! status = toListingStatusModel listing.Status
                
                return { Id = id
                         Author = listing.Author
                         Title = listing.Title
                         OwnerUserName = owner.UserName
                         Status = status }
            }

        let getPublishedListings () =
            listings
            |> List.traverseResultM toListingQueryModel
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
