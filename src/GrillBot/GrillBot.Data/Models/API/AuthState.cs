using Newtonsoft.Json;
using System;
using System.Text;

namespace GrillBot.Data.Models.API;

public class AuthState
{
    [JsonProperty("p")]
    public bool IsPublic { get; set; }

    [JsonProperty("r")]
    public string ReturnUrl { get; set; }

    public string Encode()
    {
        var json = JsonConvert.SerializeObject(this);
        var bytes = Encoding.UTF8.GetBytes(json);

        return Convert.ToBase64String(bytes);
    }

    public static AuthState Decode(string encodedData)
    {
        if (string.IsNullOrEmpty(encodedData))
            throw new ArgumentException("No data provided for state decoding.");

        var bytes = Convert.FromBase64String(encodedData);
        var json = Encoding.UTF8.GetString(bytes);

        return JsonConvert.DeserializeObject<AuthState>(json);
    }
}
