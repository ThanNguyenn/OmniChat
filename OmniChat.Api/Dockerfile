# BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OmniChat.sln .
COPY OmniChat.Api/OmniChat.Api.csproj OmniChat.Api/
COPY OmniChat.Application/OmniChat.Application.csproj OmniChat.Application/
COPY OmniChat.Infrastructure/OmniChat.Infrastructure.csproj OmniChat.Infrastructure/

RUN dotnet restore

COPY . .

WORKDIR /src/OmniChat.Api
RUN dotnet publish -c Release -o /app/publish

# RUN
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "OmniChat.Api.dll"]