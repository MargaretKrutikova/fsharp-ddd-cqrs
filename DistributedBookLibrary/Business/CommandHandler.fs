module Application.CommandHandler

open Domain
open Domain.Commands
open Application.Persistence
open Domain.Events
open Domain.Types
open FsToolkit.ErrorHandling

type CommandError =
    | Domain of DomainError
    | Persistence of PersistenceError
    static member toPersistence error = Persistence error
    static member toDomain error = Domain error

let private publishBookHandler (persistence: Persistence) (args: PublishBookListingArgs) =
    asyncResult {
        let listing, event = Logic.publishBookListing args
        do! persistence.AddBook listing
            |> AsyncResult.mapError (CommandError.toPersistence)
            
        return event
    }
   
let private performListingUpdate (persistence: Persistence) listingId updateListing =
    asyncResult {
        let! bookListing =
            persistence.GetBookById listingId
            |> AsyncResult.mapError (CommandError.toPersistence)
            
        let! updatedListing, event =
            updateListing bookListing
            |> Async.singleton
            |> AsyncResult.mapError (CommandError.toDomain)
        
        do! persistence.UpdateBook updatedListing
            |> AsyncResult.mapError (CommandError.toPersistence)
            
        return event
    } 
   
let private borrowBookHandler (persistence: Persistence) (args: BorrowBookArgs) =
    let handle = Logic.borrowBook args.BorrowerId args.DateTime
    performListingUpdate persistence args.BookListingId handle

let private placeRequestToBorrowHandler (persistence: Persistence) (args: QueueRequestToBorrowArgs) =
    let handle = Logic.placeRequestToBorrow args.RequestedBy args.DateTime
    performListingUpdate persistence args.BookListingId handle

let private returnBookHandler (persistence: Persistence) (args: ReturnBookArgs) =
    let handle = Logic.returnBook args.BorrowerId args.DateTime
    performListingUpdate persistence args.BookListingId handle

let commandHandler (dispatchDomainEvent: DomainEvent -> Async<unit>) (persistence: Persistence) (command: Command) =
    asyncResult {
        let! event =
            match command with
            | PublishBookListing args -> publishBookHandler persistence args
            | QueueRequestToBorrow args -> placeRequestToBorrowHandler persistence args
            | BorrowBook args -> borrowBookHandler persistence args
            | ReturnBook args -> returnBookHandler persistence args
            
        do! dispatchDomainEvent event
    }
