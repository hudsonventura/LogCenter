
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

                    Log.RegisterLog(Level.Info, execution_id, $"Starting Table Recycling on table '{table.table_name}'... ");

                    if(table.delete && DateTime.Now.Minute == 0) DeletingRecords(db, table);
                    
                    if(execute_vacuum) VacuumTable(db, table);
                    
                    //jump the vacuum full if vacuum was executed
                    if(execute_vacuum_full && !execute_vacuum) VacuumFullTable(db, table);

                    Log.RegisterLog(Level.Info, execution_id, $"Finished Table Recycling on table '{table.table_name}'");
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
            Log.RegisterLog(Level.Info, execution_id, $"Deleting rows from table '{table.table_name}' added before {days_before.ToString("yyyy/MM/dd")} ... ");
            db.DeleteRecords(table.table_name, days_before);
            Log.RegisterLog(Level.Info, execution_id, $"Deleting rows from table '{table.table_name}' added before {days_before.ToString("yyyy/MM/dd")} ... Ok");
        }
        catch (System.Exception error)
        {
            string msg = error.Message;
            if(error.InnerException != null){
                msg += error.InnerException.Message;
            }
            Log.RegisterLog(Level.Error, execution_id, $"Deleting rows from table '{table.table_name}' added before {days_before.ToString("yyyy/MM/dd")} ... Some error was got -> {msg}");
        }
    }

    private void VacuumTable(DBRepository db, ConfigTableObject table)
    {
        try
        {
            Log.RegisterLog(Level.Info, execution_id, $"Vacuuming table '{table.table_name}' ... ");
            db.VacuumTable(table.table_name);
            Log.RegisterLog(Level.Info, execution_id, $"Vacuuming table '{table.table_name}' ...  Ok");
        }
        catch (System.Exception error)
        {
            string msg = error.Message;
            if(error.InnerException != null){
                msg += error.InnerException.Message;
            }
            Log.RegisterLog(Level.Error, execution_id, $"Vacuuming table '{table.table_name}' ...  Some error was got -> {msg}");
        }
    }

    private void VacuumFullTable(DBRepository db, ConfigTableObject table)
    {
        try
        {
            Log.RegisterLog(Level.Info, execution_id, $"Vacuuming fully table '{table.table_name}' (this may take a long time and the table will be locked) ... ");
            db.VacuumFullTable(table.table_name);
            Log.RegisterLog(Level.Info, execution_id, $"Vacuuming fully table '{table.table_name}' ... Ok");
        }
        catch (System.Exception error)
        {
            string msg = error.Message;
            if(error.InnerException != null){
                msg += error.InnerException.Message;
            }
            Log.RegisterLog(Level.Error, execution_id, $"Vacuuming full table '{table.table_name}' ... Some error was got -> {msg}");
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


    
}
