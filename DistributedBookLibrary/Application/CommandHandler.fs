module Application.CommandHandler

open Domain
open Domain.Commands
open Application.Persistence
open FsToolkit.ErrorHandling

let publishBookHandler (persistence: Persistence) (args: PublishBookListingArgs) =
    asyncResult {
        let listing, event = Logic.publishBookListing args
        do! persistence.AddBook listing
        return event
    }
   
let borrowBookHandler (persistence: Persistence) (args: BorrowBookArgs) =
    asyncResult {
        let! bookListing = persistence.GetBookById args.BookListingId
        let! updatedListing, event =
            bookListing |> Logic.borrowBook args.BorrowerId args.DateTime
        
        do! persistence.UpdateBook updatedListing
        return event
    } 

let placeRequestToBorrowHandler (persistence: Persistence) (args: PlaceRequestToBorrowArgs) =
    asyncResult {
        let! bookListing = persistence.GetBookById args.BookListingId
        let! updatedListing, event =
            bookListing |> Logic.placeRequestToBorrow args.RequestedBy args.DateTime
        
        do! persistence.UpdateBook updatedListing
        return event
    } 

let returnBookHandler (persistence: Persistence) (args: ReturnBookArgs) =
    asyncResult {
        let! bookListing = persistence.GetBookById args.BookListingId
        let! updatedListing, event =
            bookListing |> Logic.returnBook args.BorrowerId args.DateTime
        
        do! persistence.UpdateBook updatedListing
        return event
    } 

let commandHandler (persistence: Persistence) (command: Command) =
    match command with
    | PublishBookListing args -> publishBookHandler persistence args
    | PlaceRequestToBorrow args -> placeRequestToBorrowHandler persistence args
    | BorrowBook args -> borrowBookHandler persistence args
    | ReturnBook args -> returnBookHandler persistence args
