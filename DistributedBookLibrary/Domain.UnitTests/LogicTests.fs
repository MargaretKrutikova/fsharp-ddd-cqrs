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
let ``Users already in queue can't place borrow requests for the same listing`` () =
    // Arrange
    let borrowerId, borrowedDate = createBorrowRequest "2020-02-03"
    let requesterId, requestedAt1 = createBorrowRequest "2020-02-04"
    let requestedAt2 = DateTime.Parse "2020-02-06"
    
    let listing = createPublishBookArgs () |> publishBookListing
    
    // Act
    let result =
        listing
        |> borrowBook borrowerId borrowedDate
        |> bindFst (placeRequestToBorrow requesterId requestedAt1)
        |> bindFst (placeRequestToBorrow requesterId requestedAt2)
        |> mapFst
    
    // Assert
    let expectedError = UserCantPlaceRequestToBorrow (listing.Id, requesterId)
    Assert.Equal(Error expectedError, result)

[<Fact>]
let ``Users can't borrow already borrowed books`` () =
    // Arrange
    let borrowRequest1 = createBorrowRequest "2020-02-03"
    let borrowRequest2 = createBorrowRequest "2020-02-04"
    
    let listing = createPublishBookArgs () |> publishBookListing
    
    // Act
    let result =
        listing
        |> (borrowRequest1 ||> borrowBook)
        |> bindFst (borrowRequest2 ||> borrowBook)
        |> mapFst
    

    // Assert
    let expectedError = BookIsAlreadyBorrowed listing.Id
    Assert.Equal(Error expectedError, result)