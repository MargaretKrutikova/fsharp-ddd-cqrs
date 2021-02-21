module Api.CompositionRoot

open Application.CommandHandler
open Domain.Commands
open Domain.Events
open Api.Infrastructure

open FsToolkit.ErrorHandling
open Microsoft.Extensions.Logging

type CompositionRoot = {
    CommandHandler: Command -> Async<Result<unit, CommandError>> 
}

let dispatchDomainEvent (logger: ILogger) (event: DomainEvent): Async<unit> =
    logger.LogInformation(sprintf "Domain event published: %A" event)
    Async.singleton ()

let compose (logger: ILogger) : CompositionRoot =
    let dispatchEvent = dispatchDomainEvent logger
    let persistence = InMemoryPersistence.create ()
    
    { CommandHandler = commandHandler dispatchEvent persistence  }