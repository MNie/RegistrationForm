namespace Registration.Api.Controllers

open Microsoft.AspNetCore.Mvc
open Registration.Api.Models
open Registration.Api.Infrastructure.Db
open Registration.Api.Infrastructure.Mail

[<Route("api/[controller]")>]
type RegistrationController() =
    inherit Controller()

    [<HttpPost>]
    [<Route("/Register")>]
    member this.Register([<FromBody>] reg: UnvalidatedRegistration) =
        Registration.validate reg
        |> Result.bind DbProvider.insert
        |> MailSender.send