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
 - level -> Optional. Default Info. It could be:
    - Info
    - Debug
    - Warning
    - Error
    - Critical

Body:  
The body could be your json object to save, or a list of objects.
```json
    {
        "chatId": "556592327494@c.us",
        "contentType": "string",
        "content": "content",
        "contents":{
            "filename":"iamge.jpg",
            "data": "iVBORw0KGgoAAAANSUhEUgAA"
        }
    }
```
If you upload a file in base64, the base64 string will be removed with a message that it has been removed.  

##### Response
Status code : 204 - Created
```
7245983394234892288
```
The response is a simples string with the id of the created object


  
 - TODO: Create a way to configure dates for each table in the RecyclingRecords service