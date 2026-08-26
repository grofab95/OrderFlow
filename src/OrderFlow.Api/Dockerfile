# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/OrderFlow.Api/OrderFlow.Api.csproj", "src/OrderFlow.Api/"]
COPY ["src/OrderFlow.Contracts/OrderFlow.Contracts.csproj", "src/OrderFlow.Contracts/"]
RUN dotnet restore "src/OrderFlow.Api/OrderFlow.Api.csproj"

COPY src/ src/

RUN dotnet publish "src/OrderFlow.Api/OrderFlow.Api.csproj" \
    --configuration $BUILD_CONFIGURATION \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "OrderFlow.Api.dll"]
