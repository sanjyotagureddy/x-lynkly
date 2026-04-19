# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Services/Lynkly.Resolver.API/Lynkly.Resolver.API.csproj", "src/Services/Lynkly.Resolver.API/"]
COPY ["aspire/lynkly.ServiceDefaults/lynkly.ServiceDefaults.csproj", "aspire/lynkly.ServiceDefaults/"]
COPY ["src/Core/Lynkly.Resolver.Application/Lynkly.Resolver.Application.csproj", "src/Core/Lynkly.Resolver.Application/"]
COPY ["src/Core/Lynkly.Resolver.Domain/Lynkly.Resolver.Domain.csproj", "src/Core/Lynkly.Resolver.Domain/"]
COPY ["shared/Lynkly.Shared.Kernel.Core/Lynkly.Shared.Kernel.Core.csproj", "shared/Lynkly.Shared.Kernel.Core/"]
COPY ["src/Infrastructure/Lynkly.Resolver.Infrastructure/Lynkly.Resolver.Infrastructure.csproj", "src/Infrastructure/Lynkly.Resolver.Infrastructure/"]
COPY ["src/Infrastructure/Lynkly.Resolver.Infrastructure.Caching/Lynkly.Resolver.Infrastructure.Caching.csproj", "src/Infrastructure/Lynkly.Resolver.Infrastructure.Caching/"]
COPY ["shared/Lynkly.Shared.Kernel.Caching/Lynkly.Shared.Kernel.Caching.csproj", "shared/Lynkly.Shared.Kernel.Caching/"]
COPY ["src/Infrastructure/Lynkly.Resolver.Infrastructure.Messaging/Lynkly.Resolver.Infrastructure.Messaging.csproj", "src/Infrastructure/Lynkly.Resolver.Infrastructure.Messaging/"]
COPY ["shared/Lynkly.Shared.Kernel.Messaging/Lynkly.Shared.Kernel.Messaging.csproj", "shared/Lynkly.Shared.Kernel.Messaging/"]
COPY ["shared/Lynkly.Shared.Kernel.Persistence/Lynkly.Shared.Kernel.Persistence.csproj", "shared/Lynkly.Shared.Kernel.Persistence/"]
COPY ["src/Infrastructure/Lynkly.Resolver.Infrastructure.Persistence/Lynkly.Resolver.Infrastructure.Persistence.csproj", "src/Infrastructure/Lynkly.Resolver.Infrastructure.Persistence/"]
RUN dotnet restore "./src/Services/Lynkly.Resolver.API/Lynkly.Resolver.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Lynkly.Resolver.API"
RUN dotnet build "./Lynkly.Resolver.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lynkly.Resolver.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lynkly.Resolver.API.dll"]
