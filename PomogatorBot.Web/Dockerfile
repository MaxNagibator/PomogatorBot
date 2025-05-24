FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PomogatorBot.Web/PomogatorBot.Web.csproj", "PomogatorBot.Web/"]
RUN dotnet restore "./PomogatorBot.Web/PomogatorBot.Web.csproj"
COPY . .
WORKDIR "/src/PomogatorBot.Web"
RUN dotnet build "./PomogatorBot.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PomogatorBot.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PomogatorBot.Web.dll"]
