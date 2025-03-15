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
    public static dynamic ReplaceBase64Content(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Undefined)
            return element;

        var jsonObject = new Dictionary<string, object>();

        foreach (var property in element.EnumerateObject())
        {
            var value = property.Value.ToString();

            if (value.StartsWith("data:"))
            {
                // Substituir o base64 por um texto explicativo
                jsonObject[property.Name] = $"{value.Substring(0, 950)} ... Base64 file content was droped";
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                // Se for uma lista, iterar sobre cada item
                var array = new List<object>();
                foreach (var item in property.Value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        // Chamar recursivamente para subobjetos
                        array.Add(ReplaceBase64Content(item));
                    }
                    else
                    {
                        array.Add(item.Clone()); // Clone para manter o valor original
                    }
                }
                jsonObject[property.Name] = array; // Não serializar para string, manter como objeto
            }
            else if (value.Length > 1024)
            {
                if (value.StartsWith("{") && value.EndsWith("}"))
                {
                    var jsonElement = JsonDocument.Parse(value).RootElement;
                    jsonObject[property.Name] = ReplaceBase64Content(jsonElement);
                }
                else
                {
                    jsonObject[property.Name] = $"{value.Substring(0, 950)} ... Large content was droped (more than 1024KB)";
                }
            }
            else if (property.Value.ValueKind == JsonValueKind.Object)
            {
                if (value.StartsWith("{") && value.EndsWith("}"))
                {
                    var jsonElement = JsonDocument.Parse(value).RootElement;
                    jsonObject[property.Name] = ReplaceBase64Content(jsonElement);
                }
                else
                {
                    jsonObject[property.Name] = ReplaceBase64Content(property.Value);
                }
                
            }
            else
            {
                if (value.StartsWith("{") && value.EndsWith("}"))
                {
                    var jsonElement = JsonDocument.Parse(value).RootElement;
                    jsonObject[property.Name] = ReplaceBase64Content(jsonElement);
                }
                else
                {
                    jsonObject[property.Name] = property.Value.Clone(); // Clone para manter o valor original
                }
                
            }
        }

        // Retornar o objeto JSON modificado como um JsonElement
        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(jsonObject));
    }



}
