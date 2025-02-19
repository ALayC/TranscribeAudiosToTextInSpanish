# Etapa 1: Imagen base para ejecutar la aplicación (ASP.NET Core 8.0)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Etapa 2: Build de la aplicación usando el SDK de .NET 8.0
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia el archivo de proyecto y restaura las dependencias
COPY ["TranscibirAudios/TranscibirAudios.csproj", "TranscibirAudios/"]
RUN dotnet restore "TranscibirAudios/TranscibirAudios.csproj"

# Copia el resto del código fuente
COPY . .

# Cambia al directorio del proyecto y publica la aplicación en modo Release
WORKDIR "/src/TranscibirAudios"
RUN dotnet publish "TranscibirAudios.csproj" -c Release -o /app/publish

# Etapa 3: Imagen final para ejecutar la aplicación
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TranscibirAudios.dll"]
