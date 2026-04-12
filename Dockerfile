FROM node:24-alpine AS frontend-build
WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

COPY global.json WikiRacer.slnx ./
COPY backend/src/WikiRacer.Api/WikiRacer.Api.csproj backend/src/WikiRacer.Api/
COPY backend/src/WikiRacer.Application/WikiRacer.Application.csproj backend/src/WikiRacer.Application/
COPY backend/src/WikiRacer.Contracts/WikiRacer.Contracts.csproj backend/src/WikiRacer.Contracts/
COPY backend/src/WikiRacer.Domain/WikiRacer.Domain.csproj backend/src/WikiRacer.Domain/
COPY backend/src/WikiRacer.Infrastructure/WikiRacer.Infrastructure.csproj backend/src/WikiRacer.Infrastructure/
RUN dotnet restore backend/src/WikiRacer.Api/WikiRacer.Api.csproj

COPY backend/ backend/
RUN dotnet publish backend/src/WikiRacer.Api/WikiRacer.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /app/dist/wiki-racer/browser ./wwwroot

ENTRYPOINT ["dotnet", "WikiRacer.Api.dll"]
