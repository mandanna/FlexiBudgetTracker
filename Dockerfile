# ---- Build stage: uses the full SDK to compile + publish ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy just the csproj first and restore — this layer is cached and only
# re-runs when dependencies change, not on every code edit (faster rebuilds).
COPY src/ExpenseTracker.Api/ExpenseTracker.Api.csproj src/ExpenseTracker.Api/
RUN dotnet restore src/ExpenseTracker.Api/ExpenseTracker.Api.csproj

# Now copy the rest of the source and publish a Release build.
COPY src/ src/
RUN dotnet publish src/ExpenseTracker.Api/ExpenseTracker.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Final stage: slim runtime-only image that actually ships ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Run as a non-root user (the aspnet image provides APP_UID) — security best practice.
USER $APP_UID

# Serve plain HTTP on 8080 inside the container; TLS is handled by the host/proxy.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ExpenseTracker.Api.dll"]