module Api.ApiModels

open System

[<CLIMutable>]
type PublishListingInputModel = {
    UserId: Guid
    Author: string
    Title: string
}

[<CLIMutable>]
type BorrowListingInputModel = {
    BorrowerId: Guid
    ListingId: Guid
}

[<CLIMutable>]
type ReturnListingInputModel = {
    BorrowerId: Guid
    ListingId: Guid
}

[<CLIMutable>]
type QueueRequestToBorrowInputModel = {
    BorrowerId: Guid
    ListingId: Guid
}