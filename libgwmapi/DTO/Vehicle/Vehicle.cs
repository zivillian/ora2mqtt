using System.Text.Json.Serialization;

namespace libgwmapi.DTO.Vehicle
{
    public class Vehicle
    {
        [JsonPropertyName("agreementVersion")]
        public string AgreementVersion { get; set; }

        [JsonPropertyName("airConditionModel")]
        public object AirConditionModel { get; set; }

        [JsonPropertyName("authInfo")]
        public object AuthInfo { get; set; }

        [JsonPropertyName("belongPlatform")]
        public string BelongPlatform { get; set; }

        [JsonPropertyName("bluetoothAbility")]
        public object BluetoothAbility { get; set; }

        [JsonPropertyName("bluetoothBind")]
        public object BluetoothBind { get; set; }

        [JsonPropertyName("bluetoothInduction")]
        public object BluetoothInduction { get; set; }

        [JsonPropertyName("bluetoothKey")]
        public object BluetoothKey { get; set; }

        [JsonPropertyName("brandCode")]
        public string BrandCode { get; set; }

        [JsonPropertyName("brandName")]
        public string BrandName { get; set; }

        [JsonPropertyName("buyDate")]
        public object BuyDate { get; set; }

        [JsonPropertyName("canSignalType")]
        public string CanSignalType { get; set; }

        [JsonPropertyName("carType")]
        public object CarType { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("colorUrl")]
        public string ColorUrl { get; set; }

        [JsonPropertyName("colorValue")]
        public object ColorValue { get; set; }

        [JsonPropertyName("config")]
        public string Config { get; set; }

        [JsonPropertyName("customizedCarUrl")]
        public object CustomizedCarUrl { get; set; }

        [JsonPropertyName("dealerName")]
        public object DealerName { get; set; }

        [JsonPropertyName("defaultVehicle")]
        public bool DefaultVehicle { get; set; }

        [JsonPropertyName("eCallServiceEndDate")]
        public object ECallServiceEndDate { get; set; }

        [JsonPropertyName("eCallServiceRemainDays")]
        public object ECallServiceRemainDays { get; set; }

        [JsonPropertyName("eCallServiceStartDate")]
        public object ECallServiceStartDate { get; set; }

        [JsonPropertyName("eCallServiceStatus")]
        public object ECallServiceStatus { get; set; }

        [JsonPropertyName("engineNo")]
        public string EngineNo { get; set; }

        [JsonPropertyName("engineType")]
        public string EngineType { get; set; }

        [JsonPropertyName("etype")]
        public int Etype { get; set; }

        [JsonPropertyName("hasIdNo")]
        public object HasIdNo { get; set; }

        [JsonPropertyName("hasScyPwd")]
        public object HasScyPwd { get; set; }

        [JsonPropertyName("hasSsWin")]
        public object HasSsWin { get; set; }

        [JsonPropertyName("hasWinControl")]
        public object HasWinControl { get; set; }

        [JsonPropertyName("imageUrl")]
        public object ImageUrl { get; set; }

        [JsonPropertyName("imsi")]
        public string Imsi { get; set; }

        [JsonPropertyName("isEcallforever")]
        public object IsEcallforever { get; set; }

        [JsonPropertyName("lastUpdate")]
        public object LastUpdate { get; set; }

        [JsonPropertyName("lat")]
        public object Lat { get; set; }

        [JsonPropertyName("licenseNumber")]
        public string LicenseNumber { get; set; }

        [JsonPropertyName("lon")]
        public object Lon { get; set; }

        [JsonPropertyName("material45Url")]
        public object Material45Url { get; set; }

        [JsonPropertyName("material90Url")]
        public object Material90Url { get; set; }

        [JsonPropertyName("materialBgUrl")]
        public object MaterialBgUrl { get; set; }

        [JsonPropertyName("materialCode")]
        public object MaterialCode { get; set; }

        [JsonPropertyName("minImageUrl")]
        public string MinImageUrl { get; set; }

        [JsonPropertyName("modelCode")]
        public string ModelCode { get; set; }

        [JsonPropertyName("modelName")]
        public string ModelName { get; set; }

        [JsonPropertyName("otBrandName")]
        public string OtBrandName { get; set; }

        [JsonPropertyName("ownerModel")]
        public object OwnerModel { get; set; }

        [JsonPropertyName("ownership")]
        public int Ownership { get; set; }

        [JsonPropertyName("realNameAuth")]
        public string RealNameAuth { get; set; }

        [JsonPropertyName("remote")]
        public object Remote { get; set; }

        [JsonPropertyName("rudder")]
        public string Rudder { get; set; }

        [JsonPropertyName("salesMarket")]
        public string SalesMarket { get; set; }

        [JsonPropertyName("serviceType")]
        public object ServiceType { get; set; }

        [JsonPropertyName("shareCount")]
        public object ShareCount { get; set; }

        [JsonPropertyName("shareId")]
        public object ShareId { get; set; }

        [JsonPropertyName("showedVin")]
        public string ShowedVin { get; set; }

        [JsonPropertyName("signAgreementState")]
        public string SignAgreementState { get; set; }

        [JsonPropertyName("simIccid")]
        public string SimIccid { get; set; }

        [JsonPropertyName("styleName")]
        public string StyleName { get; set; }

        [JsonPropertyName("tServiceEndDate")]
        public long TServiceEndDate { get; set; }

        [JsonPropertyName("tServiceStartDate")]
        public long TServiceStartDate { get; set; }

        [JsonPropertyName("tServiceStatus")]
        public string TServiceStatus { get; set; }

        [JsonPropertyName("tankCapacity")]
        public object TankCapacity { get; set; }

        [JsonPropertyName("telematicsType")]
        public object TelematicsType { get; set; }

        [JsonPropertyName("type")]
        public object Type { get; set; }

        [JsonPropertyName("vCode")]
        public string VCode { get; set; }

        [JsonPropertyName("vTypeName")]
        public string VTypeName { get; set; }

        [JsonPropertyName("vehicleId")]
        public string VehicleId { get; set; }

        [JsonPropertyName("vehicleNick")]
        public object VehicleNick { get; set; }

        [JsonPropertyName("vehicleSts")]
        public object VehicleSts { get; set; }

        [JsonPropertyName("vin")]
        public string Vin { get; set; }

        [JsonPropertyName("vtype")]
        public string Vtype { get; set; }
    }
}
