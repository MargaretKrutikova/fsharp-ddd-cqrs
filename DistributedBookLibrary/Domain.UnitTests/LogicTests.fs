module Tests

open System
open Domain.Types
open Domain.Logic
open Domain.UnitTests
open Xunit
open Helpers

[<Fact>]
let ``A published book automatically becomes available`` () =
    let listing = createPublishBookArgs () |> publishBookListing
    Assert.Equal(Available, listing.Status)

[<Fact>]
let ``Users can borrow available published books`` () =
    let (borrowerId, borrowedAt) = createBorrowRequest "2020-02-03"
    let borrowedListing =
        createPublishBookArgs ()
        |> publishBookListing 
        |> borrowBook borrowerId borrowedAt
        |> mapFst
        |> unwrap
    
    let expectedStatus = {
        BorrowedBy = borrowerId
        BorrowedAt = borrowedAt
        RequestToBorrowQueue = BorrowedStatusDetails.createEmptyQueue ()
    }
    Assert.Equal(Borrowed expectedStatus, borrowedListing.Status)

[<Fact>]
let ``Users' requests to borrow are added to the queue in the right order`` () =
    let borrowRequest = createBorrowRequest "2020-02-03"

    let queueRequest1 = createBorrowRequest "2020-02-04"
    let queueRequest2 = createBorrowRequest "2020-02-05"
    let queueRequest3 = createBorrowRequest "2020-02-07"

    let updatedListing =
        createPublishBookArgs ()
        |> publishBookListing 
        |> (borrowRequest ||> borrowBook)
        |> bindFst (queueRequest1 ||> placeRequestToBorrow)
        |> bindFst (queueRequest2 ||> placeRequestToBorrow)
        |> bindFst (queueRequest3 ||> placeRequestToBorrow)
        |> mapFst
        |> unwrap
    
    let assertRequestQueue (queue: RequestToBorrowQueue) =
        Assert.True(queue.Length = 3)
        assertQueueEntry queueRequest1 queue.[0]
        assertQueueEntry queueRequest2 queue.[1]
        assertQueueEntry queueRequest3 queue.[2]
    
    match updatedListing.Status with
    | Borrowed details ->
        Assert.Equal(borrowRequest |> fst, details.BorrowedBy)
        Assert.Equal(borrowRequest |> snd, details.BorrowedAt)
        assertRequestQueue details.RequestToBorrowQueue
    | other -> Assert.True (false, sprintf "Incorrect status %A" other)

[<Fact>]
let ``First user in request queue will automatically borrow book when it is returned by the borrower`` () =
    let borrowerId = Guid.NewGuid () |> UserId
    let borrowedDate = DateTime.Parse "2020-02-03"

    let requesterId = Guid.NewGuid () |> UserId
    let requestedDate = DateTime.Parse "2020-02-04"
    let returnDate = DateTime.Parse "2020-02-06"
    
    let updatedListing =
        createPublishBookArgs ()
        |> publishBookListing
        |> borrowBook borrowerId borrowedDate
        |> bindFst (placeRequestToBorrow requesterId requestedDate)
        |> bindFst (returnBook borrowerId returnDate)
        |> mapFst
        |> unwrap
    
    let expectedStatus = {
        BorrowedBy = requesterId
        BorrowedAt = returnDate
        RequestToBorrowQueue = BorrowedStatusDetails.createEmptyQueue ()
    }
    Assert.Equal(Borrowed expectedStatus, updatedListing.Status)
