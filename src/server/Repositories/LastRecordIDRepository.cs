using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace server.Repositories;

/// <summary>
/// It's used to identify a new record added to a table
/// </summary>
public class LastRecordIDRepository
{
    private static Dictionary<string, Guid> List { get; set; } = new Dictionary<string, Guid>();

    private LastRecordIDRepository(){}
    private static LastRecordIDRepository instance = new LastRecordIDRepository();

    public static LastRecordIDRepository Instance = new LastRecordIDRepository();
    
    public LastRecordIDRepository(NpgsqlConnection conn){
        _db = new DBRepository(conn);
    }
    private DBRepository _db;

    public Guid GetMaxID(string table){
        try
        {
            return List.Where(x => x.Key == table).Max(x => x.Value);
        }
        catch (System.Exception)
        {
            try
            {
                var last = _db.GetMaxID(table);
            SetMaxID(table, last);
            return last;
            }
            catch (System.Exception)
            {
                throw new Exception($"Doesn't exist a table named '{table}' or the table is empty");
            }
        }
        
        
        
    }

    public void SetMaxID(string table, Guid id){
        try
        {
            List.Add(table, id);
            return;
        }
        catch (System.Exception)
        {
            List[table] = id;
        }
        
    }
}
