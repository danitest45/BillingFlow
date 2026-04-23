# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto
COPY BillingFlow.Api/BillingFlow.Api.csproj BillingFlow.Api/
COPY BillingFlow.Application/BillingFlow.Application.csproj BillingFlow.Application/
COPY BillingFlow.Domain/BillingFlow.Domain.csproj BillingFlow.Domain/
COPY BillingFlow.Infrastructure/BillingFlow.Infrastructure.csproj BillingFlow.Infrastructure/

# Restaura dependências
RUN dotnet restore BillingFlow.Api/BillingFlow.Api.csproj

# Copia o restante do código
COPY . .

# Publica a aplicação
RUN dotnet publish BillingFlow.Api/BillingFlow.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Render injeta a variável PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "BillingFlow.Api.dll"]