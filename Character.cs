using Newtonsoft.Json;

namespace dc_scrapper
{
    internal class Character
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonProperty("description")]
        public List<String> Description { get; set; } = [];

        [JsonProperty("bannerImages")]
        public HashSet<string> BannerImages { get; set; } = [];

        [JsonProperty("relatedCharacters")]
        public List<Related> RelatedCharacters { get; set; } = [];

        [JsonProperty("facts")]
        public List<KeyValuePair<string, string>> Facts { get; set; } = new List<KeyValuePair<string, string>>();
    }

    internal class Related
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonProperty("uri")]
        public String Uri { get; set; }
    }
}
