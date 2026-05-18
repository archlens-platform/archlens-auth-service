FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY archlens-contracts/Directory.Build.props ./archlens-contracts/
COPY archlens-contracts/src/ArchLens.SharedKernel/*.csproj ./archlens-contracts/src/ArchLens.SharedKernel/
COPY archlens-contracts/src/ArchLens.Contracts/*.csproj ./archlens-contracts/src/ArchLens.Contracts/

COPY archlens-auth-service/*.sln ./archlens-auth-service/
COPY archlens-auth-service/Directory.Build.props ./archlens-auth-service/
COPY archlens-auth-service/src/ArchLens.Auth.Api/*.csproj ./archlens-auth-service/src/ArchLens.Auth.Api/
COPY archlens-auth-service/src/ArchLens.Auth.Application/*.csproj ./archlens-auth-service/src/ArchLens.Auth.Application/
COPY archlens-auth-service/src/ArchLens.Auth.Application.Contracts/*.csproj ./archlens-auth-service/src/ArchLens.Auth.Application.Contracts/
COPY archlens-auth-service/src/ArchLens.Auth.Domain/*.csproj ./archlens-auth-service/src/ArchLens.Auth.Domain/
COPY archlens-auth-service/src/ArchLens.Auth.Infrastructure/*.csproj ./archlens-auth-service/src/ArchLens.Auth.Infrastructure/

WORKDIR /src/archlens-auth-service
RUN dotnet restore src/ArchLens.Auth.Api/ArchLens.Auth.Api.csproj

WORKDIR /src
COPY archlens-contracts/ ./archlens-contracts/
COPY archlens-auth-service/ ./archlens-auth-service/

WORKDIR /src/archlens-auth-service
RUN dotnet publish src/ArchLens.Auth.Api -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
LABEL org.opencontainers.image.source="https://github.com/archlens-platform/archlens-auth-service"
LABEL org.opencontainers.image.title="ArchLens Auth Service"
LABEL org.opencontainers.image.version="1.0.0"
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

USER $APP_UID
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ArchLens.Auth.Api.dll"]
