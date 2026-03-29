# ========================
# BUILD STAGE
# ========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj 
COPY OmniChat.Api/OmniChat.Api.csproj OmniChat.Api/
COPY OmniChat.Application/OmniChat.Application.csproj OmniChat.Application/
COPY OmniChat.Infrastructure/OmniChat.Infrastructure.csproj OmniChat.Infrastructure/

# restore
RUN dotnet restore OmniChat.Api/OmniChat.Api.csproj

# copy full source
COPY . .

# publish
RUN dotnet publish OmniChat.Api/OmniChat.Api.csproj -c Release -o /app/publish

# ========================
# RUNTIME STAGE
# ========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "OmniChat.Api.dll"]