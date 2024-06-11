FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WebsocketServer/WebsocketServer.csproj", "WebsocketServer/"]
RUN dotnet restore "WebsocketServer/WebsocketServer.csproj"
COPY . .
WORKDIR "/src/WebsocketServer"
RUN dotnet build "WebsocketServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebsocketServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebsocketServer.dll"]
