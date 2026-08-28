FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

COPY . .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SampleApp.dll"]