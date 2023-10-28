using System.Text.Json.Serialization;

namespace libgwmapi.DTO.UserAuth;

public class LoginAccountRequest
{
    [JsonPropertyName("account")]
    public string Account { get; set; }

    [JsonPropertyName("agreement")]
    public int[] Agreement { get; set; } = { 1, 2, 23 };

    [JsonPropertyName("appType")]
    public int AppType { get; set; } = 0;

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; }

    [JsonPropertyName("isEncrypt")]
    public bool IsEncrypt { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("pushToken")]
    public string PushToken { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; } = 1;
}