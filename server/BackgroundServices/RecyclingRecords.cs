
using System.Data.Common;
using System.Text;
using System.Text.Json;
using Cronos;
using server.Domain;
using server.Repositories;

namespace server.BackgroundServices;

public class RecyclingRecords : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private HttpClient _client = new HttpClient();

    public RecyclingRecords(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    { 
        _client = new HttpClient();
        _client.BaseAddress = new Uri("http://localhost:9200/");

        Task task11 = Task.Run(() => Recycling()); 
        return Task.CompletedTask;

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    string execution_id = string.Empty;

    public void Recycling()
    {
        Thread.Sleep(1000*60); //wait 1 minute for api be started

        while(true){
            execution_id = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DBRepository>();

                
                
                List<ConfigTableObject> tables = db.GetConfiguredTables();
            
                foreach(ConfigTableObject table in tables)
                {
                    bool execute_vacuum = ValidateCron(table.vacuum_input);
                    bool execute_vacuum_full = ValidateCron(table.vacuum_full_input);
                    if(!table.delete && !execute_vacuum && !execute_vacuum_full){
                        continue;
                    }

                    RegisterLog(Level.Info, execution_id, $"Starting Table Recycling on table '{table.table_name}'... ");

                    if(table.delete && DateTime.Now.Minute == 0) DeletingRecords(db, table);
                    
                    if(execute_vacuum) VacuumTable(db, table);
                    
                    //jump the vacuum full if vacuum was executed
                    if(execute_vacuum_full && !execute_vacuum) VacuumFullTable(db, table);

                    RegisterLog(Level.Info, execution_id, $"Finished Table Recycling on table '{table.table_name}'");
                }
            
                
                Thread.Sleep(1000*60); //each 1 minute
            }
        }
    }


    private void DeletingRecords(DBRepository db, ConfigTableObject table)
    {
        DateTime days_before = DateTime.UtcNow.AddDays(-table.delete_input);
        try
        {
            RegisterLog(Level.Info, execution_id, $"Deleting rows from table '{table.table_name}' added before {days_before.ToString("yyyy/MM/dd")} ... ");
            db.DeleteRecords(table.table_name, days_before);
            RegisterLog(Level.Info, execution_id, $"Deleting rows from table '{table.table_name}' added before {days_before.ToString("yyyy/MM/dd")} ... Ok");
        }
        catch (System.Exception error)
        {
            string msg = error.Message;
            if(error.InnerException != null){
                msg += error.InnerException.Message;
            }
            RegisterLog(Level.Error, execution_id, $"Deleting rows from table '{table.table_name}' added before {days_before.ToString("yyyy/MM/dd")} ... Some error was got -> {msg}");
        }
    }

    private void VacuumTable(DBRepository db, ConfigTableObject table)
    {
        try
        {
            RegisterLog(Level.Info, execution_id, $"Vacuuming table '{table.table_name}' ... ");
            db.VacuumTable(table.table_name);
            RegisterLog(Level.Info, execution_id, $"Vacuuming table '{table.table_name}' ...  Ok");
        }
        catch (System.Exception error)
        {
            string msg = error.Message;
            if(error.InnerException != null){
                msg += error.InnerException.Message;
            }
            RegisterLog(Level.Error, execution_id, $"Vacuuming table '{table.table_name}' ...  Some error was got -> {msg}");
        }
    }

    private void VacuumFullTable(DBRepository db, ConfigTableObject table)
    {
        try
        {
            RegisterLog(Level.Info, execution_id, $"Vacuuming fully table '{table.table_name}' (this may take a long time and the table will be locked) ... ");
            db.VacuumFullTable(table.table_name);
            RegisterLog(Level.Info, execution_id, $"Vacuuming fully table '{table.table_name}' ... Ok");
        }
        catch (System.Exception error)
        {
            string msg = error.Message;
            if(error.InnerException != null){
                msg += error.InnerException.Message;
            }
            RegisterLog(Level.Error, execution_id, $"Vacuuming full table '{table.table_name}' ... Some error was got -> {msg}");
        }
    }

    private bool ValidateCron(string cron)
    {
    
        var cronExpression = CronExpression.Parse(cron);

        var next = cronExpression.GetNextOccurrence(DateTime.UtcNow);
        if (next.HasValue && next.Value <= DateTime.UtcNow.AddMinutes(1))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    private void RegisterLog(Level level, string execution_id, string log)
    {
        Console.WriteLine($"{level.ToString()}: {execution_id} -> {log}");


        HttpContent content = new StringContent($"\"{log}\"", Encoding.UTF8, "application/json");
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,"/LogCenter_JobExecution")
        {
            Content = content
        };
        request.Headers.Add("description", execution_id);
        request.Headers.Add("level", level.ToString());

        try
        {
            HttpResponseMessage response = _client.SendAsync(request).Result;
        }
        catch (System.Exception error)
        {
            string msg = error.Message;
            if(error.InnerException != null){
                msg += error.InnerException.Message;
            }
            Console.WriteLine(msg);
        }
		
    }
}
