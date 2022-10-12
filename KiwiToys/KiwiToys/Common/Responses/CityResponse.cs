using Newtonsoft.Json;

namespace KiwiToys.Common.Responses {
    public class CityResponse {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}