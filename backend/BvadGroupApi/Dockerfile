# ==================== STAGE 1 : Build ====================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["BvadGroupApi.csproj", "./"]
RUN dotnet restore

COPY . .
RUN dotnet publish "BvadGroupApi.csproj" -c Release -o /app/publish

# ==================== STAGE 2 : Runtime ====================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "BvadGroupApi.dll"]