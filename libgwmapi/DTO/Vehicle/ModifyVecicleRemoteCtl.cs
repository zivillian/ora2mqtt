using System.Text.Json.Serialization;

namespace libgwmapi.DTO.Vehicle;

public class ModifyVecicleRemoteCtl
{
    [JsonPropertyName("airConditionerTemperature")]
    public string AirConditionerTemperature { get; set; }

    [JsonPropertyName("airConditionerTime")]
    public string AirConditionerTime { get; set; }

    [JsonPropertyName("vin")]
    public string Vin { get; set; }
}