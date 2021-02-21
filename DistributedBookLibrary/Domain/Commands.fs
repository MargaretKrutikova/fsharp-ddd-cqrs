module Domain.Commands

open System
open Types

type PublishBookListingArgs =
    { Id: ListingId
      OwnerId: UserId
      DateTime: DateTime
      Title: string
      Author: string }

type PlaceRequestToBorrowArgs =
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
    | PlaceRequestToBorrow of PlaceRequestToBorrowArgs
    | BorrowBook of ReturnBookArgs
    | ReturnBook of BorrowBookArgs
