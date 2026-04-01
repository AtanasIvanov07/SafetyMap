# =============================================================================
# SafetyMap Dockerfile — Multi-stage build for ASP.NET Core 8.0
# =============================================================================

# ---------------------
# Stage 1: Build
# ---------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and all project files first (better layer caching)
COPY SafetyMap.sln ./
COPY SafetyMapWeb/SafetyMapWeb.csproj SafetyMapWeb/
COPY SafetyMapData/SafetyMapData.csproj SafetyMapData/
COPY SafetyMap.Core/SafetyMap.Core.csproj SafetyMap.Core/
COPY SafetyMap.Core.Tests/SafetyMap.Core.Tests.csproj SafetyMap.Core.Tests/
COPY SafetyMapWeb.Tests/SafetyMapWeb.Tests.csproj SafetyMapWeb.Tests/

# Restore NuGet packages (cached unless .csproj files change)
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish SafetyMapWeb/SafetyMapWeb.csproj -c Release -o /app/publish --no-restore

# ---------------------
# Stage 2: Runtime
# ---------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port 8080 (ASP.NET Core default in containers)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the app
ENTRYPOINT ["dotnet", "SafetyMapWeb.dll"]
