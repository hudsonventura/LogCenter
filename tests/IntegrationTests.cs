using System.Net;
using System.Text;
using System.Text.Json;
using Bogus;
using Microsoft.Extensions.Logging;
using server.Domain;
using Xunit.Abstractions;

namespace tests;

public class UnitTest1
{
    ITestOutputHelper _output;
    HttpClient client = new Host().CreateClient();
    string table = "integration tests";


    public UnitTest1(ITestOutputHelper output)
    {
        _output = output;
        client.DefaultRequestHeaders.Add("Authorization", "Bearer xB70Valquiredb2c884bb1b01b4dd884fghrtnnrnc5ea57");

        
    }

    [Fact]
    public async Task InsertLongList()
    {
        List<MyObjectExample> list = new List<MyObjectExample>();

        //gera 1000 objetos
        for (int i = 0; i < 1_000; i++){
            var faker = new Faker<MyObjectExample>()
                .RuleFor(o => o.Name, f => f.Person.FullName)
                .RuleFor(o => o.Email, f => f.Person.Email)
                .RuleFor(o => o.Age, f => f.Random.Int(18, 65));

            // Gerando o objeto
            var objetoFicticio = faker.Generate();

            list.Add(objetoFicticio);
        }

        var json = JsonSerializer.Serialize(list);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/{table}/_doc") {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Level", Level.Debug.ToString());
        var response = await client.SendAsync(request);

        

        string responseBody = await response.Content.ReadAsStringAsync();
        long id = long.Parse(responseBody);
        _output.WriteLine($"ID: {id}");
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CheckInputLongList(){
        
    }

    //TODO: check if the object was input

    [Fact]
    public async Task InsertLargeObject()
    {
        using var image = File.OpenRead("../../../example_image.jpg");
        var bytes = new byte[image.Length];
        await image.ReadAsync(bytes, 0, (int)image.Length);
        var imageBase64 = Convert.ToBase64String(bytes);

        var obj = new { name = "joao", age = 18, email = "joao@joao.com", imageBase64 };
        var content = new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/{table}/_doc", content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    [Fact]
    public async Task CheckInputLargeObject(){

    }
    
    [Fact]
    public async Task CheckInputLargeObjectBase64Droped(){

    }


    [Fact]
    public async Task ListTables(){

    }

    [Fact]
    public async Task DeleteOldRecords(){

    }

    

    [Fact]
    public async Task GetLogByID(){

    }


    [Fact]
    public async Task InputHundredsObjects()
    {
        int contador = 0;
        Random random = new Random();

        //loop para criar os registros
        for (int i = 0; i < 1_000_000; i++){
            var faker = new Faker<MyObjectExample>()
                .RuleFor(o => o.Name, f => f.Person.FullName)
                .RuleFor(o => o.Email, f => f.Person.Email)
                .RuleFor(o => o.Age, f => f.Random.Int(18, 65));

            // Gerando o objeto
            var objetoFicticio = faker.Generate();

            string gerado = System.Text.Json.JsonSerializer.Serialize(objetoFicticio);

            // Gera um texto Lorem Ipsum
            //var faker = new Faker();
            //string gerado = faker.Lorem.Paragraphs(1); // Gera 1 parágrafo


            int number = random.Next(1, 6);
            Level level = (Level)number;

            
            var request = new HttpRequestMessage(HttpMethod.Post, $"/{table}/_doc") {
                Content = new StringContent(gerado, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Level", level.ToString());
            var response = await client.SendAsync(request);
            


            
            // Opcional: Log para cada 100.000 inserções
            if (contador % 100 == 0)
            {
                Console.WriteLine($"{contador} registros gerados ...");
            }
            contador++;
        }
        Assert.Equal(true, true);
    }

    

    [Fact]
    public async Task Search(){

    }
}




class MyObjectExample{
    public int Id { get; set;} = 0;
    public string Name { get; set;}
    public string Email { get; set;}
    public int Age { get; set;}
}