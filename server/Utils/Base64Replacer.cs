using System;
using System.Collections.Generic;
using System.Text.Json;
namespace server.Utils;

/// <summary>
/// Remove and replace base64 file content
/// </summary>
public class Base64Replacer
{
    /// <summary>
    /// Remove and replace base64 file content
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static JsonElement ReplaceBase64Content(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Undefined)
            return element;

        var jsonObject = new Dictionary<string, object>();

        foreach (var property in element.EnumerateObject())
        {
            var value = property.Value.ToString();
            Console.WriteLine(value);
            if (value.StartsWith("data:"))
            {
                // Substituir o base64 por um texto explicativo
                jsonObject[property.Name] = "Base64 file content was removed";
            }
            else if (value.Length > 1024)
            {
                // Substituir o base64 por um texto explicativo
                jsonObject[property.Name] = "Large content was removed (more than 1024KB)";
            }
            else if (property.Value.ValueKind == JsonValueKind.Object)
            {
                // Chamar recursivamente para subobjetos
                jsonObject[property.Name] = ReplaceBase64Content(property.Value);
            }
            else
            {
                jsonObject[property.Name] = property.Value.Clone(); // Clone para manter o valor original
            }
        }

        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(jsonObject));
    }


}
