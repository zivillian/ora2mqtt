using System.Text.Json.Serialization;

namespace libgwmapi.DTO.UserAuth;

public class AddAppDevice
{
    [JsonPropertyName("appCountry")]
    public string Country { get; set; } = "-";

    [JsonPropertyName("appVersion")]
    public string Version { get; set; } = "-";

    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; }

    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; } = "0";

    [JsonPropertyName("deviceVersion")]
    public string DeviceVersion { get; set; } = "-";

    [JsonPropertyName("gwId")]
    public string GwId { get; set; }

    [JsonPropertyName("operatingSystemType")]
    public string OsType { get; set; } = "-";

    [JsonPropertyName("operatingSystemVersion")]
    public string OsVersion { get; set; } = "-";

    [JsonPropertyName("systemVersionNumber")]
    public int SystemVersion { get; set; }
}