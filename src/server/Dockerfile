# Use a imagem base do SDK do .NET Core 10.0 para compilar o aplicativo
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Defina o diretório de trabalho no container
WORKDIR /app

# Copie os arquivos de projeto para o diretório de trabalho
COPY *.csproj ./

# Restaure as dependências do projeto
RUN dotnet restore

# Copie o restante dos arquivos do aplicativo para o diretório de trabalho
COPY . .

# Compile o aplicativo em modo Release
RUN dotnet publish server.csproj -c Release -o out

# Use a imagem base do runtime do .NET Core 10.0 para executar o aplicativo
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# Defina o diretório de trabalho no container
WORKDIR /app

# Copie os arquivos compilados do estágio de build para o diretório de trabalho
COPY --from=build /app/out .

# Porta padrao usada pelo Program.cs quando ASPNETCORE_LISTEN nao for informado
EXPOSE 9200

# Defina o comando de entrada para executar o aplicativo
ENTRYPOINT ["dotnet", "server.dll"]
