# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj first to leverage Docker layer caching
COPY ["src.csproj", "./"]
RUN dotnet restore "./src.csproj"

# Copy the rest of the source
COPY . .

# Publish
RUN dotnet publish "./src.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Bind to a container-friendly port
ENV ASPNETCORE_URLS=http://+:8080 \
	ASPNETCORE_ENVIRONMENT=Development \
	DOTNET_ENVIRONMENT=Development
EXPOSE 8080

# Serilog writes to Logs/log-.txt; ensure the folder exists
RUN mkdir -p /app/Logs

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "src.dll"]