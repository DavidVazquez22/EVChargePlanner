FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore EVChargePlanner.Api/EVChargePlanner.Api.csproj
RUN dotnet publish EVChargePlanner.Api/EVChargePlanner.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EVChargePlanner.Api.dll"]