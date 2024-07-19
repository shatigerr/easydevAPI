FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["easydev/easydev.csproj","easydev/"]
RUN dotnet restore "./easydev/./easydev.csproj"
COPY . .
WORKDIR "/src/easydev"
RUN dotnet build "./easydev.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./easydev.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "easydev.dll"]