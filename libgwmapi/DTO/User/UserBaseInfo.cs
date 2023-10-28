using System.Text.Json.Serialization;

namespace libgwmapi.DTO.User
{
    public class UserBaseInfo
    {
        [JsonPropertyName("attentionStatus")]
        public string AttentionStatus { get; set; }

        [JsonPropertyName("avoidClose")]
        public string AvoidClose { get; set; }

        [JsonPropertyName("badge")]
        public string Badge { get; set; }

        [JsonPropertyName("beanId")]
        public string BeanId { get; set; }

        [JsonPropertyName("concernNumber")]
        public int ConcernNumber { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("fansNumber")]
        public int FansNumber { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("followStatus")]
        public int FollowStatus { get; set; }

        [JsonPropertyName("gender")]
        public int Gender { get; set; }

        [JsonPropertyName("gwId")]
        public string GwId { get; set; }

        [JsonPropertyName("isMuted")]
        public bool IsMuted { get; set; }

        [JsonPropertyName("isSecurityPasswordExist")]
        public int IsSecurityPasswordExist { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("officialAccount")]
        public string OfficialAccount { get; set; }

        [JsonPropertyName("postsNumber")]
        public int PostsNumber { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("registrationTime")]
        public string RegistrationTime { get; set; }

        [JsonPropertyName("replyNumber")]
        public int ReplyNumber { get; set; }

        [JsonPropertyName("securityTime")]
        public int SecurityTime { get; set; }

        [JsonPropertyName("userIdentity")]
        public string UserIdentity { get; set; }

        [JsonPropertyName("userImgFlag")]
        public string UserImgFlag { get; set; }

        [JsonPropertyName("userLabel")]
        public int UserLabel { get; set; }

        [JsonPropertyName("userLevel")]
        public int UserLevel { get; set; }
    }
}
