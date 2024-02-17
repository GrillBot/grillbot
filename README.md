# GrillBot

GrillBot is Discord bot for fun and management VUT FIT discord server.

## Requirements

- [PostgreSQL](https://www.postgresql.org/) server (minimal recommended version is 13) [Docker image](https://hub.docker.com/_/postgres)
- [RabbitMQ](https://rabbitmq.com/) (minimal recommended version is 2) [Docker image](https://hub.docker.com/_/rabbitmq)
- [.NET 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) (with ASP.NET Core 7)
- Registered Microsoft Azure account with purchased Storage account or Storage account emulator.

If you're running bot on Linux distributions, you have to install these packages: `tzdata` and `libc6-dev`.

Only debian based distros are tested. Funcionality cannot be guaranteed for other distributions.

### Development requirements

- Microsoft Visual Studio 2022, JetBrains Rider (or another IDE supports .NET)
- [dotnet-ef](https://docs.microsoft.com/cs-cz/ef/core/cli/dotnet) utility (for code first migrations)
- Generate [personal access token (Classic)](https://docs.github.com/en/enterprise-server@3.4/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token) with `read:packages` permission.
- Add new NuGet source with URL `https://nuget.pkg.github.com/GrillBot/index.json`.
  - You can do it from your IDE or via CLI `dotnet nuget add source https://nuget.pkg.github.com/GrillBot/index.json -n GrillBot -u {Username} -p {PersonalAccessToken}`
  - On Linux systems add to previous command parameter `--store-password-in-clear-text`.

## Configuration

If you starting bot in development environment (require environment variable `ASPNETCORE_ENVIRONMENT=Development`), you have to fill `appsettings.Development.json`.

If you starting bot in production environment (docker recommended), you have to configure environment variables.

Mandatory environment variables:

- `ConnectionStrings:Default` - Connection string to database.
- `ConnectionStrings:Cache` - Connection string to cache database.
- `ConnectionStrings:StorageAccount` - Connection string to Azure Storage Account or Storage Account emulator.
- `Discord:Token` - Discord authentication token.
- `Auth:OAuth2:ClientId`, `Auth:OAuth2:ClientSecret` - Client ID and secret for login to administrations.
- `RabbitMQ:Hostname`, `RabbitMQ:Username`, `RabbitMQ:Password` - Credentials for your RabbitMQ instance.

_Without these settings the bot will not run._

Recommended environment variables:

- `Discord:Logging:GuildId`, `Discord:Logging:ChannelId` - Guild and channel specification for notifications on errors to channel.
- `Birthday:Notifications:GuildId`, `Birthday:Notifications:ChannelId` - Guild and channel specification for notifications of birthdays.

_If you're using Docker instance, bind `/GrillBotData` directory as volume._

_If you're not have access to the GrillBot development environment, ask [Hobit](https://hobiiitt.carrd.co/) for access or deploy your instances of [microservices](https://github.com/grillbot/grillbot.services), RabbitMQ and Postgres._

## Docker

Latest docker image is published in [GitHub container registry](https://github.com/orgs/GrillBot/packages).

## Licence

GrillBot is licensed as All Rights Reserved. The source code is available for reading and contribution. Owner consent is required for use in a production environment.
