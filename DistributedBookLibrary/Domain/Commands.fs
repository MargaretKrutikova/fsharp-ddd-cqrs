module Domain.Commands

open System
open Types

type PublishBookListingArgs =
    { Id: ListingId
      OwnerId: UserId
      DateTime: DateTime
      Title: string
      Author: string }

type QueueRequestToBorrowArgs =
    { RequestedBy: UserId
      DateTime: DateTime
      BookListingId: ListingId }

type ReturnBookArgs =
    { BorrowerId: UserId
      DateTime: DateTime
      BookListingId: ListingId }

type BorrowBookArgs =
    { BorrowerId: UserId
      DateTime: DateTime
      BookListingId: ListingId }

type Command =
    | PublishBookListing of PublishBookListingArgs
    | QueueRequestToBorrow of QueueRequestToBorrowArgs
    | BorrowBook of BorrowBookArgs
    | ReturnBook of ReturnBookArgs
