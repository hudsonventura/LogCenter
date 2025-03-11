import asyncio
import os
import sys
import uuid

sys.path.append(os.path.abspath("Pypi/Logger"))


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
logger.log_async(LogLevel.INFO, "Hello World - 1")
logger.log_async(LogLevel.WARNING, "Hello World - 2")
logger.log_async(LogLevel.TRACE, "Hello World - 3")
logger.log_async(LogLevel.DEBUG, "Hello World - 4")
logger.log_async(LogLevel.SUCCESS, "Hello World - 5")
logger.log_async(LogLevel.ERROR, "Hello World - 6")
logger.log(LogLevel.CRITICAL, "Hello World - 7")



logger.log_async(LogLevel.CRITICAL, "Hello World 8", {
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
    "descricao": "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat",
    "observacoes": "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat",
    "comentarios": "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat",
    "historico": "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat",
    "detalhesTecnicos": "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat",
})
logger.log(LogLevel.SUCCESS, "Hello World - 9")
