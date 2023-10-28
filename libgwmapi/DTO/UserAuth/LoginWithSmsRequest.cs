using System.Text.Json.Serialization;

namespace libgwmapi.DTO.UserAuth;

public class LoginWithSmsRequest
{
    [JsonPropertyName("agreement")]
    public int[] Agreement { get; set; } = { 1, 2, 23 };

    [JsonPropertyName("appType")]
    public int AppType { get; set; } = 0;

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("pushToken")]
    public string PushToken { get; set; }

    [JsonPropertyName("smsCode")]
    public string SmsCode { get; set; }

}