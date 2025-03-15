# Etapa 1: Construção
FROM node:18 AS build

# Define o diretório de trabalho no container
WORKDIR /app

# Copia o package.json
COPY package.json ./


# Instala as dependências
RUN npm install

# Copia os diretórios public e src
COPY public/ ./public
COPY src/ ./src

# Copia qualquer outro arquivo necessário (como .env, vite.config.js, etc.)
COPY . .

# Constrói a aplicação para produção
RUN npm run build

CMD ["npm", "start"]