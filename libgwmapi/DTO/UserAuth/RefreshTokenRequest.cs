using System.Text.Json.Serialization;

namespace libgwmapi.DTO.UserAuth;

public class RefreshTokenRequest
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; }
}