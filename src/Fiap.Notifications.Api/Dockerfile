FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8083

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia os arquivos de projeto respeitando a estrutura de pastas da solução
COPY ["Fiap.Notifications.Api/Fiap.Notifications.Api.csproj", "Fiap.Notifications.Api/"]
COPY ["Fiap.Notifications.Domain/Fiap.Notifications.Domain.csproj", "Fiap.Notifications.Domain/"]

# Restaura as dependências a partir do projeto principal
RUN dotnet restore "Fiap.Notifications.Api/Fiap.Notifications.Api.csproj"

# Copia todo o código fonte
COPY . .

# Define o diretório de trabalho no projeto principal e compila
WORKDIR "/src/Fiap.Notifications.Api"
RUN dotnet build "Fiap.Notifications.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Fiap.Notifications.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV ASPNETCORE_URLS="http://+:8083"
ENV ASPNETCORE_ENVIRONMENT="Development"
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fiap.Notifications.Api.dll"]
