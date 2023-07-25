using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace XTuleap
{
    /// <summary>
    ///     Enumeration of all managed tracker field types.
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
        Unknown
    }

    /// <summary>
    ///     This class represents a tracker field.
    /// </summary>
    public class TrackerField
    {
        private static readonly Dictionary<string?, TrackerFieldType> msTypes =
            new Dictionary<string?, TrackerFieldType>();

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

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public TrackerField()
        {
            this.EnumValues = new List<EnumEntry>();
        }

        [JsonProperty("field_id")]
        public int Id
        {
            get;
            set;
        }

        [JsonProperty("label")]
        public string? Label
        {
            get;
            set;
        }

        [JsonProperty("name")]
        public string? Name
        {
            get;
            set;
        }

        [JsonProperty("type")]
        public string? Type
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets the type as enum.
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
        public List<EnumEntry> EnumValues
        {
            get;
            set;
        }

        /// <summary>
        ///     This method encode a value in to value_field JSON format
        /// </summary>
        /// <param name="pValue">The value to encode.</param>
        /// <returns>The encoded value.</returns>
        public string EncodeValueField(object pValue)
        {
            switch (this.FieldType)
            {
                case TrackerFieldType.Int:
                {
                    int lValue = (int) pValue;
                    return "  {  \"field_id\": " + this.Id + ", \"value\": " +
                           lValue.ToString(CultureInfo.InvariantCulture) + "  }";
                }

                case TrackerFieldType.Float:
                {
                    double lValue = (double) pValue;
                    return "  {  \"field_id\": " + this.Id + ", \"value\": " +
                           lValue.ToString(CultureInfo.InvariantCulture) + "  }";
                }

                case TrackerFieldType.DateTime:
                {
                    DateTime lValue = (DateTime) pValue;
                    return "  {  \"field_id\": " + this.Id + ", \"value\": " + lValue.ToString("o") + "  }";
                }

                case TrackerFieldType.String:
                case TrackerFieldType.Text:
                {
                    return "  {  \"field_id\": " + this.Id + ", \"value\": \"" + pValue + "\"  }";
                }

                case TrackerFieldType.SingleChoice:
                {
                    EnumEntry lFieldValue = this.EnumValues.First(pItem => pItem.Label == pValue.ToString());
                    return "  {  \"field_id\": " + this.Id + ", \"bind_value_ids\": [" + lFieldValue.Id + "]  }";
                }

                case TrackerFieldType.MultipleChoice:
                {
                    List<string> lValues = pValue as List<string>;
                    List<int> lEntryIds = new List<int>();
                    foreach (string lValue in lValues)
                    {
                        EnumEntry lFieldValue = this.EnumValues.FirstOrDefault(pItem => pItem.Label == lValue);
                        if (lFieldValue != null)
                        {
                            lEntryIds.Add(lFieldValue.Id);
                        }
                    }

                    if (lEntryIds.Any())
                    {
                        return "  {  \"field_id\": " + this.Id + ", \"bind_value_ids\": [" +
                               string.Join(",", lEntryIds) + "]  }";
                    }
                }
                    break;

                case TrackerFieldType.ArtifactLinks:
                {
                    List<ArtifactLink> lValues = pValue as List<ArtifactLink>;

                    List<string> lLinkStr = new List<string>();
                    foreach (ArtifactLink lValue in lValues) lLinkStr.Add("{\"id\" :" + lValue + '}');

                    return "  {  \"field_id\": " + this.Id + ", \"links\": [" + string.Join(",", lLinkStr) + "]  }";
                }

                case TrackerFieldType.Cross:
                {
                    List<ArtifactLink> lValues = pValue as List<ArtifactLink>;

                    List<string> lLinkStr = new List<string>();
                    foreach (ArtifactLink lValue in lValues) lLinkStr.Add("{\"ref\" :" + lValue + '}');

                    return "  {  \"field_id\": " + this.Id + ", \"value\": [" + string.Join(",", lLinkStr) + "]  }";
                }

                default:
                {
                    throw new NotSupportedException("Type not managed when encoding " + this.FieldType);
                }
            }

            return string.Empty;
        }
    }
}