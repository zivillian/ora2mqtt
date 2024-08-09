using System.Text.Json.Serialization;

namespace libgwmapi.DTO.Vehicle;

public class VehicleStatus
{
    [JsonPropertyName("acquisitionTime")]
    public long AcquisitionTime { get; set; }

    [JsonPropertyName("charge")]
    public object Charge { get; set; }

    [JsonPropertyName("command")]
    public object Command { get; set; }

    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; }

    [JsonPropertyName("deviceType")]
    public object DeviceType { get; set; }

    [JsonPropertyName("globalStatusList")]
    public object[] GlobalStatusList { get; set; }

    [JsonPropertyName("id")]
    public object Id { get; set; }

    [JsonPropertyName("items")]
    public VehicleStatusItems[] Items { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("oilQty")]
    public object OilQty { get; set; }

    [JsonPropertyName("percentageOfOil")]
    public object PercentageOfOil { get; set; }

    [JsonPropertyName("updateTime")]
    public long UpdateTime { get; set; }

    [JsonPropertyName("uploadTime")]
    public object UploadTime { get; set; }
}

public class VehicleStatusItems
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("overstep")]
    public object Overstep { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; }

    //sometimes int, sometimes double, sometimes string
    [JsonPropertyName("value")]
    public object Value { get; set; }

}