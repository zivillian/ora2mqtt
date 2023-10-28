using System.Text.Json.Serialization;

namespace libgwmapi.DTO.UserAuth
{
    public class CustomerServicePhone
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }
    }
}
