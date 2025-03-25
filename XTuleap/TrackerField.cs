﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using NLog;

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
        CreatedOn,
        UpdatedOn,
        CreatedBy,
        UpdatedBy,
        StepDefinitions,
        File,
        Unknown
    }

    /// <summary>
    ///     This class represents a tracker field.
    /// </summary>
    public class TrackerField : ITrackerField
    {
        private static readonly Dictionary<string?, TrackerFieldType> msTypes = new Dictionary<string?, TrackerFieldType>();
        
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
            msTypes.Add("subon", TrackerFieldType.CreatedOn);
            msTypes.Add("subby", TrackerFieldType.CreatedBy);
            msTypes.Add("lud", TrackerFieldType.UpdatedOn);
            msTypes.Add("luby", TrackerFieldType.UpdatedBy);
            msTypes.Add("ttmstepdef", TrackerFieldType.StepDefinitions);
            msTypes.Add("file", TrackerFieldType.File);
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public TrackerField()
        {
            this.EnumValues = new List<EnumEntry>();
        }

        [JsonProperty("field_id")]
        public virtual int Id
        {
            get;
            set;
        }

        [JsonProperty("label")]
        public virtual string? Label
        {
            get;
            set;
        }

        [JsonProperty("name")]
        public virtual string? Name
        {
            get;
            set;
        }

        [JsonProperty("type")]
        public virtual string? Type
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

        public virtual List<EnumEntry> EnumValues
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
                    return "  {  \"field_id\": " + this.Id + ", \"value\": \"" + lValue.ToString("yyyy-MM-ddTHH:mm:ss") + "\"  }";
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
                    foreach (ArtifactLink lValue in lValues)
                    {
                        lLinkStr.Add("{\"id\" :" + lValue + '}');
                    }

                    return "  {  \"field_id\": " + this.Id + ", \"links\": [" + string.Join(",", lLinkStr) + "]  }";
                }

                case TrackerFieldType.Cross:
                {
                    List<ArtifactLink> lValues = pValue as List<ArtifactLink>;

                    List<string> lLinkStr = new List<string>();
                    foreach (ArtifactLink lValue in lValues)
                    {
                        lLinkStr.Add("{\"ref\" :" + lValue + '}');
                    }

                    return "  {  \"field_id\": " + this.Id + ", \"value\": [" + string.Join(",", lLinkStr) + "]  }";
                }

                case TrackerFieldType.StepDefinitions:
                {
                    List<StepDefinition> lValues = pValue as List<StepDefinition>;

                    List<string> lLinkStr = new List<string>();
                    foreach (StepDefinition lValue in lValues)
                    {
                        lLinkStr.Add("{\"id\" :" + lValue.Id + ", \"description\" :\"" + lValue.Description + "\", \"description_format\": \"text\", \"expected_results_format\": \"text\", \"expected_results\" : \"" + lValue.ExpectedResults + "\", \"rank\" :" + lValue.Rank + '}');
                    }

                    return "  {  \"field_id\": " + this.Id + ", \"type\": \"ttmstepdef\", \"value\": [" + string.Join(",", lLinkStr) + "]}";
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