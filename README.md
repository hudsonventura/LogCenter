# LogCenter  
<p align="center">
  <img src="logo.png" alt="Descrição da imagem" width="230px">
</p>


`LogCenter` is a custom solution for log store and querying, debug and audit log, similar to ElasticSearch — which is widely used for storing, searching, but not analyzing data logs (yet). However, LogCenter utilizes PostgreSQL as the underlying database for storing and querying logs.  

![Web Interface](resources/image3.png)


# Getting Started

## How can I start LogCente?

Edit the `.env` file as your reality:

```
DB_HOST=localhost
DB_PORT=5432
DB_NAME=logcenter
DB_USER=logcenter
DB_PASSWORD=MyS3cr3tP@ssw0rd                    # CHANGE DB PASS
JWT_KEY=MyS3cr3tP@ssw0rdF0rJWTT0k3nGener4tion   # GENERATE A NEW KEY TO JWT ENCODING
SERVER_BACKEND_URL=http://localhost:9200        # PUT YOU BACKEND URL TO SERVE FRONTEND APP
```

After:
``` bash
sudo docker compose up
```



## Generating a token

Go to `http://localhost:5173` and make login.  
The initial user and pass is `admin@admin.com` and `admin`.  
![Login page](resources/image1.png)

Click on profile image, `Generate token`, set the values and click in `Generate`. After you can copy the token. Store the token in your system config file.
![Generating token](resources/image2.png)


## Sending logs - How to store a log?
You can use an existing libraries listed below. Simply configure it and start using it.

| Lang                      | Docs                                   |
|---------------------------|---------------------------------------|
| Dotnet Core console app   | [README.md](nuget/Logger/README.md)  |
| Aspnet Core               | [README.md](nuget/RequestLogger/README.md)  |
| Python console app        | [README.md](Pypi/Logger/README.md)  |
| Python with FastAPI       | [README.md](Pypi/Logger/README.md)  |


## How can I do a Request with out a lib?

Go to backend docs at `http://localhost:9200/scalar/v1`


#### Request
`POST` /{YOUR_TABLE_NAME}/

Headers:
 - `Authorization` -> **Required**  Bearer {{token}}
 - `level` -> Optional. Default is `Info`. It could be:
    - Trace
    - Info
    - Debug
    - Warning
    - Error
    - Critical
    - Success
 - `message` -> *Required*. Your message log.
 - `timezone` -> Optional. Default is `UTC`. Your can use UNIX timezones like `America/New_York`.
 - `traceID` -> **Optional, but highly recomended**. Use this to tag your log message or a group of messages with the same identifier. This allows you to filter by traceID in the web interface and view all related messages in a sequence.
 

Body:  
The body could be your json object to save, or a list of objects.
```json
{
    "nome": "João",
    "idade": 30,
    "cargo": "Desenvolvedor",
    "localizacao": {
      "cidade": "São Paulo",
      "pais": "Brasil"
    },
    "habilidades": ["C#", ".NET", "Elasticsearch"],
    "FileContent1": "SGVsbG8gd29ybGQh",
    "FileContent2": "data:/SGVsbG8gd29ybGQh",
    "NestedObject": 
    {
      "AnotherFile": "data:image/png;base64,/9j/4QuYRXhpZgAATU0AKgAAAAgABwESAAMAAAA" // Base64 de "Some other content"
    }
}
```

 > If you upload a file encoded in base64 that exceeds 1024 characters, the base64 string will be truncated. A message will indicate that the content has been shortened due to size limitations.


##### Response
Status code : 204 - Created
```
"655e331a-8340-0000-0000-8a3a38f32672"
```
The response is a simples string with the id of the created object



# Table Recycling



# Table Configuration


# TODO 

 ~~- TODO: Create a way to configure dates for each table in the RecyclingRecords service~~

 ~~- BUG: In the cloud test, table Lucas id 7246184241443110912, it was possible to add a base64 without the drop work~~

 ~~- TODO: Create a way to set a timezone to show on frontend. Backend was implemented with header timezone.~~

 ~~- TODO: Do VACUUM and/or VACUUM FULL ANALYSE.~~

 - TODO: Compatibility with XML -> https://github.com/alissonmbr/react-xml-viewer

 - ~~TODO: Change API port to 9200, like ElasticSearch default~~

 - BUG: favicon int backend
 - TODO: Normalize backend return
 - TODO: Retrieve more than 100 itens on frontend.
