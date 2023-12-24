using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace GrillBot.Data.Models.API;

public class AuthState
{
    [JsonProperty("p")]
    public bool IsPublic { get; set; }

    [JsonProperty("r")]
    public string? ReturnUrl { get; set; }

    public string Encode()
    {
        var json = JsonConvert.SerializeObject(this);
        var bytes = Encoding.UTF8.GetBytes(json);

        return Convert.ToBase64String(bytes);
    }

    public static bool TryDecode(string encodedData, [MaybeNullWhen(false)] out AuthState state)
    {
        state = null;
        if (string.IsNullOrEmpty(encodedData))
            return false;

        try
        {
            var bytes = Convert.FromBase64String(encodedData);
            var json = Encoding.UTF8.GetString(bytes);

            state = JsonConvert.DeserializeObject<AuthState>(json)!;
            return true;
        }
        catch (JsonReaderException)
        {
            return false;
        }
    }
}
