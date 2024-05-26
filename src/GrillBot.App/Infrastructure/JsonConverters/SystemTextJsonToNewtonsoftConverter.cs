using System.Text.Json;
using System.Text.Json.Nodes;

namespace GrillBot.App.Infrastructure.JsonConverters;

public class SystemTextJsonToNewtonsoftConverter : JsonConverter<JsonElement>
{
    public override JsonElement ReadJson(JsonReader reader, Type objectType, JsonElement existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        var jObject = JToken.ReadFrom(reader);
        var json = jObject.ToString();

        using var document = JsonDocument.Parse(json);
        return document.RootElement;
    }

    public override void WriteJson(JsonWriter writer, JsonElement value, Newtonsoft.Json.JsonSerializer serializer)
    {
        var jsonObject = JsonObject.Create(value) ?? new JsonObject();
        var json = jsonObject.ToJsonString();

        JObject.Parse(json).WriteTo(writer);
    }
}
