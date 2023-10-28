using System.Text.Json.Serialization;

namespace libgwmapi.DTO.Country;

public class Country
{
    [JsonPropertyName("regionCode")]
    public string RegionCode { get; set; }

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; }

    [JsonPropertyName("countryName")]
    public string CountryName { get; set; }

    [JsonPropertyName("abbreviation")]
    public string Abbreviation { get; set; }
}