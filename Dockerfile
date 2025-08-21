# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY DAAS.sln .

# Copy project files for better caching
COPY src/DAAS.Domain/DAAS.Domain.csproj src/DAAS.Domain/
COPY src/DAAS.Application/DAAS.Application.csproj src/DAAS.Application/
COPY src/DAAS.Infrastructure/DAAS.Infrastructure.csproj src/DAAS.Infrastructure/
COPY src/DAAS.Api/DAAS.Api.csproj src/DAAS.Api/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ src/

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish src/DAAS.Api/DAAS.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install necessary packages
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/api/Users || exit 1

# Run the application
ENTRYPOINT ["dotnet", "DAAS.Api.dll"]