using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FamilyApp.Common.Dtos
{
    public class OuthGoogleDto
    {
        public class GoogleTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("id_token")]
            public string IdToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }
        }

        public class GoogleCertsResponse
        {
            [JsonPropertyName("keys")]
            public List<GoogleCertKey> Keys { get; set; }
        }

        public class GoogleCertKey
        {
            [JsonPropertyName("kty")]
            public string Kty { get; set; }

            [JsonPropertyName("use")]
            public string Use { get; set; }

            [JsonPropertyName("kid")]
            public string Kid { get; set; }

            [JsonPropertyName("alg")]
            public string Alg { get; set; }

            [JsonPropertyName("n")]
            public string N { get; set; }

            [JsonPropertyName("e")]
            public string E { get; set; }
        }
    }
}
