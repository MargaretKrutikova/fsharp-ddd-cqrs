module Api.ApiModels

open System

[<CLIMutable>]
type PublishListingInputModel = {
    UserId: Guid
    Author: string
    Title: string
}
