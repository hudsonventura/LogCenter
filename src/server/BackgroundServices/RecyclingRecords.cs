
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

                    Log.RegisterLog(LogLevel.Information, execution_id, $"Starting Table Recycling on table '{table.table_name}'... ");

                    if(table.delete && DateTime.Now.Minute == 0) DeletingRecords(db, table);
                    
                    if(execute_vacuum) VacuumTable(db, table);
                    
                    //jump the vacuum full if vacuum was executed
                    if(execute_vacuum_full && !execute_vacuum) VacuumFullTable(db, table);

                    Log.RegisterLog(LogLevel.Information, execution_id, $"Finished Table Recycling on table '{table.table_name}'");
                }
            
                
                Thread.Sleep(1000*60); //each 1 minute
            }
        }
    }


    private void DeletingRecords(DBRepository db, ConfigTableObject table)
    {
        DateTime days_before = DateTime.UtcNow.AddDays(-table.delete_input);
        string executionId = Guid.NewGuid().ToString("N");
        try
        {
            Log.RegisterLog(LogLevel.Information, execution_id, $"Deleting rows from table '{table.table_name}' added before {days_before.ToString("yyyy/MM/dd")} ... ");
            db.InsertMaintenanceLog(
                RecordLevel.Information,
                $"Automatic delete started for table '{table.table_name}'",
                new
                {
                    action = "delete_started",
                    origin = "automatic",
                    table = table.table_name,
                    before_date = days_before
                },
                executionId);
            int rowsAffected = db.DeleteRecords(table.table_name, days_before);
            Log.RegisterLog(LogLevel.Information, execution_id, $"Deleting rows from table '{table.table_name}' added before {days_before.ToString("yyyy/MM/dd")} ... Ok");
            db.InsertMaintenanceLog(
                RecordLevel.Information,
                $"Automatic delete finished for table '{table.table_name}' affecting {rowsAffected} rows",
                new
                {
                    action = "delete_finished",
                    origin = "automatic",
                    table = table.table_name,
                    before_date = days_before,
                    rows_affected = rowsAffected
                },
                executionId);
        }
        catch (System.Exception error)
        {
            string msg = error.Message;
            if(error.InnerException != null){
                msg += error.InnerException.Message;
            }
            Log.RegisterLog(LogLevel.Error, execution_id, $"Deleting rows from table '{table.table_name}' added before {days_before.ToString("yyyy/MM/dd")} ... Some error was got -> {msg}");
            db.InsertMaintenanceLog(
                RecordLevel.Error,
                $"Automatic delete failed for table '{table.table_name}'",
                new
                {
                    action = "delete_failed",
                    origin = "automatic",
                    table = table.table_name,
                    before_date = days_before,
                    error = msg
                },
                executionId);
        }
    }

    private void VacuumTable(DBRepository db, ConfigTableObject table)
    {
        string executionId = Guid.NewGuid().ToString("N");
        long sizeBefore = db.GetTableSizeBytes(table.table_name);
        try
        {
            Log.RegisterLog(LogLevel.Information, execution_id, $"Vacuuming table '{table.table_name}' ... ");
            db.InsertMaintenanceLog(
                RecordLevel.Information,
                $"Automatic VACUUM started for table '{table.table_name}'",
                new
                {
                    action = "vacuum_started",
                    origin = "automatic",
                    mode = "vacuum",
                    table = table.table_name,
                    size_before_bytes = sizeBefore
                },
                executionId);
            db.VacuumTable(table.table_name);
            Log.RegisterLog(LogLevel.Information, execution_id, $"Vacuuming table '{table.table_name}' ...  Ok");
            long sizeAfter = db.GetTableSizeBytes(table.table_name);
            db.InsertMaintenanceLog(
                RecordLevel.Information,
                $"Automatic VACUUM finished for table '{table.table_name}'",
                new
                {
                    action = "vacuum_finished",
                    origin = "automatic",
                    mode = "vacuum",
                    table = table.table_name,
                    size_before_bytes = sizeBefore,
                    size_after_bytes = sizeAfter,
                    free_space_bytes = sizeBefore - sizeAfter
                },
                executionId);
        }
        catch (System.Exception error)
        {
            string msg = error.Message;
            if(error.InnerException != null){
                msg += error.InnerException.Message;
            }
            Log.RegisterLog(LogLevel.Error, execution_id, $"Vacuuming table '{table.table_name}' ...  Some error was got -> {msg}");
            db.InsertMaintenanceLog(
                RecordLevel.Error,
                $"Automatic VACUUM failed for table '{table.table_name}'",
                new
                {
                    action = "vacuum_failed",
                    origin = "automatic",
                    mode = "vacuum",
                    table = table.table_name,
                    size_before_bytes = sizeBefore,
                    error = msg
                },
                executionId);
        }
    }

    private void VacuumFullTable(DBRepository db, ConfigTableObject table)
    {
        string executionId = Guid.NewGuid().ToString("N");
        long sizeBefore = db.GetTableSizeBytes(table.table_name);
        try
        {
            Log.RegisterLog(LogLevel.Information, execution_id, $"Vacuuming fully table '{table.table_name}' (this may take a long time and the table will be locked) ... ");
            db.InsertMaintenanceLog(
                RecordLevel.Information,
                $"Automatic VACUUM FULL started for table '{table.table_name}'",
                new
                {
                    action = "vacuum_started",
                    origin = "automatic",
                    mode = "vacuum_full",
                    table = table.table_name,
                    size_before_bytes = sizeBefore
                },
                executionId);
            db.VacuumFullTable(table.table_name);
            Log.RegisterLog(LogLevel.Information, execution_id, $"Vacuuming fully table '{table.table_name}' ... Ok");
            long sizeAfter = db.GetTableSizeBytes(table.table_name);
            db.InsertMaintenanceLog(
                RecordLevel.Information,
                $"Automatic VACUUM FULL finished for table '{table.table_name}'",
                new
                {
                    action = "vacuum_finished",
                    origin = "automatic",
                    mode = "vacuum_full",
                    table = table.table_name,
                    size_before_bytes = sizeBefore,
                    size_after_bytes = sizeAfter,
                    free_space_bytes = sizeBefore - sizeAfter
                },
                executionId);
        }
        catch (System.Exception error)
        {
            string msg = error.Message;
            if(error.InnerException != null){
                msg += error.InnerException.Message;
            }
            Log.RegisterLog(LogLevel.Error, execution_id, $"Vacuuming full table '{table.table_name}' ... Some error was got -> {msg}");
            db.InsertMaintenanceLog(
                RecordLevel.Error,
                $"Automatic VACUUM FULL failed for table '{table.table_name}'",
                new
                {
                    action = "vacuum_failed",
                    origin = "automatic",
                    mode = "vacuum_full",
                    table = table.table_name,
                    size_before_bytes = sizeBefore,
                    error = msg
                },
                executionId);
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
