FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 7014

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MS.API.Mini/MS.API.Mini.csproj", "MS.API.Mini/"]
RUN dotnet restore "MS.API.Mini/MS.API.Mini.csproj"
COPY . .
WORKDIR "/src/MS.API.Mini"
RUN dotnet build "./MS.API.Mini.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MS.API.Mini.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

ENV ASPNETCORE_URLS=http://+:7104
ENV DOTNET_ENVIRONMENT=Production

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MS.API.Mini.dll"]
