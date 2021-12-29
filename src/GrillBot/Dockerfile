FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS Build

EXPOSE 5000
ENV TZ=Europe/Prague
ENV ASPNETCORE_URLS 'http://+:5000'
ENV DOTNET_PRINT_TELEMETRY_MESSAGE 'false'

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
COPY "GrillBot.Database/" /src/GrillBot.Database
COPY "GrillBot.Data/" /src/GrillBot.Data
COPY "GrillBot.App/" /src/GrillBot.App
RUN mkdir -p /publish
RUN dotnet publish /src/GrillBot.App -c Release -o /publish --no-restore -r linux-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim as FinalImage

# Dependencies
WORKDIR /app
RUN sed -i'.bak' 's/$/ contrib/' /etc/apt/sources.list
RUN apt update && apt install -y --no-install-recommends tzdata ttf-mscorefonts-installer fontconfig libc6-dev libgdiplus libx11-dev
RUN ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll
RUN fc-cache -fv
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

COPY --from=Build /publish .
ENTRYPOINT [ "dotnet", "GrillBot.App" ]
