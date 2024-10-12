namespace server.Domain;

public class ConfigTableObject
{
    /// <summary>
    /// Able auto DELETE rows table older than a number of days
    /// </summary>
    public bool delete { get; set; } = false;

    /// <summary>
    /// Number of days to auto delete rows
    /// </summary>
    public int delete_input { get; set; } = 0; // 1x por ano, no dia 1 de janeiro as 00:00

    /// <summary>
    /// Able VACUUM to remove deleted rows and reuse the space
    /// </summary>
    public bool vacuum { get; set; } = false;

    /// <summary>
    /// Schedulle to VACUUM
    /// </summary>
    public string vacuum_input { get; set; } = "0 0 1 1 *";


    /// <summary>
    /// Able VACUUM FULL ANALYSE to remove deleted rows and claim free space on disk
    /// </summary>
    public bool vacuum_full { get; set; } = false;

    /// <summary>
    /// Schedulle to VACUUM FULL ANALYSE
    /// </summary>
    public string vacuum_full_input { get; set; } = "0 0 1 1 *";
}
