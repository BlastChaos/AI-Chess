#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["AI Chess/AI Chess.csproj", "AI Chess/"]
RUN dotnet restore "AI Chess/AI Chess.csproj"
COPY . .
WORKDIR "/src/AI Chess"
RUN dotnet build "AI Chess.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AI Chess.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/Games
ENTRYPOINT ["dotnet", "AI Chess.dll"]