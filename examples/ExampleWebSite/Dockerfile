FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS brandup-website-example
WORKDIR /app

COPY ./ ./
ENTRYPOINT ["dotnet", "ExampleWebSite.dll"]