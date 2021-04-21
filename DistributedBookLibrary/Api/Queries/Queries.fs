module Api.Queries.Queries

open System
open Api.Queries.QueryModels

type QueryError =
   | InternalError
   | RecordNotFound
   
type QueryResult<'a> = Async<Result<'a, QueryError>>

type ReadStorage = {
    getPublishedListings: unit -> QueryResult<PublishedListingModel list>
    getUserListings: Guid -> QueryResult<UserListing list>
}
