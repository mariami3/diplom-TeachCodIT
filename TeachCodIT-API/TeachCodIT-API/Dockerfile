FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TeachCodIT-API/TeachCodIT-API/TeachCodIT-API.csproj", "TeachCodIT-API/"]
RUN dotnet restore "TeachCodIT-API/TeachCodIT-API.csproj"

COPY TeachCodIT-API/TeachCodIT-API/ TeachCodIT-API/
WORKDIR "/src/TeachCodIT-API"
RUN dotnet build "TeachCodIT-API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TeachCodIT-API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TeachCodIT-API.dll"]