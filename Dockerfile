# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY TeamsForge.sln ./

# Copy project file
COPY TeamsForgeAPI/TeamsForgeAPI.csproj TeamsForgeAPI/

# Restore dependencies
RUN dotnet restore TeamsForgeAPI/TeamsForgeAPI.csproj

# Copy the rest of the source code
COPY TeamsForgeAPI/ TeamsForgeAPI/

# Build the application
WORKDIR /src/TeamsForgeAPI
RUN dotnet build TeamsForgeAPI.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish TeamsForgeAPI.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Expose ports
EXPOSE 5002
EXPOSE 7256

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5002
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "TeamsForgeAPI.dll"]
