using System.Text.Json.Serialization;

namespace libgwmapi.DTO.Vehicle;

public class VehicleBasicsInfo
{
    [JsonPropertyName("appFenceVO")]
    public object AppFenceVo { get; set; }

    [JsonPropertyName("config")]
    public VehicleConfig Config { get; set; }

    [JsonPropertyName("score")]
    public object Score { get; set; }

    [JsonPropertyName("subscribe")]
    public string Subscribe { get; set; }

    [JsonPropertyName("time")]
    public object Time { get; set; }
}

public class VehicleConfig
{
    [JsonPropertyName("airConditionerStatusTime")]
    public string AirConditionerStatusTime { get; set; }

    [JsonPropertyName("airConditionerTemperature")]
    public string AirConditionerTemperature { get; set; }

    [JsonPropertyName("airPurifierStatus")]
    public int AirPurifierStatus { get; set; }

    [JsonPropertyName("backDefrostStatus")]
    public int BackDefrostStatus { get; set; }

    [JsonPropertyName("blowingMode")]
    public string BlowingMode { get; set; }

    [JsonPropertyName("cabinCleanNum")]
    public int CabinCleanNum { get; set; }

    [JsonPropertyName("cabinCleanTime")]
    public object CabinCleanTime { get; set; }

    [JsonPropertyName("defrostTime")]
    public string DefrostTime { get; set; }

    [JsonPropertyName("engineStatusTime")]
    public string EngineStatusTime { get; set; }

    [JsonPropertyName("frontDefrostStatus")]
    public int FrontDefrostStatus { get; set; }

    [JsonPropertyName("leftBackSeat")]
    public int LeftBackSeat { get; set; }

    [JsonPropertyName("leftBackWindow")]
    public int LeftBackWindow { get; set; }

    [JsonPropertyName("leftFrontSeat")]
    public int LeftFrontSeat { get; set; }

    [JsonPropertyName("leftFrontWindow")]
    public int LeftFrontWindow { get; set; }

    [JsonPropertyName("powerGear")]
    public string PowerGear { get; set; }

    [JsonPropertyName("purifierTime")]
    public string PurifierTime { get; set; }

    [JsonPropertyName("rightBackSeat")]
    public int RightBackSeat { get; set; }

    [JsonPropertyName("rightBackWindow")]
    public int RightBackWindow { get; set; }

    [JsonPropertyName("rightFrontSeat")]
    public int RightFrontSeat { get; set; }

    [JsonPropertyName("rightFrontWindow")]
    public int RightFrontWindow { get; set; }

    [JsonPropertyName("seatHeatingControlTime")]
    public string SeatHeatingControlTime { get; set; }

    [JsonPropertyName("seatHeatingType")]
    public string SeatHeatingType { get; set; }

    [JsonPropertyName("shadeScreen")]
    public int ShadeScreen { get; set; }

    [JsonPropertyName("skyLight")]
    public int SkyLight { get; set; }

    [JsonPropertyName("steeringWheelHeatingTime")]
    public int SteeringWheelHeatingTime { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("vin")]
    public string Vin { get; set; }

}