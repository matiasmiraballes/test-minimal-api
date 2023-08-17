FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY . .
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime
WORKDIR /app
COPY --from=build /app/publish /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV IS_GOOGLE_CLOUD=true

ENTRYPOINT ["dotnet", "MyNotesMetrics.dll"]