FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/OpenClue.FSharpToDo/OpenClue.FSharpToDo.fsproj", "src/OpenClue.FSharpToDo/"]
RUN dotnet restore "src/OpenClue.FSharpToDo/OpenClue.FSharpToDo.fsproj"
COPY . .
WORKDIR "/src/src/OpenClue.FSharpToDo"
RUN dotnet build "OpenClue.FSharpToDo.fsproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OpenClue.FSharpToDo.fsproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "OpenClue.FSharpToDo.dll"]
