module Api.ApiHandlers

open System
open System.Threading.Tasks
open Api.ApiModels
open Api.Queries.Queries
open Application.CommandHandler
open Domain.Commands
open Domain.Types
open CompositionRoot

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open Giraffe

let private commandErrorToHttpResponse (next: HttpFunc) (ctx: HttpContext) (error: CommandError) =
    match error with
    | Domain _ -> RequestErrors.BAD_REQUEST "" next ctx
    | Persistence _ -> RequestErrors.BAD_REQUEST "Please try again later" next ctx

let private queryErrorToHttpResponse (next: HttpFunc) (ctx: HttpContext) (error: QueryError) =
    match error with
    | InternalError -> RequestErrors.BAD_REQUEST "Please try again later" next ctx
    | RecordNotFound -> RequestErrors.NOT_FOUND "Object not found" next ctx

let private commandToHttpResponse
    (next: HttpFunc) (ctx: HttpContext) (result: Result<unit, CommandError>) : Task<HttpContext option> =
    match result with
    | Ok () -> json () next ctx
    | Error error -> commandErrorToHttpResponse next ctx error

let private queryToHttpResponse
    (next: HttpFunc) (ctx: HttpContext) (result: Result<'a, QueryError>) : Task<HttpContext option> =
    match result with
    | Ok data -> json data next ctx
    | Error error -> queryErrorToHttpResponse next ctx error

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
        return! commandToHttpResponse next ctx result
    }

let getPublishedListings (next: HttpFunc) (ctx: HttpContext) =
    task {
        let compositionRoot = ctx.GetService<CompositionRoot>()
        let! result = compositionRoot.ReadStorage.getPublishedListings ()
        
        return! queryToHttpResponse next ctx result
    }