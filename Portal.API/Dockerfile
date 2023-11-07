#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Portal.API/Portal.API.csproj", "Portal.API/"]
RUN dotnet restore "Portal.API/Portal.API.csproj"
COPY . .
WORKDIR "/src/Portal.API"
RUN dotnet build "Portal.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Portal.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Portal.API.dll"]