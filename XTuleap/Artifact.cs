using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace XTuleap
{
    /// <summary>
    ///     This class represents an artifact.
    /// </summary>
    public class Artifact : INotifyPropertyChanged
    {
        /// <summary>
        ///     A constant for INVALID_ARTIFACT.
        /// </summary>
        public static readonly Artifact INVALID_ARTIFACT = new Artifact { Id = -1 };

        /// <summary>
        /// Logger of the class.
        /// </summary>
        private static readonly Logger msLogger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// This constant stores the data time format.
        /// </summary>
        public const string DATE_TIME_FORMAT = "MM/dd/yyyy HH:mm:ss";

        /// <summary>
        ///     This dictionary stores the values non wrapped by child class.
        /// </summary>
        private readonly Dictionary<string?, object?> mDataValues = new Dictionary<string?, object?>();

        public Artifact(int pTrackerId)
        {
            this.TrackerId = pTrackerId;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public Artifact()
        {
            this.TrackerId = -1;
        }

        /// <summary>
        ///     Gets the id of artifact.
        /// </summary>
        [JsonProperty("id")]
        public virtual int Id { get; set; }

        /// <summary>
        ///     Gets the tracker name.
        /// </summary>
        public string? TrackerName { get; private set; }

        /// <summary>
        ///     Gets the tracker id.
        /// </summary>
        public int TrackerId { get; private set; }

        /// <summary>
        ///     Gets the links.
        /// </summary>
        [DisplayName("links")]
        public List<ArtifactLink>? Links { get; set; }

        /// <summary>
        ///     Gets a field value.
        /// </summary>
        /// <param name="pFieldName">The field name.</param>
        /// <returns>The retrieved value.</returns>
        public object GetFieldValue(string? pFieldName)
        {
            if (this.mDataValues.TryGetValue(pFieldName, out object lResult))
            {
                return lResult;
            }

            PropertyInfo[] lPropertyInfos = this.GetType().GetProperties()
                .Where(pProp => pProp.IsDefined(typeof(DisplayNameAttribute), false)).ToArray();
            PropertyInfo lPropertyInfo = lPropertyInfos.FirstOrDefault(pProp =>
                (pProp.GetCustomAttributes(typeof(DisplayNameAttribute), false).First() as DisplayNameAttribute)
                ?.DisplayName == pFieldName);
            if (lPropertyInfo != null)
            {
                return lPropertyInfo.GetValue(this);
            }

            return null;
        }

        /// <summary>
        ///     Gets a field value as string.
        /// </summary>
        /// <param name="pFieldName">The field name.</param>
        /// <returns>The field value as string or "null" if the value is null</returns>
        public string GetFieldValueAsString(string? pFieldName)
        {
            object lValue = this.GetFieldValue(pFieldName);
            if (lValue != null)
            {
                return lValue.ToString();
            }

            return null;
        }

        public TValueType GetFieldValue<TValueType>(string? pFieldName)
        {
            object lFieldValue = this.GetFieldValue(pFieldName);
            return (TValueType)lFieldValue;
        }

        /// <summary>
        ///     Internally stores the value from the JSON.
        /// </summary>
        /// <param name="pFieldName">The field name.</param>
        /// <param name="pValue">The value to set.</param>
        private void StoreValue(string? pFieldName, object? pValue)
        {
            PropertyInfo[] lPropertyInfos = this.GetType().GetProperties()
                .Where(pProp => pProp.IsDefined(typeof(DisplayNameAttribute), false)).ToArray();
            PropertyInfo lPropertyInfo = lPropertyInfos.FirstOrDefault(pProp =>
                (pProp.GetCustomAttributes(typeof(DisplayNameAttribute), false).First() as DisplayNameAttribute)
                ?.DisplayName == pFieldName);
            if (lPropertyInfo != null)
            {
                lPropertyInfo.SetValue(this, pValue);
            }
            else
            {
                this.mDataValues.Add(pFieldName, pValue);
            }
        }

        /// <summary>
        ///     Sets the value into Tuleap.
        /// </summary>
        /// <param name="pConnection">The connection.</param>
        /// <param name="pFieldName">The field name.</param>
        /// <param name="pValue">The value to set.</param>
        public bool Update(Connection pConnection, string pFieldName, object pValue)
        {
            TrackerStructure lStructure =
                pConnection.TrackerStructures.FirstOrDefault(pTracker => pTracker.Id == this.TrackerId);
            if (lStructure != null)
            {
                TrackerField lTrackerField =
                    lStructure.Fields.FirstOrDefault(pField => pField.Name.ToLower() == pFieldName.ToLower());
                if (lTrackerField != null)
                {
                    try
                    {
                        string lUpdateData = "{" +
                                             "  \"values\": [" +
                                             lTrackerField.EncodeValueField(pValue) +
                                             "  ]}";

                        pConnection.PutRequest("artifacts/" + this.Id, lUpdateData);
                    }
                    catch
                    {
                        msLogger.Error("An error occured during update of " + lTrackerField.Name);
                    }
                    
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Deletes the artifact.
        /// </summary>
        /// <param name="pConnection">The connection.</param>
        public bool Delete(Connection pConnection)
        {
            return pConnection.DeleteRequest("artifacts/" + this.Id, "");
        }


        /// <summary>
        ///     Creates an artifact.
        /// </summary>
        /// <param name="pConnection">The connection.</param>
        /// <param name="pValues">The list of values</param>
        public void Create(Connection pConnection, Dictionary<string, object> pValues)
        {
            TrackerStructure lStructure =
                pConnection.TrackerStructures.FirstOrDefault(pTracker => pTracker.Id == this.TrackerId);
            if (lStructure != null)
            {
                string lCreateData = "{\"tracker\": {\"id\" : " + this.TrackerId + "},";
                lCreateData += "\"values\": [";
                foreach (KeyValuePair<string, object> lValue in pValues)
                {
                    TrackerField lTrackerField =
                        lStructure.Fields.FirstOrDefault(pField => pField.Name.ToLower() == lValue.Key.ToLower());
                    if (lTrackerField != null)
                    {
                        lCreateData += lTrackerField.EncodeValueField(lValue.Value);
                    }

                    lCreateData += ",";
                }

                lCreateData = lCreateData.Remove(lCreateData.Length - 1);
                lCreateData += "]}";
                string lResult = pConnection.PostRequest("artifacts", lCreateData);
                JObject lResponse = JObject.Parse(lResult);

                this.Id = Convert.ToInt32(lResponse["id"]);
            }
        }

        /// <summary>
        ///     Requests all artifacts of the tracker.
        /// </summary>
        /// <param name="pConnection">The connection</param>
        /// <param name="pTracker">The host tracker</param>
        public virtual void Request(Connection pConnection, ITracker? pTracker = null)
        {
            string lArtifactContent = pConnection.GetRequest("artifacts/" + this.Id + "?values_format=collection&tracker_structure_format=complete", "");
            TrackerStructure? lTrackerStructure = null;
            if (pTracker != null)
            {
                lTrackerStructure = pTracker.Structure;
            }
            else
            {
                if (string.IsNullOrEmpty(lArtifactContent) == false)
                {
                    JObject lObject = JsonConvert.DeserializeObject(lArtifactContent) as JObject;
                    lTrackerStructure = pConnection.TrackerStructures.FirstOrDefault(pTrackerStructure =>
                        pTrackerStructure.Id == (int)lObject["tracker"]["id"]);
                }
            }

            if (lTrackerStructure != null)
            {
                if (string.IsNullOrEmpty(lArtifactContent) == false)
                {
                    //Console.WriteLine(lArtifactContent);
                    JObject lObject = JsonConvert.DeserializeObject(lArtifactContent) as JObject;
                    this.TrackerName = lObject["tracker"].Value<string>("label");
                    this.TrackerId = lObject["tracker"].Value<int>("id");
                    this.mDataValues.Add("aid", lObject.Value<int>("id"));
                    this.mDataValues.Add("xref", lObject.Value<string>("xref"));
                    foreach (TrackerField lTrackerField in lTrackerStructure.Fields)
                    {
                        JToken lToken = lObject["values"].Children().FirstOrDefault(pToken => pToken["field_id"] != null && pToken["field_id"].ToString() == lTrackerField.Id.ToString());
                        if (lToken == null)
                        {
                            this.StoreValue(lTrackerField.Name, null);
                        }
                        else
                        {
                            switch (lTrackerField.FieldType)
                            {
                                case TrackerFieldType.Aid:
                                    {
                                        // Already managed.
                                    }
                                    break;

                                case TrackerFieldType.SingleChoice:
                                    {
                                        if (lToken["values"] != null && lToken["values"].Count() != 0)
                                        {
                                            if (lToken["values"].First()["id"] != null)
                                            {
                                                try
                                                {
                                                    int lValueId = (int)lToken["values"].First()["id"];
                                                    EnumEntry lValue = lTrackerField.EnumValues.FirstOrDefault(pValue => pValue.Id == lValueId);
                                                    if (lValue != null)
                                                    {
                                                        this.StoreValue(lTrackerField.Name, lValue.Label);
                                                    }
                                                    else
                                                    {
                                                        this.StoreValue(lTrackerField.Name, "null");
                                                    }
                                                }
                                                catch
                                                {
                                                    this.StoreValue(lTrackerField.Name, "null");
                                                }
                                            }
                                            else
                                            {
                                                this.StoreValue(lTrackerField.Name, "null");
                                            }
                                        }
                                        else
                                        {
                                            this.StoreValue(lTrackerField.Name, "null");
                                        }
                                    }
                                    break;

                                case TrackerFieldType.MultipleChoice:

                                    {
                                        if (lToken["values"] != null && lToken["values"].Count() != 0)
                                        {
                                            List<string?> lValues = new List<string?>();
                                            foreach (JToken lValueItem in lToken["values"])
                                            {
                                                if (lValueItem["id"] != null)
                                                {
                                                    try
                                                    {
                                                        int lValueId = (int)lValueItem["id"];
                                                        EnumEntry lValueEnum = lTrackerField.EnumValues.FirstOrDefault(pValue => pValue.Id == lValueId);

                                                        if (lValueEnum != null)
                                                        {
                                                            lValues.Add(lValueEnum.Label);
                                                        }
                                                        else
                                                        {
                                                            lValues.Add("null");
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        lValues.Add("null");
                                                    }
                                                }
                                                else
                                                {
                                                    lValues.Add("null");
                                                }
                                            }

                                            this.StoreValue(lTrackerField.Name, lValues);
                                        }
                                        else
                                        {
                                            this.StoreValue(lTrackerField.Name, "null");
                                        }
                                    }
                                    break;

                                case TrackerFieldType.String:
                                    {
                                        try
                                        {
                                            string? lValue = lToken.Value<string>("value");
                                            if (lValue != null)
                                            {
                                                this.StoreValue(lTrackerField.Name, lValue);
                                            }
                                            else
                                            {
                                                this.StoreValue(lTrackerField.Name, null);
                                            }
                                        }
                                        catch
                                        {
                                            this.StoreValue(lTrackerField.Name, null);
                                        }
                                    }
                                    break;

                                case TrackerFieldType.Text:
                                    {
                                        try
                                        {
                                            string? lValue = lToken.Value<string>("value");
                                            if (lValue != null)
                                            {
                                                var lDocument = new HtmlDocument();
                                                lDocument.LoadHtml(lValue);
                                                this.StoreValue(lTrackerField.Name, lDocument.DocumentNode.InnerText.Trim());
                                            }
                                            else
                                            {
                                                this.StoreValue(lTrackerField.Name, null);
                                            }
                                        }
                                        catch
                                        {
                                            this.StoreValue(lTrackerField.Name, null);
                                        }
                                    }
                                    break;

                                case TrackerFieldType.DateTime:
                                    {
                                        try
                                        {
                                            string lDateFormat = DATE_TIME_FORMAT;
                                            string? lValue = lToken.Value<string>("value");
                                            if (lValue != null)
                                            {
                                                DateTime lDateTime = DateTime.ParseExact(lValue, lDateFormat, CultureInfo.InvariantCulture);
                                                this.StoreValue(lTrackerField.Name, lDateTime);
                                            }
                                            else
                                            {
                                                this.StoreValue(lTrackerField.Name, null);
                                            }
                                        }
                                        catch
                                        {
                                            this.StoreValue(lTrackerField.Name, null);
                                        }
                                    }
                                    break;

                                case TrackerFieldType.Int:
                                    {
                                        try
                                        {
                                            int? lValue = lToken.Value<int>("value");
                                            if (lValue != null)
                                            {
                                                this.StoreValue(lTrackerField.Name, lValue);
                                            }
                                            else
                                            {
                                                this.StoreValue(lTrackerField.Name, null);
                                            }
                                        }
                                        catch
                                        {
                                            this.StoreValue(lTrackerField.Name, null);
                                        }
                                    }
                                    break;

                                case TrackerFieldType.Float:
                                    {
                                        try
                                        {
                                            float? lValue = lToken.Value<int>("value");
                                            if (lValue != null)
                                            {
                                                this.StoreValue(lTrackerField.Name, lValue);
                                            }
                                            else
                                            {
                                                this.StoreValue(lTrackerField.Name, null);
                                            }
                                        }
                                        catch
                                        {
                                            this.StoreValue(lTrackerField.Name, null);
                                        }
                                    }
                                    break;

                                case TrackerFieldType.ArtifactLinks:
                                    {
                                        List<ArtifactLink>? lLinks = new List<ArtifactLink>();
                                        foreach (JToken lSubToken in lToken["links"])
                                        {
                                            ArtifactLink lLink = new ArtifactLink
                                            {
                                                Id = (int)lSubToken["id"],
                                                IsReverse = false
                                            };
                                            lLinks.Add(lLink);
                                        }

                                        foreach (JToken lSubToken in lToken["reverse_links"])
                                        {
                                            ArtifactLink lLink = new ArtifactLink
                                            {
                                                Id = (int)lSubToken["id"],
                                                IsReverse = true
                                            };
                                            lLinks.Add(lLink);
                                        }

                                        this.Links = Links;
                                        this.StoreValue(lTrackerField.Name, lLinks);
                                    }
                                    break;

                                case TrackerFieldType.Cross:

                                    {
                                        List<ArtifactLink>? lLinks = new List<ArtifactLink>();
                                        foreach (JToken lSubToken in lToken["value"])
                                        {
                                            ArtifactLink lLink = new ArtifactLink
                                            {
                                                Reference = lSubToken["ref"].ToString(),
                                                Url = lSubToken["url"].ToString(),
                                                IsReverse = false
                                            };
                                            lLinks.Add(lLink);
                                        }

                                        this.StoreValue(lTrackerField.Name, lLinks);
                                    }
                                    break;

                                case TrackerFieldType.CreatedOn:
                                    {
                                        string lDateFormat = DATE_TIME_FORMAT;
                                        string lValue = lToken.Value<string>("value");
                                        if (lValue != null)
                                        {
                                            DateTime lDateTime = DateTime.ParseExact(lValue, lDateFormat, CultureInfo.InvariantCulture);
                                            this.StoreValue(lTrackerField.Name, lDateTime);
                                        }
                                        else
                                        {
                                            msLogger.Error("Error while parsing 'CreatedOn' field.");
                                            this.StoreValue(lTrackerField.Name, null);
                                        }
                                    }

                                    break;

                                case TrackerFieldType.UpdatedOn:
                                    {
                                        string lDateFormat = DATE_TIME_FORMAT;
                                        string lValue = lToken.Value<string>("value");

                                        if (lValue != null)
                                        {
                                            DateTime lDateTime = DateTime.ParseExact(lValue, lDateFormat, CultureInfo.InvariantCulture);
                                            this.StoreValue(lTrackerField.Name, lDateTime);
                                        }
                                        else
                                        {
                                            this.StoreValue(lTrackerField.Name, null);
                                        }
                                    }
                                    break;

                                case TrackerFieldType.CreatedBy:

                                    {
                                        this.StoreValue(lTrackerField.Name, lToken["value"].Value<string>("username"));
                                    }
                                    break;

                                case TrackerFieldType.UpdatedBy:

                                    {
                                        this.StoreValue(lTrackerField.Name, lToken["value"].Value<string>("username"));
                                    }
                                    break;

                                case TrackerFieldType.StepDefinitions:
                                    {
                                        List<StepDefinition>? lStepDefinitions = new List<StepDefinition>();
                                        foreach (JToken lSubToken in lToken["value"])
                                        {
                                            StepDefinition lStepDefinition = new StepDefinition
                                            {
                                                Id = lSubToken["id"].Value<int>(),
                                                Description = lSubToken["description"].Value<string>(),
                                                ExpectedResults = lSubToken["expected_results"].Value<string>(),
                                                Rank = lSubToken["rank"].Value<int>()
                                            };
                                            lStepDefinitions.Add(lStepDefinition);
                                        }

                                        this.StoreValue(lTrackerField.Name, lStepDefinitions);
                                    } 
                                    break;

                                case TrackerFieldType.Unknown:

                                    {
                                        //Console.WriteLine("Type of field " + lTrackerField.Name + " non managed : " + lTrackerField.Type);
                                    }

                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Overrides ToString
        /// </summary>
        public override string ToString()
        {
            StringBuilder lBuilder = new StringBuilder();
            lBuilder.AppendLine("[aid] = " + this.Id);
            foreach (KeyValuePair<string?, object?> lDataValue in this.mDataValues)
            {
                if (lDataValue.Key != "aid")
                {
                    if (lDataValue.Value is IList)
                    {
                        IList lList = lDataValue.Value as IList;
                        string lValueStr = "";
                        foreach (object lItem in lList)
                        {
                            lValueStr += lItem.ToString();
                            lValueStr += "; ";
                        }
                        lBuilder.AppendLine("[" + lDataValue.Key + "] / " + lDataValue.Value.GetType() + " = " + lValueStr);
                    }
                    else
                    {
                        lBuilder.AppendLine("[" + lDataValue.Key + "] = " + lDataValue.Value);
                    }
                }
            }

            return lBuilder.ToString();
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}