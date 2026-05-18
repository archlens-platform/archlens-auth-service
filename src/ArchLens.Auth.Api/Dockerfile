FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY src/SharedKernel/ src/SharedKernel/
COPY src/Services/Auth/ src/Services/Auth/

RUN dotnet restore src/Services/Auth/ArchLens.Auth.Api/ArchLens.Auth.Api.csproj
RUN dotnet publish src/Services/Auth/ArchLens.Auth.Api/ArchLens.Auth.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "ArchLens.Auth.Api.dll"]
