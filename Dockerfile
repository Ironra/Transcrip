# Base runtime image
# 1) Imagen base de runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS="http://+:5142"
EXPOSE 5142

# 2) Imagen de build + publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia sólo el csproj para cachear restore
COPY ["VoskRealtimeApi.csproj", "./"]
RUN dotnet restore "VoskRealtimeApi.csproj"

# Copia todo el código (incluyendo Models/) y construye
COPY . .
RUN dotnet build "VoskRealtimeApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publica la aplicación (incluyendo wwwroot/, Models/, etc.)
RUN dotnet publish "VoskRealtimeApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 3) Imagen final
FROM base AS final
WORKDIR /app
# Copia los artefactos publicados (con Models/) al contenedor runtime
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "VoskRealtimeApi.dll"]



