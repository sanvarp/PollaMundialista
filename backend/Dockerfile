# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (layer-cached) by copying only project files.
COPY src/PollaMundialista.Api/*.csproj src/PollaMundialista.Api/
COPY src/PollaMundialista.Application/*.csproj src/PollaMundialista.Application/
COPY src/PollaMundialista.Domain/*.csproj src/PollaMundialista.Domain/
COPY src/PollaMundialista.Infrastructure/*.csproj src/PollaMundialista.Infrastructure/
RUN dotnet restore src/PollaMundialista.Api/PollaMundialista.Api.csproj

# Copy the rest and publish.
COPY . .
RUN dotnet publish src/PollaMundialista.Api/PollaMundialista.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Azure Container Apps terminates TLS at the ingress; the app serves HTTP on 8080.
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run as the image's non-root user.
USER $APP_UID

ENTRYPOINT ["dotnet", "PollaMundialista.Api.dll"]
