# ---------------------------
# 1. Build stage (.NET 9 SDK)
# ---------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for all referenced projects
COPY ["KinoAppService/KinoAppService.csproj", "KinoAppService/"]
COPY ["KinoAppCore/KinoAppCore.csproj", "KinoAppCore/"]
COPY ["KinoAppDB/KinoAppDB.csproj", "KinoAppDB/"]
COPY ["KinoAppShared/KinoAppShared.csproj", "KinoAppShared/"]

# Restore dependencies for the API project
RUN dotnet restore "KinoAppService/KinoAppService.csproj"

# Copy the rest of the source code
COPY . .

# Publish the API
WORKDIR /src/KinoAppService
RUN dotnet publish "KinoAppService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------------------------
# 2. Runtime stage (.NET 9 ASP.NET)
# ---------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published binaries from build stage
COPY --from=build /app/publish .

# Expose the API port (matches docker-compose & .env)
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "KinoAppService.dll"]
