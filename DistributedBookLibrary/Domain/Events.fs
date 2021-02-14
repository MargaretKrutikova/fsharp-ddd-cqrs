module Domain.Events

open System
open Types

type BookReturnedArgs =
    { BookId: ListingId
      ReturnedBy: UserId
      DateTime: DateTime }

type BookBorrowedArgs =
    { BookId: ListingId
      BorrowedBy: UserId
      DateTime: DateTime }

type UserAddedToRequestQueueArgs =
    { BookId: ListingId
      UserId: UserId
      DateTime: DateTime }

type BookRequestQueueAdvancedArgs =
    { BookId: ListingId
      ReturnedBy: UserId
      NextBorrowerId: UserId
      DateTime: DateTime }

type DomainEvent =
    | UserAddedToRequestQueue of UserAddedToRequestQueueArgs
    | BookBorrowed of BookBorrowedArgs
    | BookReturned of BookReturnedArgs
    | BookRequestQueueAdvanced of BookRequestQueueAdvancedArgs
