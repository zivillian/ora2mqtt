using System.Text.Json.Serialization;

namespace libgwmapi.DTO.UserAuth;

public class LoginAccountResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("accountExist")]
    public string AccountExist { get; set; }

    [JsonPropertyName("beanId")]
    public string BeanId { get; set; }

    [JsonPropertyName("gwId")]
    public string GwId { get; set; }

    [JsonPropertyName("isProtocols")]
    public bool IsProtocols { get; set; }

    [JsonPropertyName("newUserFlag")]
    public int NewUserFlag { get; set; }

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("userState")]
    public string UserState { get; set; }
}