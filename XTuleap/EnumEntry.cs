using Newtonsoft.Json;

namespace XTuleap
{
    public class EnumEntry
    {
        [JsonProperty("id")]
        public int Id
        {
            get;
            set;
        }

        [JsonProperty("label")]
        public string Label
        {
            get;
            set;
        }
    }
}
