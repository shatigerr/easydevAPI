# Imagen base para el runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Imagen para la construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiamos el archivo de proyecto y restauramos las dependencias
COPY ["easydev.csproj", "."]
RUN dotnet restore "./easydev.csproj"

# Copiamos el resto de los archivos y construimos el proyecto
COPY . .
WORKDIR "/src"
RUN dotnet build "easydev.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicamos el proyecto
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "easydev.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "easydev.dll"]
