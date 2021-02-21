module LogicTests

open System
open Domain.Types
open Domain.Logic
open Domain.UnitTests
open Xunit
open Helpers

[<Fact>]
let ``A published book automatically becomes available`` () =
    // Arrange
    let args = createPublishBookArgs ()
    
    // Act
    let listing = publishBookListing args
    
    // Assert
    Assert.Equal(Available, listing.Status)

[<Fact>]
let ``Users can borrow available published books`` () =
    // Arrange
    let (borrowerId, borrowedAt) = createBorrowRequest "2020-02-03"
    
    // Act
    let borrowedListing =
        createPublishBookArgs ()
        |> publishBookListing 
        |> borrowBook borrowerId borrowedAt
        |> mapFst
        |> unwrap
    
    // Assert
    let expectedStatus = {
        BorrowedBy = borrowerId
        BorrowedAt = borrowedAt
        RequestToBorrowQueue = BorrowedStatusDetails.createEmptyQueue ()
    }
    Assert.Equal(Borrowed expectedStatus, borrowedListing.Status)

[<Fact>]
let ``First user in request queue will automatically borrow book when it is returned by the borrower`` () =
    // Arrange
    let borrowerId, borrowedDate = createBorrowRequest "2020-02-03"
    let requesterId, requestedDate = createBorrowRequest "2020-02-04"
    let returnDate = DateTime.Parse "2020-02-06"
    
    // Act
    let updatedListing =
        createPublishBookArgs ()
        |> publishBookListing
        |> borrowBook borrowerId borrowedDate
        |> bindFst (placeRequestToBorrow requesterId requestedDate)
        |> bindFst (returnBook borrowerId returnDate)
        |> mapFst
        |> unwrap
    
    // Assert
    let expectedStatus = {
        BorrowedBy = requesterId
        BorrowedAt = returnDate
        RequestToBorrowQueue = BorrowedStatusDetails.createEmptyQueue ()
    }
    Assert.Equal(Borrowed expectedStatus, updatedListing.Status)


[<Fact>]
let ``Queue with requests to borrow works in first-in-first-out order`` () =
    // Arrange
    let borrowerId, borrowedAt = createBorrowRequest "2020-02-03"

    let queueRequest1 = createBorrowRequest "2020-02-04"
    let queueRequest2 = createBorrowRequest "2020-02-05"
    let queueRequest3 = createBorrowRequest "2020-02-07"

    let returnDate1 = DateTime.Parse "2020-02-08"
    let returnDate2 = DateTime.Parse "2020-02-10"
    let returnDate3 = DateTime.Parse "2020-02-11"

    // Act
    let updatedListing =
        createPublishBookArgs ()
        |> publishBookListing 
        |> borrowBook borrowerId borrowedAt
        |> bindFst (queueRequest1 ||> placeRequestToBorrow)
        |> bindFst (queueRequest2 ||> placeRequestToBorrow)
        |> bindFst (queueRequest3 ||> placeRequestToBorrow)
        |> mapFst
        |> unwrap
    
    // Assert
    let requester1, _ = queueRequest1
    let requester2, _ = queueRequest2
    let requester3, _ = queueRequest3

    updatedListing
    |> returnBookAndAssert borrowerId returnDate1 requester1
    |> returnBookAndAssert requester1 returnDate2 requester2
    |> returnBookAndAssert requester2 returnDate3 requester3

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