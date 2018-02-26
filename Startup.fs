namespace Registration.Api

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

module Configuration =
    let private buildConf(): IConfigurationRoot = 
        ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
            .AddJsonFile(sprintf "appsettings.json", optional = true)
            .AddEnvironmentVariables()
            .Build()

    let mutable private _conf: IConfigurationRoot = null

    let conf: IConfigurationRoot =
        match _conf with
        | null -> 
            _conf <- buildConf()
            _conf
        | _ -> _conf

type Startup private() =
    let mutable configuration: IConfigurationRoot = null
    member this.Configuration
        with get() = configuration
        and private set (value) =
            configuration <- value

    new (env: IHostingEnvironment) as this =
        Startup() then
        let builder = 
            ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
                .AddJsonFile(sprintf "appsettings.%s.json" env.EnvironmentName, optional = true)
                .AddEnvironmentVariables()
        this.Configuration <- builder.Build()

    member this.ConfigureServices(services: IServiceCollection) =
        services.AddMvc() |> ignore

    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment, loggerFactory: ILoggerFactory) =
        loggerFactory.AddConsole(this.Configuration.GetSection("Logging"))
        |> fun x -> x.AddDebug()
        |> ignore

        app.UseMvc() |> ignore
            