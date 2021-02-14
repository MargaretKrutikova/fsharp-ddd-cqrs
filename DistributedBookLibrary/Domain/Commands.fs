module Domain.Commands

open System
open Types

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
    | PlaceRequestToBorrow of PlaceRequestToBorrowArgs
    | BorrowBook of ReturnBookArgs
    | ReturnBook of BorrowBookArgs