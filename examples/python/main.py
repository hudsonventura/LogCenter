import os
import sys
import uuid


#sys.path.append(os.path.abspath("src/libs/Pypi/Logger/src")) # Just for API development

from LogCenter import LogCenterOptions,LogCenterLogger, LogLevel





options = LogCenterOptions(
    url="http://localhost:9200",
    table="teste",
    token="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AbG9nY2VudGVyLm9yZyIsIm5hbWUiOiJ0ZXN0ZSIsInRhYmxlcyI6InRlc3RlIiwiZXhwIjoyMDU2ODM4ODY3LCJpc3MiOiJTZXVJc3N1ZXIiLCJhdWQiOiJTZXVBdWRpZW5jZSJ9.MNkYByqeXTNYngEzpDx0NXXsDPQBT6oSy-ESKyACJmU",
    consoleLog = True,                                # Log the message on the console as a comon Console.WriteLine(). Default is true
    consoleLogEntireObject = True                     # Log the entire objeti to the Console.WriteLine(). Default is false
)

logger = LogCenterLogger(options, 
                         #trace_id = str(uuid.uuid4()) # It's optional. If empty, it will generate a new one Guid, else, you can you your own traceId, Guid or string
                         )

# Logs síncronos
logger.LogAsync(LogLevel.INFO, "Hello World - 1")
logger.LogAsync(LogLevel.WARNING, "Hello World - 2")
logger.LogAsync(LogLevel.TRACE, "Hello World - 3")
logger.LogAsync(LogLevel.DEBUG, "Hello World - 4")
logger.LogAsync(LogLevel.SUCCESS, "Hello World - 5")
logger.LogAsync(LogLevel.ERROR, "Hello World - 6")
logger.Log(LogLevel.CRITICAL, "Hello World - 7")
logger.Log(LogLevel.FATAL, "Hello World - 7,5")



logger.LogAsync(LogLevel.CRITICAL, "Hello World 8", {
    "nome": "John Doe Jr. da Silva",
    "idade": 30,
    "CPF": "123.456.789-00",
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

logger.Log(LogLevel.SUCCESS, "Hello World - 9")
