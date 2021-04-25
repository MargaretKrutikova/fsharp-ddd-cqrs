module Api.Queries.QueryModels

open System

type BorrowedStatusModel =
    { BorrowedByUserName: string
      NumberOfUsersInQueue: int }

type ListingStatusModel =
    | Available
    | Borrowed of BorrowedStatusModel

type PublishedListingModel =
    { Id: Guid
      Author: string
      Title: string
      OwnerUserName: string
      Status: ListingStatusModel }

type UserListingType =
    | BorrowedByUser
    | WaitingInQueue
    | PublishedByUser

type UserListing =
    { ListingId: Guid
      Author: string
      Title: string
      Type: UserListingType }
