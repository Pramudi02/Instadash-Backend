# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY Backend.csproj ./
RUN dotnet restore Backend.csproj

# copy everything else and build
COPY . ./
RUN dotnet publish Backend.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Default port used by the backend (can be overridden by env var ASPNETCORE_URLS)
EXPOSE 10000

ENV ASPNETCORE_URLS="http://+:10000"
ENTRYPOINT ["dotnet", "Backend.dll"]
