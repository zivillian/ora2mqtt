using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace libgwmapi.DTO.UserAuth;

public class CheckSecurityPassword
{
    public CheckSecurityPassword(string pin)
    {
        using (var md5 = MD5.Create())
        {
            //yes, it's case sensitive
            Md5Hash = Convert.ToHexString(md5.ComputeHash(Encoding.ASCII.GetBytes(pin))).ToLower();
        }
    }

    [JsonPropertyName("securityPassword")]
    public string Md5Hash { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "2";
}