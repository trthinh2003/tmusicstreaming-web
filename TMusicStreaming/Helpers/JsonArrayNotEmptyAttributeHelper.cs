using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class JsonArrayNotEmptyAttributeHelper : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        var json = value as string;
        if (string.IsNullOrWhiteSpace(json)) return false;

        try
        {
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            return element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0;
        }
        catch
        {
            return false;
        }
    }
}
