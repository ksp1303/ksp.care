# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Reduce memory usage during build
ENV DOTNET_EnableDiagnostics=0

# Copy project file and restore dependencies
COPY ksp.care.csproj ./
RUN dotnet restore ksp.care.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish ksp.care.csproj -c Release -o /app/publish --no-restore

# Runtime stage (smaller image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Reduce memory footprint
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_gcServer=0
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ksp.care.dll"]
