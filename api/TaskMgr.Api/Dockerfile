FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskMgr.Api/TaskMgr.Api.csproj", "TaskMgr.Api/"]
RUN dotnet restore "TaskMgr.Api/TaskMgr.Api.csproj"
COPY . .
WORKDIR "/src/TaskMgr.Api"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskMgr.Api.dll"]