# MotoHM.Api qovluğunda yerləşir: MotoHM/MotoHM/MotoHM.Api/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY MotoHM.Api.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /data/uploads /app/logs
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    Uploads__Path=/data/uploads
VOLUME ["/data/uploads"]
EXPOSE 8080
ENTRYPOINT ["dotnet", "MotoHM.Api.dll"]
