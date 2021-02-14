module Domain.Types

open System

type UserId = UserId of Guid
type ListingId = ListingId of Guid

type RequestQueueEntry =
    { RequestedBy: UserId
      RequestedDate: DateTime }

type RequestToBorrowQueue = RequestQueueEntry list

type BorrowedStatusDetails =
    { BorrowedBy: UserId
      BorrowedAt: DateTime
      RequestToBorrowQueue: RequestToBorrowQueue }

type ListingStatus =
    | Available
    | Borrowed of BorrowedStatusDetails

type BookListing =
    { Id: ListingId
      OwnerId: UserId
      Author: string
      Title: string
      Status: ListingStatus }

type DomainError =
    | BookHasNoRequestQueue of ListingId
    | UserCantPlaceRequestToBorrow of ListingId * UserId
    | BookIsNotBorrowed of ListingId
    | BookIsAlreadyBorrowed of ListingId
    | BookIsNotBorrowedByUser of ListingId * UserId