using System.Collections.Generic;
using Newtonsoft.Json;

namespace XTuleap
{
    /// <summary>
    /// Enumeration of all managed tracker field types.
    /// </summary>
    public enum TrackerFieldType
    {
        Int,
        Aid,
        Float,
        String,
        Text,
        SingleChoice,
        MultipleChoice,
        DateTime,
        ArtifactLinks,
        Cross,
        Radio,
        Unknown,
    }

    /// <summary>
    /// This class represents a tracker field.
    /// </summary>
    public class TrackerField
    {
        private static Dictionary<string, TrackerFieldType> msTypes = new Dictionary<string, TrackerFieldType>();

        static TrackerField()
        {
            msTypes.Add("int", TrackerFieldType.Int);
            msTypes.Add("aid", TrackerFieldType.Aid);
            msTypes.Add("float", TrackerFieldType.Float);
            msTypes.Add("string", TrackerFieldType.String);
            msTypes.Add("text", TrackerFieldType.Text);
            msTypes.Add("sb", TrackerFieldType.SingleChoice);
            msTypes.Add("msb", TrackerFieldType.MultipleChoice);
            msTypes.Add("rb", TrackerFieldType.Radio);
            msTypes.Add("date", TrackerFieldType.DateTime);
            msTypes.Add("cross", TrackerFieldType.Cross);
            msTypes.Add("art_link", TrackerFieldType.ArtifactLinks);
        }
        
        [JsonProperty("field_id")]
        public int Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets the type as enum.
        /// </summary>
        public TrackerFieldType FieldType
        {
            get
            {
                TrackerFieldType lField;
                if (msTypes.TryGetValue(this.Type, out lField))
                {
                    return lField;
                }

                return TrackerFieldType.Unknown;
            }
        }

        [JsonProperty("values")]
        public List<EnumEntry> EnumValues { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TrackerField()
        {
            this.EnumValues = new List<EnumEntry>();
        }
    }
}
