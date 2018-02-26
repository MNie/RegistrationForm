namespace Registration.Api.Infrastructure.Mail

open Registration.Api.Models
open Registration.Api
open Registration.Api.Infrastructure.Db
open Microsoft.Extensions.Configuration
open System.Net.Mail
open System

type Mail = 
    {
        user: string
        pass: string
        name: string
        ifFailedSendTo: string
        server: string
        port: int
    }

type MailInfo = 
    | Checked of CheckedParticipant 
    | Invalid of InvalidRegistration

module MailSender =
    let createSuccessMessage (mailConf: Mail) (info: CheckedParticipant) =
            let msg =
                new MailMessage(
                    mailConf.user,
                    info.email,
                    "Conference ticket confirmation",
                    sprintf "Hi %s %s\r\n we confirm that you granted a ticket for fantastic Registration Conference\r\nBest regards %s" info.name info.surname mailConf.name
                )
            msg.IsBodyHtml <- true
            msg

    let createFailedMessage (mailConf: Mail) (info: InvalidRegistration) =
            let msg =
                new MailMessage(
                    mailConf.user,
                    mailConf.user,
                    "Conference ticket confirmation - Failed",
                    sprintf "Confirmation for user: %s %s with email: %s failed because of: %s" info.name info.surname info.mail info.cause
                )
            msg.IsBodyHtml <- true
            msg

    let private prepareAndSend (mail: Mail) (req: MailInfo) =
        let msg = 
            match req with
            | Checked c -> createSuccessMessage mail c
            | Invalid i -> createFailedMessage mail i
            
        let client = new SmtpClient(mail.server, mail.port)
        client.EnableSsl <- true
        client.Credentials <- System.Net.NetworkCredential(mail.user, mail.pass)
        client.SendCompleted |> Observable.add(fun e ->
            let msg = e.UserState :?> MailMessage
            if e.Cancelled then
                ("Mail message cancelled:\r\n" + msg.Subject) |> Console.WriteLine
            if isNull(e.Error) then
                ("Sending mail failed for message:\r\n" + msg.Subject + ", reason:\r\n" + e.Error.ToString())
                |> Console.WriteLine
            if msg<>Unchecked.defaultof<MailMessage> then msg.Dispose()
            if client<>Unchecked.defaultof<SmtpClient> then client.Dispose()
        )
        client.SendAsync(msg, msg)
        "send mail to participant and office" |> ignore
    
    let private success mail req = prepareAndSend mail req
    
    let private failed mail req = prepareAndSend mail req

    let send(req: Result<CheckedParticipant, InvalidRegistration>): Result<string, string> =
        let mail = Configuration.conf.GetValue<Mail>("Mail")
        match req with
        | Ok v -> 
            success mail (Checked v)
            Ok v.email
        | Error f -> 
            failed mail (Invalid f)
            Error ""