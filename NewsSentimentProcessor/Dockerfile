FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["NewsSentimentProcessor/NewsSentimentProcessor/NewsSentimentProcessor.csproj", "NewsSentimentProcessor/NewsSentimentProcessor/"]
COPY ["NewsSentimentProcessor/NewsSentimentProcessor.Application/NewsSentimentProcessor.Application.csproj", "NewsSentimentProcessor/NewsSentimentProcessor.Application/"]
COPY ["NewsApi/Shared/Shared.csproj", "NewsApi/Shared/"]

RUN dotnet restore "NewsSentimentProcessor/NewsSentimentProcessor/NewsSentimentProcessor.csproj"

COPY ["NewsSentimentProcessor/NewsSentimentProcessor", "NewsSentimentProcessor/NewsSentimentProcessor"]
COPY ["NewsSentimentProcessor/NewsSentimentProcessor.Application", "NewsSentimentProcessor/NewsSentimentProcessor.Application"]
COPY ["NewsApi/Shared", "NewsApi/Shared"]

WORKDIR "/src/NewsSentimentProcessor/NewsSentimentProcessor"
RUN dotnet build "./NewsSentimentProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NewsSentimentProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NewsSentimentProcessor.dll"]