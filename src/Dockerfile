FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Run external NuGet source
ARG github_actions_token
RUN dotnet nuget add source https://nuget.pkg.github.com/GrillBot/index.json -n GrillBot -u Misha12 -p "${github_actions_token}" --store-password-in-clear-text

# Common
RUN mkdir -p /src/GrillBot.Common
COPY "GrillBot.Common/GrillBot.Common.csproj" /src/GrillBot.Common
RUN dotnet restore "src/GrillBot.Common/GrillBot.Common.csproj" -r linux-x64

# Cache
RUN mkdir -p /src/GrillBot.Cache
COPY "GrillBot.Cache/GrillBot.Cache.csproj" /src/GrillBot.Cache
RUN dotnet restore "src/GrillBot.Cache/GrillBot.Cache.csproj" -r linux-x64

# Database
RUN mkdir -p /src/GrillBot.Database
COPY "GrillBot.Database/GrillBot.Database.csproj" /src/GrillBot.Database
RUN dotnet restore "src/GrillBot.Database/GrillBot.Database.csproj" -r linux-x64

# Data
RUN mkdir -p /src/GrillBot.Data
COPY "GrillBot.Data/GrillBot.Data.csproj" /src/GrillBot.Data
RUN dotnet restore "src/GrillBot.Data/GrillBot.Data.csproj" -r linux-x64

# App
RUN mkdir -p /src/GrillBot.App
COPY "GrillBot.App/GrillBot.App.csproj" /src/GrillBot.App
RUN dotnet restore "src/GrillBot.App/GrillBot.App.csproj" -r linux-x64

# Publish
COPY "GrillBot.Common/" /src/GrillBot.Common
COPY "GrillBot.Cache/" /src/GrillBot.Cache
COPY "GrillBot.Database/" /src/GrillBot.Database
COPY "GrillBot.Data/" /src/GrillBot.Data
COPY "GrillBot.App/" /src/GrillBot.App
RUN mkdir -p /publish
RUN dotnet publish /src/GrillBot.App -c Release -o /publish --no-restore -r linux-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS FinalImage
LABEL org.opencontainers.image.source=https://github.com/grillbot/grillbot

EXPOSE 5000
ENV TZ=Europe/Prague
ENV ASPNETCORE_URLS='http://+:5000'
ENV DOTNET_PRINT_TELEMETRY_MESSAGE='false'

# Dependencies
WORKDIR /app
RUN apt update && apt install -y --no-install-recommends tzdata libc6-dev
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

COPY --from=build /publish .
ENTRYPOINT [ "dotnet", "GrillBot.App.dll" ]
