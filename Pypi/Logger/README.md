# LogCenter.Logger

This lib will help you to send your app's logs to LogCenter.  

<p align="center">
  <img src="https://github.com/hudsonventura/LogCenter/blob/main/logo.png?raw=true" alt="Descrição da imagem" width="230px">
</p>

Each log entry is queued and will be sent in order, following a FIFO (First In, First Out) approach.  
This ensures that logs are processed sequentially, maintaining the correct logging sequence.  


### Getting Started


### Basic usage
First install the lib from Nuget
``` bash
pip install LogCenter --no-cache-dir
```


Initialize the logger with you LogCenter configs
``` python
from LogCenter import LogCenterOptions,LogCenterLogger, LogLevel

options = LogCenterOptions(
    url="http://localhost:9200",
    table="example dotnet console", //the spaces will be converted to _ (underscore).
    token="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9....", //Generate this on LogCenter inteface, on you profile photo.
)

logger = LogCenterLogger(options)
```

Logging a simple message async, in the background without blocking code execution
``` python
// Use this 
logger.LogAsync(LogLevel.INFO, "Hello World")
```

Logging a simple message syncronous in foreground, blocking code execution.
``` python
// Use this 
logger.Log(LogLevel.ERROR, "Hello World")
```

Logging an object in the background without blocking code execution. You can use any type of object, which will be converted to JSON before being sent to LogCenter.
``` python
logger.LogAsync(LogLevel.CRITICAL, "This is just an example", {
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
    "descricao": "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat"
})
```


#### Build Package
```bash
python -m build # must be installed before
twine upload dist/* # must be installed before
pip install LogCenter --no-cache-dir
```