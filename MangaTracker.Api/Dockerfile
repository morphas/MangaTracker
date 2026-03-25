# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MangaTracker.Api.csproj", "."]
RUN dotnet restore "MangaTracker.Api.csproj"

COPY . .
RUN dotnet build "MangaTracker.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "MangaTracker.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MangaTracker.Api.dll"]
