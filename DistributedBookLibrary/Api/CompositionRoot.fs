module Api.CompositionRoot

open Api.Queries.Queries
open Application.CommandHandler
open Domain.Commands
open Domain.Events
open Api.Infrastructure

open FsToolkit.ErrorHandling
open Microsoft.Extensions.Logging

type CompositionRoot =
    { CommandHandler: Command -> Async<Result<unit, CommandError>>
      ReadStorage: ReadStorage }

let dispatchDomainEvent (logger: ILogger) (event: DomainEvent list): Async<unit> =
    logger.LogInformation(sprintf "Domain events published: %A" event)
    Async.singleton ()

let compose (logger: ILogger): CompositionRoot =
    let dispatchEvent = dispatchDomainEvent logger
    let persistence, readStorage = InMemoryPersistence.create ()

    { CommandHandler = commandHandler dispatchEvent persistence
      ReadStorage = readStorage }
