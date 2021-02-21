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

let assertQueueEntry (requestedBy, requestedDate) (actual: RequestQueueEntry) =
    Assert.Equal(requestedBy, actual.RequestedBy)
    Assert.Equal(requestedDate, actual.RequestedDate)
