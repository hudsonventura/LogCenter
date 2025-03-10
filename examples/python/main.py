import asyncio
import os
import sys
import uuid

sys.path.append(os.path.abspath("Pypi/Logger"))
#sys.path.append('../')

print(f"Dir atual: {os.getcwd()}")

from LogCenterOptions import LogCenterOptions
from LogCenterLogger import LogCenterLogger
from LogLevel import LogLevel




options = LogCenterOptions(
    url="http://localhost:9200",
    table="teste",
    token="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AbG9nY2VudGVyLm9yZyIsIm5hbWUiOiJ0ZXN0ZSIsInRhYmxlcyI6InRlc3RlIiwiZXhwIjoyMDU2ODM4ODY3LCJpc3MiOiJTZXVJc3N1ZXIiLCJhdWQiOiJTZXVBdWRpZW5jZSJ9.MNkYByqeXTNYngEzpDx0NXXsDPQBT6oSy-ESKyACJmU",
)

logger = LogCenterLogger(options)

# Logs síncronos
logger.log(LogLevel.INFO, "Hello World - Info")
logger.log(LogLevel.WARNING, "Hello World - Warning")
logger.log(LogLevel.TRACE, "Hello World - Trace")
logger.log(LogLevel.DEBUG, "Hello World - Debug")
logger.log(LogLevel.SUCCESS, "Hello World - Success")
logger.log(LogLevel.ERROR, "Hello World - Error")
logger.log(LogLevel.CRITICAL, "Hello World - Critical")

# Log assíncrono
asyncio.run(logger.log_async(LogLevel.CRITICAL, "Hello World 2", {
    "nome": "John Doe Jr. da Silva",
    "idade": 30,
    "cpf": "123.456.789-00",
    "email": "john.doe.jr@example.com",
    "telefone": "11 91234-5678",
    "cep": "04545-000",
    "logradouro": "Rua Professor Atílio Innocenti, 165",
    "bairro": "Vila Nova Conceição",
    "cidade": "São Paulo",
    "uf": "SP",
    "pais": "Brasil",
    "dataDeNascimento": str(uuid.uuid4()),  # Simulação de data
    "altura": 1.80,
    "peso": 70.5,
    "descricao": "Lorem ipsum dolor sit amet...",
    "observacoes": "Lorem ipsum dolor sit amet...",
    "comentarios": "Lorem ipsum dolor sit amet...",
    "historico": "Lorem ipsum dolor sit amet...",
    "detalhesTecnicos": "Lorem ipsum dolor sit amet..."
}))
