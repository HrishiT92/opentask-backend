FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/OpenTask.Api/OpenTask.Api.csproj", "src/OpenTask.Api/"]
COPY ["src/OpenTask.Application/OpenTask.Application.csproj", "src/OpenTask.Application/"]
COPY ["src/OpenTask.Domain/OpenTask.Domain.csproj", "src/OpenTask.Domain/"]
COPY ["src/OpenTask.Infrastructure/OpenTask.Infrastructure.csproj", "src/OpenTask.Infrastructure/"]
RUN dotnet restore "src/OpenTask.Api/OpenTask.Api.csproj"
COPY . .
WORKDIR "/src/src/OpenTask.Api"
RUN dotnet build "OpenTask.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenTask.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenTask.Api.dll"]
