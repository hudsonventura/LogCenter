# LogCenter  
<p align="center">
  <img src="logo.png" alt="Descrição da imagem" width="230px">
</p>


`LogCenter` is a custom solution for log store and querying, debug and audit log, similar to ElasticSearch — which is widely used for storing, searching, but not analyzing data logs (yet). However, LogCenter utilizes PostgreSQL as the underlying database for storing and querying logs.  





### How to store a log?

#### Request
`POST` /{YOUR_TABLE_NAME}  
or  
`POST` /{YOUR_TABLE_NAME}/_doc (like ElasticSearch)

Headers:
 - `level` -> *Optional*. Default is `Info`. It could be:
    - Info
    - Debug
    - Warning
    - Error
    - Critical

 - `description` -> *Optional*. Default is `null`. A brief description or identification about the log, protocol number or what you want. May be repeated.
 

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
    "FileContent1": "SGVsbG8gd29ybGQh", // Base64 de "Hello world!"
    "FileContent2": "data:/SGVsbG8gd29ybGQh",
    "NestedObject": 
    {
      "AnotherFile": "data:image/png;base64,/9j/4QuYRXhpZgAATU0AKgAAAAgABwESAAMAAAA" // Base64 de "Some other content"
    }
}
```

 > If you upload a file in base64, the base64 string will be removed with a message that it has been removed.  

##### Response
Status code : 204 - Created
```
7245983394234892288
```
The response is a simples string with the id of the created object



 - TODO: Create a way to configure dates for each table in the RecyclingRecords service

 - BUG: In the cloud test, table Lucas id 7246184241443110912, it was possible to add a base64 without the drop work

 - TODO: Create a way to set a timezone to show on frontend. Backend was implemented with header timezone.

 - TODO: Do VACUUM and/or VACUUM FULL ANALYSE.

 - ~~TODO: Change API port to 9200, like ElasticSearch default~~