FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["StudyCompanion.Core/StudyCompanion.Core.csproj", "StudyCompanion.Core/"]
RUN dotnet restore "StudyCompanion.Core/StudyCompanion.Core.csproj"
COPY . .
WORKDIR "/src/StudyCompanion.Core"
RUN dotnet build "StudyCompanion.Core.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "StudyCompanion.Core.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudyCompanion.Core.dll"]
