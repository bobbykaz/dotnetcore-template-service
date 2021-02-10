#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["bk-dotnet-template/bk-dotnet-template.csproj", "bk-dotnet-template/"]
RUN dotnet restore "bk-dotnet-template/bk-dotnet-template.csproj"
COPY . .
WORKDIR "/src/bk-dotnet-template"
RUN dotnet build "bk-dotnet-template.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "bk-dotnet-template.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "bk-dotnet-template.dll"]