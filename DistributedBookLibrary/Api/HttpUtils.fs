module Api.HttpUtils

open System.Threading.Tasks
open Api.Queries.Queries
open Application.CommandHandler

open Application.Persistence
open Microsoft.AspNetCore.Http
open Giraffe

let private commandErrorToHttpResponse (next: HttpFunc) (ctx: HttpContext) (error: CommandError) =
    match error with
    | Domain _ -> RequestErrors.BAD_REQUEST "Operation can't be performed" next ctx
    | Persistence PersistenceError.BookNotFound -> RequestErrors.NOT_FOUND "Book wasn't found" next ctx
    | Persistence ConnectionError -> RequestErrors.BAD_REQUEST "Service unavailable" next ctx

let private queryErrorToHttpResponse (next: HttpFunc) (ctx: HttpContext) (error: QueryError) =
    match error with
    | InternalError -> RequestErrors.BAD_REQUEST "Service unavailable" next ctx
    | RecordNotFound -> RequestErrors.NOT_FOUND "Object not found" next ctx

let commandToHttpResponse
    (next: HttpFunc) (ctx: HttpContext) (result: Result<unit, CommandError>) : Task<HttpContext option> =
    match result with
    | Ok () -> json () next ctx
    | Error error -> commandErrorToHttpResponse next ctx error

let queryToHttpResponse
    (next: HttpFunc) (ctx: HttpContext) (result: Result<'a, QueryError>) : Task<HttpContext option> =
    match result with
    | Ok data -> json data next ctx
    | Error error -> queryErrorToHttpResponse next ctx error
    