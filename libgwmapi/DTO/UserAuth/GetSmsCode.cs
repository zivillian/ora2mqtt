using System.Text.Json.Serialization;

namespace libgwmapi.DTO.UserAuth;

public class GetSmsCode
{
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("scenario")]
    public int Scenario { get; set; } = 0;

    [JsonPropertyName("type")]
    public int Type { get; set; } = 3;
}