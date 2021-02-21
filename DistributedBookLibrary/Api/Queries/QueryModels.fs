module Api.Queries.QueryModels

open System

type PublishedListingQueryModel = {
    Id: Guid
    Author: string
    Title: string
}

