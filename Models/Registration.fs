namespace Registration.Api.Models

open System

type Registration =
    {
        id: int
        mail: string
        name: string
        surname: string
        fromWhom: string
        occupation: string
        company: string
        mealPreference: int
        specialMealPreference: string
    }

type InvalidRegistration =
    {
        mail: string
        name: string
        surname: string
        cause: string
    }

type UnvalidatedRegistration = Registration
type ValidatedRegistration = Registration

type MailValidation =
    | Correct of string
    | InCorrect of string

module MailValidation =
    let validate(mail): MailValidation =
        let regex = Text.RegularExpressions.Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*")
        match regex.IsMatch mail with
        | true -> Correct(mail)
        | _ -> InCorrect("mail is invalid")

module Registration =
    let private create(mail, name, surname, fromWhom, occupation, company, mealPreference, specialMealPreference): ValidatedRegistration=
        { id = 0; mail = mail; name = name; surname = surname; fromWhom = fromWhom; occupation = occupation; company = company; mealPreference = mealPreference; specialMealPreference = specialMealPreference }
    let validate(reg: UnvalidatedRegistration): Result<ValidatedRegistration, InvalidRegistration> =
        let mail = MailValidation.validate reg.mail
        match mail with
        | Correct m -> Ok(create(m, reg.name, reg.surname, reg.fromWhom, reg.occupation, reg.company, reg.mealPreference, reg.specialMealPreference))
        | _ -> 
            let cause = sprintf "mail %s for user: %s %s is invalid" reg.mail reg.name reg.surname
            Error {mail = reg.mail; name = reg.name; surname = reg.surname; cause = cause}