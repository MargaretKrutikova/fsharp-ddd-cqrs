module Application.Persistence

open Domain.Types

type PersistenceError =
    | BookNotFound
    | ConnectionError
    
type Persistence = {
    GetBookById: ListingId -> Async<Result<BookListing, PersistenceError>>
    UpdateBook: BookListing -> Async<Result<unit, PersistenceError>>
    AddBook: BookListing -> Async<Result<unit, PersistenceError>>
}