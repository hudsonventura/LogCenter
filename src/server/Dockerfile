# Use a imagem base do SDK do .NET Core 8.0 para compilar o aplicativo
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

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

# Use a imagem base do runtime do .NET Core 8.0 para executar o aplicativo
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Defina o diretório de trabalho no container
WORKDIR /app

# Copie os arquivos compilados do estágio de build para o diretório de trabalho
COPY --from=build /app/out .

# Defina a variável de ambiente para a porta 5000
ENV ASPNETCORE_URLS=http://+:5000

# Exponha a porta 80 para acessar o aplicativo
EXPOSE 5000

# Defina o comando de entrada para executar o aplicativo
ENTRYPOINT ["dotnet", "server.dll"]
