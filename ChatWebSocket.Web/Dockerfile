#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ChatWebSocket.Web/ChatWebSocket.Web.csproj", "ChatWebSocket.Web/"]
COPY ["ChatWebSocket/ChatWebSocket.csproj", "ChatWebSocket/"]
RUN dotnet restore "ChatWebSocket.Web/ChatWebSocket.Web.csproj"
COPY . .
WORKDIR "/src/ChatWebSocket.Web"
RUN dotnet build "ChatWebSocket.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatWebSocket.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatWebSocket.Web.dll"]
