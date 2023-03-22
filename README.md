# GrillBot

GrillBot is Discord bot for fun and management VUT FIT discord server.

## Requirements

- [PostgreSQL](https://www.postgresql.org/) server (minimal recommended version is 13)
- [.NET 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) (with ASP.NET Core 7)

If you're running bot on Linux distributions, you have to install these packages: `tzdata`, `ttf-mscorefonts-installer`, `fontconfig`, `libc6-dev`, `libgdiplus` and `libx11-dev`.

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
- `Discord:Token` - Discord authentication token.
- `OAuth2:ClientId`, `OAuth2:ClientSecret` - Client ID and secret for login to administrations.

*Without these settings the bot will not run.*

Recommended environment variables:

- `Discord:Logging:GuildId`, `Discord:Logging:ChannelId` - Guild and channel specification for notifications on errors to channel.
- `Birthday:Notifications:GuildId`, `Birthday:Notifications:ChannelId` - Guild and channel specification for notifications of birthdays.
- `Services:Graphics:Api` - Base URL to the [graphics](https://github.com/GrillBot/GrillBot.Services/tree/master/src/Graphics) microservice.
- `Services:RubbergodService:Api` - Base URL to the [rubbergod](https://github.com/GrillBot/GrillBot.Services/tree/master/src/RubbergodService) microservice.
- `Services:FileService:Api` - Base URL to the [file](https://github.com/GrillBot/GrillBot.Services/tree/master/src/FileService) microservice.

*If you're using Docker instance, bind `/GrillBotData` directory as volume.*

## Docker

Latest docker image is published in [GitHub container registry](https://github.com/orgs/GrillBot/packages).

## Licence

GrillBot is licensed as All Rights Reserved. The source code is available for reading and contribution. Owner consent is required for use in a production environment.
