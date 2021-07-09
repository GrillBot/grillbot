FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim as base

RUN sed -i'.bak' 's/$/ contrib/' /etc/apt/sources.list
RUN apt-get update && apt-get -y install tzdata ttf-mscorefonts-installer fontconfig libc6-dev libgdiplus libx11-dev && apt-get clean
RUN ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll
RUN fc-cache -fv

ENV DOTNET_PRINT_TELEMETRY_MESSAGE 'false'
ENV TZ=Europe/Prague

EXPOSE 80
VOLUME [ "/data" ]

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /source
COPY GrillBot.App/*.csproj GrillBot.App/
COPY GrillBot.Data/*.csproj GrillBot.Data/
COPY GrillBot.Database/*.csproj GrillBot.Database/
RUN dotnet restore GrillBot.Database/ -r linux-x64
RUN dotnet restore GrillBot.Data/ -r linux-x64
RUN dotnet restore GrillBot.App/ -r linux-x64
COPY . .
RUN dotnet publish GrillBot.App/ -c release -o /app -r linux-x64 --self-contained false

FROM base AS final
WORKDIR /app
COPY --from=build /app .
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

ENTRYPOINT ["./GrillBot.App"]
