module Application.Persistence

open Domain.Types

type Persistence = {
    GetBookById: ListingId -> Async<BookListing>
    UpdateBook: BookListing -> Async<unit>
    AddBook: BookListing -> Async<unit>
}