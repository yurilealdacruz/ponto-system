FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Ponto.Api/Ponto.Api.csproj", "src/Ponto.Api/"]
COPY ["src/Ponto.Data/Ponto.Data.csproj", "src/Ponto.Data/"]
COPY ["src/Ponto.Domain/Ponto.Domain.csproj", "src/Ponto.Domain/"]

RUN dotnet restore "src/Ponto.Api/Ponto.Api.csproj"

COPY . .

WORKDIR "/src/src/Ponto.Api"

RUN dotnet build "Ponto.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Ponto.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Ponto.Api.dll"]
