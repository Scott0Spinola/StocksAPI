FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# Container listens on this port; map it from the host via docker run/compose.
EXPOSE 5275
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5275

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /source

# Copy the full build context. This Dockerfile supports building from either:
# - repo root (project at ./src/src.csproj)
# - src/ folder (project at ./src.csproj)
COPY . .

RUN set -eux; \
	if [ -f "./src/src.csproj" ]; then PROJECT_PATH="./src/src.csproj"; \
	elif [ -f "./src.csproj" ]; then PROJECT_PATH="./src.csproj"; \
	else echo "ERROR: Could not find src.csproj (expected ./src/src.csproj or ./src.csproj)"; exit 1; fi; \
	dotnet restore "$PROJECT_PATH"; \
	dotnet publish "$PROJECT_PATH" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "src.dll"]
