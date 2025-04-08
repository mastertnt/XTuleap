using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using XTuleap.Extensions;

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
        MultiCheckbox,
        Unknown
    }

    /// <summary>
    ///     This class represents a tracker field.
    /// </summary>
    public class TrackerField : ITrackerField
    {
        private static readonly Dictionary<string?, TrackerFieldType> msTypes = new Dictionary<string?, TrackerFieldType>();

        /// <summary>
        /// Logger of the class.
        /// </summary>
        private static readonly Logger msLogger = LogManager.GetCurrentClassLogger();

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
            msTypes.Add("cb", TrackerFieldType.MultiCheckbox);
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
        public string EncodeValueField(object? pValue)
        {
            switch (this.FieldType)
            {
                case TrackerFieldType.Int:
                    {
                        int lValue = (int)pValue;
                        return "  {  \"field_id\": " + this.Id + ", \"value\": " +
                               lValue.ToString(CultureInfo.InvariantCulture) + "  }";
                    }

                case TrackerFieldType.Float:
                    {
                        double lValue = (double)pValue;
                        return "  {  \"field_id\": " + this.Id + ", \"value\": " +
                               lValue.ToString(CultureInfo.InvariantCulture) + "  }";
                    }

                case TrackerFieldType.DateTime:
                    {
                        DateTime lValue = (DateTime)pValue;
                        return "  {  \"field_id\": " + this.Id + ", \"value\": \"" + lValue.ToString("yyyy-MM-ddTHH:mm:ss") + "\"  }";
                    }

                case TrackerFieldType.String:
                case TrackerFieldType.Text:
                    {
                        return "  {  \"field_id\": " + this.Id + ", \"value\": \"" + pValue + "\"  }";
                    }

                case TrackerFieldType.Radio:
                    {
                        EnumEntry lFieldValue = this.EnumValues.FirstOrDefault(pItem => pItem.Label == pValue.ToString());
                        if (lFieldValue == null)
                        {
                            return string.Empty;
                        }
                        return "  {  \"field_id\": " + this.Id + ", \"bind_value_ids\": [" + lFieldValue.Id + "]  }";
                    }
                    break;
                case TrackerFieldType.SingleChoice:
                    {
                        EnumEntry lFieldValue = this.EnumValues.FirstOrDefault(pItem => pItem.Label == pValue.ToString());
                        if (lFieldValue == null)
                        {
                            return string.Empty;
                        }
                        return "  {  \"field_id\": " + this.Id + ", \"bind_value_ids\": [" + lFieldValue.Id + "]  }";
                    }

                case TrackerFieldType.MultiCheckbox:
                case TrackerFieldType.MultipleChoice:
                    {
                        List<string> lValues = pValue as List<string>;
                        List<int> lEntryIds = new List<int>();
                        if (lValues != null)
                        {
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

                        return "";

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
                        List<StepDefinition> lStepDefinitions = pValue as List<StepDefinition>;
                        if (lStepDefinitions != null)
                        {
                            List<string> lLinkStr = new List<string>();
                            foreach (StepDefinition lStepDefinition in lStepDefinitions)
                            {
                                string lDescriptionHtml = lStepDefinition.Description.IsHtml() ? "html" : "text";
                                string lResultsHtml = lStepDefinition.Description.IsHtml() ? "html" : "text";
                                var lStepData = new { id = lStepDefinition.Id, description = lStepDefinition.Description, description_format = lDescriptionHtml, rank = lStepDefinition.Rank, expected_results = lStepDefinition.ExpectedResults, expected_results_format = lResultsHtml };
                                lLinkStr.Add(JsonConvert.SerializeObject(lStepData));
                            }

                            return "  {  \"field_id\": " + this.Id + ", \"type\": \"ttmstepdef\", \"value\": [" + string.Join(",", lLinkStr) + "]}";
                        }

                        return "";
                    }

                case TrackerFieldType.CreatedBy:
                case TrackerFieldType.CreatedOn:
                case TrackerFieldType.UpdatedBy:
                case TrackerFieldType.UpdatedOn:
                case TrackerFieldType.Unknown:
                case TrackerFieldType.File:
                    {
                        // Ignored
                    }
                    break;


                default:
                    {
                        msLogger.Error("Type not managed when encoding " + this.FieldType);
                    }
                    break;
            }

            return string.Empty;
        }
    }
}