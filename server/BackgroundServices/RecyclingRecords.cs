
using server.Repositories;

namespace server.BackgroundServices;

public class RecyclingRecords : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public RecyclingRecords(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task task11 = Task.Run(() => Recycling()); 
        return Task.CompletedTask;

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;

    }


    public void Recycling(){
        while(true){
            Console.WriteLine("Starting Recycling Records");

            // Crie um escopo para utilizar o serviço scoped
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DBRepository>();

                List<string> tables = db.ListTabels();
            
                foreach(string table in tables){
                    try
                    {
                        Console.Write($"Recycling records from table '{table}' ... ");
                        db.DeleteRecords(table, DateTime.UtcNow.AddDays(-360));
                        Console.WriteLine($"Ok");
                    }
                    catch (System.Exception error)
                    {
                        string msg = error.Message;
                        if(error.InnerException != null){
                            msg += error.InnerException.Message;
                        }
                        Console.WriteLine($"Some error was got -> {msg}");
                    }
                }
            }
            Console.WriteLine("Starting Recycling Records - Finished right now");
            Thread.Sleep(1000*60*10); //each 10 minutes
        }
    }
}
