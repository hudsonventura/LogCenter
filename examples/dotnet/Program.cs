// See https://aka.ms/new-console-template for more information
using LogCenter;


LogCenterLogger logger = new LogCenterLogger(
    new LogCenterOptions(){
        url = "http://localhost:9200",
        table = "teste",
        token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AbG9nY2VudGVyLm9yZyIsIm5hbWUiOiJ0ZXN0ZSIsInRhYmxlcyI6InRlc3RlIiwiZXhwIjoyMDU2ODM4ODY3LCJpc3MiOiJTZXVJc3N1ZXIiLCJhdWQiOiJTZXVBdWRpZW5jZSJ9.MNkYByqeXTNYngEzpDx0NXXsDPQBT6oSy-ESKyACJmU",
        consoleLog = true,
        consoleLogEntireObject = true
    } 
    //, Guid.NewGuid() //or a string. It's optional. If empty, it will generate a new one Guid, else, you can you your own traceId, Guid or string
);




// Log asynchronously. Start a threat to send log to log center.
logger.Log(LogLevel.Info, "Hello World - Info", Guid.NewGuid());
logger.Log(LogLevel.Warning, "Hello World - Warning");
logger.Log(LogLevel.Trace, "Hello World - Trace");
logger.Log(LogLevel.Debug, "Hello World - Debug");
logger.Log(LogLevel.Success, "Hello World - Success");
logger.Log(LogLevel.Error, "Hello World - Error");
logger.Log(LogLevel.Critical, "Hello World - Critical");
logger.Log(LogLevel.Fatal, "Hello World - Fatal");



// Log asynchronously
logger.LogAsync(LogLevel.Trace, "Hello World 2", new { 
    nome = "John Doe Jr. da Silva",
    idade = 30,
    cpf = "123.456.789-00",
    email = "john.doe.jr@example.com",
    telefone = "11 91234-5678",
    cep = "04545-000",
    logradouro = "Rua Professor At lio Innocenti, 165",
    bairro = "Vila Nova Concei o",
    cidade = "S o Paulo",
    uf = "SP",
    pais = "Brasil",
    dataDeNascimento = DateTime.Now.AddDays(-30),
    altura = 1.80m,
    peso = 70.5m,
    descricao = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed sit amet nulla auctor, vestibulum magna sed, convallis ex. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Nulla facilisi. In hac habitasse platea dictumst. Vivamus nec sapien euismod, facilisis lectus at, ultrices ipsum. Nullam malesuada, odio ut euismod congue, nisi lorem tincidunt nisi, at convallis nulla erat sed ex.",
    observacoes = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed sit amet nulla auctor, vestibulum magna sed, convallis ex. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Nulla facilisi. In hac habitasse platea dictumst. Vivamus nec sapien euismod, facilisis lectus at, ultrices ipsum. Nullam malesuada, odio ut euismod congue, nisi lorem tincidunt nisi, at convallis nulla erat sed ex.",
    comentarios = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed sit amet nulla auctor, vestibulum magna sed, convallis ex. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Nulla facilisi. In hac habitasse platea dictumst. Vivamus nec sapien euismod, facilisis lectus at, ultrices ipsum. Nullam malesuada, odio ut euismod congue, nisi lorem tincidunt nisi, at convallis nulla erat sed ex.",
    historico = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed sit amet nulla auctor, vestibulum magna sed, convallis ex. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Nulla facilisi. In hac habitasse platea dictumst. Vivamus nec sapien euismod, facilisis lectus at, ultrices ipsum. Nullam malesuada, odio ut euismod congue, nisi lorem tincidunt nisi, at convallis nulla erat sed ex.",
    detalhesTecnicos = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed sit amet nulla auctor, vestibulum magna sed, convallis ex. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Nulla facilisi. In hac habitasse platea dictumst. Vivamus nec sapien euismod, facilisis lectus at, ultrices ipsum. Nullam malesuada, odio ut euismod congue, nisi lorem tincidunt nisi, at convallis nulla erat sed ex."
});

// Log asynchronously and wait for confirmation
logger.LogAsync(LogLevel.Critical, "Hello World 3");