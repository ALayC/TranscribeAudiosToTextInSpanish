# Stage 1: Base image for running the application (ASP.NET Core 8.0)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Stage 2: Build the application using the .NET 8.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["TranscibirAudios/TranscibirAudios.csproj", "TranscibirAudios/"]
RUN dotnet restore "TranscibirAudios/TranscibirAudios.csproj"

# Copy the remaining source code
COPY . .

# Set the working directory to the project folder and publish the app
WORKDIR "/src/TranscibirAudios"
RUN dotnet publish "TranscibirAudios.csproj" -c Release -o /app/publish

# Stage 3: Final stage - run the application
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TranscibirAudios.dll"]
