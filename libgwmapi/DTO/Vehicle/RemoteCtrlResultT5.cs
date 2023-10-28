using System.Text.Json.Serialization;

namespace libgwmapi.DTO.Vehicle;

public class RemoteCtrlResultT5
{
    [JsonPropertyName("anticipationCode")]
    public object AnticipationCode { get; set; }

    [JsonPropertyName("commandType")]
    public object CommandType { get; set; }

    [JsonPropertyName("hwCommandId")]
    public string HwCommandId { get; set; }

    [JsonPropertyName("hwResult")]
    public int? HwResult { get; set; }

    [JsonPropertyName("remoteType")]
    public string RemoteType { get; set; }

    [JsonPropertyName("resultCode")]
    public string ResultCode { get; set; }

    [JsonPropertyName("resultMsg")]
    public string ResultMsg { get; set; }

    [JsonPropertyName("vehicleId")]
    public long VehicleId { get; set; }
}