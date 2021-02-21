module Api.Infrastructure.InMemoryPersistence

open Application.Persistence
open Domain.Types
open FsToolkit.ErrorHandling

let create (): Persistence =
    let mutable listings: BookListing list = List.empty

    let getBookById id =
        listings
        |> List.tryFind (fun l -> l.Id = id)
        |> Result.requireSome PersistenceError.BookNotFound
        |> Async.singleton

    let updateBook book =
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
