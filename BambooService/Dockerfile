FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY BambooService/BambooService.csproj BambooService/
COPY Shared/Shared.csproj Shared/
RUN dotnet restore BambooService/BambooService.csproj

# Copy source and build
COPY BambooService/ BambooService/
COPY Shared/ Shared/
RUN dotnet publish BambooService/BambooService.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install OpenSSL for HTTPS
RUN apt-get update && apt-get install -y openssl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY certs/ /certs/

EXPOSE 8080 8443

ENTRYPOINT ["dotnet", "BambooService.dll"]