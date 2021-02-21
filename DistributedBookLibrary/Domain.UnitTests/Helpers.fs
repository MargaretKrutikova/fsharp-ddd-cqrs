module Domain.UnitTests.Helpers

open System
open Domain
open Domain.Commands
open Domain.Types
open Xunit

let mapFst value = Result.map fst value
let bindFst fn value = value |> mapFst |> Result.bind fn

let createUserId () = Guid.NewGuid() |> UserId

let createBorrowRequest (date: string) = createUserId (), DateTime.Parse date

let unwrap =
    function
    | Ok value -> value
    | Error err -> failwithf "%A" err

let createPublishBookArgs (): PublishBookListingArgs =
    let listingId = Guid.NewGuid() |> ListingId
    let ownerId = Guid.NewGuid() |> UserId
    let publishedDate = DateTime.Parse "2020-02-02"

    { Id = listingId
      OwnerId = ownerId
      DateTime = publishedDate
      Title = "test"
      Author = "test" }

let assertBorrowStatus borrowerId borrowedAt (status: ListingStatus) =
    match status with
    | Borrowed actual ->
        Assert.Equal(borrowerId, actual.BorrowedBy)
        Assert.Equal(borrowedAt, actual.BorrowedAt)
    | other ->
        Assert.True(false, sprintf "Received wrong listing status %A" other)

let returnBookAndAssert borrowerId date nextBorrowerId listing =
    let returnedListing =
        listing
        |> Logic.returnBook borrowerId date
        |> mapFst
        |> unwrap
        
    assertBorrowStatus nextBorrowerId date returnedListing.Status
    
    returnedListing
