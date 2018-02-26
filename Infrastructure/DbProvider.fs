namespace Registration.Api.Infrastructure.Db

open Registration.Api.Models
open Registration.Api
open Microsoft.Extensions.Configuration
open Dapper
open System.Data.SqlClient

type Participant =
    {
        email: string
        name: string
        surname: string
    }

type CheckedParticipant = Participant
type FailedParticipant = Participant

module DbProvider =
    let private insertToDb reg =
        use con = new SqlConnection(Configuration.conf.GetValue<string>("db"))
        (
            use tx = con.BeginTransaction()
            (
                let sql = 
                    "insert into Registration(mail, name, surname, fromWhom, occupation, company, mealPreference, specialMealPreference)" +
                    "values (@mail, @name, @surname, @fromWhom, @occupation, @company, @mealPreference, @specialMealPreference)"
                con.Execute(sql, reg, tx) |> ignore
                tx.Commit()
            )
        )        

    let insert(reg: ValidatedRegistration): Result<CheckedParticipant, InvalidRegistration> =
        try
            insertToDb(reg)
            Ok {email = reg.mail; name = reg.name; surname = reg.surname}
        with
            | _ -> Error {mail = reg.mail; name = reg.name; surname = reg.surname; cause = "User alreary registered for a conference."}