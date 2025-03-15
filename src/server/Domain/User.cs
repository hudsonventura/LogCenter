using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace server.Domain;

#pragma warning disable IDE1006 // Naming Styles
[Table("users")]
#pragma warning restore IDE1006 // Naming Styles
public class User
{
    public Guid id { get; internal set; } = SnowflakeGuid.Create().ToGuid();
    public string name { get; internal set; }
    public string email { get; internal set; }

    [JsonIgnore]
    public string password { get; private set; }

    public void SetPassword(string password){
        this.password = HashPassword(password);
    }
    public bool CheckPassword(string password)
    {
        return this.password == HashPassword(password);
    }
    
    private string HashPassword(string password){
        return HashPassword(id, password);
    }

    public static string HashPassword(Guid id, string password){
        var passwordWithId = id.ToString() + password + id.ToString();
        var bytes = System.Text.Encoding.UTF8.GetBytes(passwordWithId);
        var hash = System.Security.Cryptography.SHA512.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    internal static void LoginOrPasswordIncorrect()
    {
        throw new Exception("Email or password is incorrect");
    }
    
}
