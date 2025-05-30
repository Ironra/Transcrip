# Base runtime image
# 1) Imagen base de runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# 2) Imagen de build + publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Instala curl y unzip
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl unzip \
 && rm -rf /var/lib/apt/lists/*

# Descarga y descomprime el modelo
# RUN curl -L https://alphacephei.com/vosk/models/vosk-model-small-es-0.42.zip \
RUN curl -L https://alphacephei.com/vosk/models/vosk-model-small-es-0.42.zip \
    -o model.zip \
 && unzip model.zip -d Models \
 && rm model.zip

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



