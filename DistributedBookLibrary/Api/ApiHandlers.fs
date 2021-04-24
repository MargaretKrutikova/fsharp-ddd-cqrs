module Api.ApiHandlers

open System
open Api.ApiModels
open Api.Queries.Queries
open Domain.Commands
open Domain.Types
open CompositionRoot

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open Giraffe

let handlePublishListing (next: HttpFunc) (ctx: HttpContext) =
    task {
        let compositionRoot = ctx.GetService<CompositionRoot>()
        let! listingModel = ctx.BindJsonAsync<PublishListingInputModel>()

        let listingId = Guid.NewGuid()

        let command =
            { Id = listingId |> ListingId
              OwnerId = listingModel.UserId |> UserId
              Title = listingModel.Title
              Author = listingModel.Author
              DateTime = DateTime.Now }
            |> PublishBookListing

        let! result = compositionRoot.CommandHandler command
        return! HttpUtils.commandToHttpResponse next ctx result
    }

let getPublishedListings (next: HttpFunc) (ctx: HttpContext) =
    task {
        let compositionRoot = ctx.GetService<CompositionRoot>()
        let! result = compositionRoot.ReadStorage.getPublishedListings ()
        
        return! HttpUtils.queryToHttpResponse next ctx result
    }
    
let getUserListings userId =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let compositionRoot = ctx.GetService<CompositionRoot>()
            let! result = compositionRoot.ReadStorage.getUserListings userId
            
            return! HttpUtils.queryToHttpResponse next ctx result
        }