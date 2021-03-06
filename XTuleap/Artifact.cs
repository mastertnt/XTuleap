using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XTuleap
{
    /// <summary>
    /// This class represents an artifact.
    /// </summary>
    public class Artifact
    {
        /// <summary>
        /// A constant for INVALID_ARTIFACT.
        /// </summary>
        public static readonly Artifact INVALID_ARTIFACT = new Artifact() { Id = -1 };

        /// <summary>
        /// This dictionary stores the values non wrapped by child class.
        /// </summary>
        private readonly Dictionary<string, object> mDataValues = new Dictionary<string, object>();

        /// <summary>
        /// Gets the id of artifact.
        /// </summary>
        [JsonProperty("id")]
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the tracker name.
        /// </summary>
        public string TrackerName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the tracker id.
        /// </summary>
        public int TrackerId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the links.
        /// </summary>
        [DisplayName("links")]
        public List<ArtifactLink> Links
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a field value.
        /// </summary>
        /// <param name="pFieldName">The field name.</param>
        /// <returns>The retrieved value.</returns>
        public object GetFieldValue(string pFieldName)
        {
            object lResult;
            if (this.mDataValues.TryGetValue(pFieldName, out lResult))
            {
                return lResult;
            }
            PropertyInfo[] lPropertyInfos = this.GetType().GetProperties().Where(pProp => pProp.IsDefined(typeof(DisplayNameAttribute), false)).ToArray();
            PropertyInfo lPropertyInfo = lPropertyInfos.FirstOrDefault(pProp => (pProp.GetCustomAttributes(typeof(DisplayNameAttribute), false).First() as DisplayNameAttribute)?.DisplayName == pFieldName);
            if (lPropertyInfo != null)
            {
                return lPropertyInfo.GetValue(this);
            }

            return null;
        }

        /// <summary>
        /// Gets a field value as string.
        /// </summary>
        /// <param name="pFieldName">The field name.</param>
        /// <returns>The field value as string or "null" if the value is null</returns>

        public string GetFieldValueAsString(string pFieldName)
        {
            object lValue = this.GetFieldValue(pFieldName);
            if (lValue != null)
            {
                return lValue.ToString();
            }

            return null;
        }

        public TValueType GetFieldValue<TValueType>(string pFieldName)
        {
            object lFieldValue = this.GetFieldValue(pFieldName);
            return (TValueType)lFieldValue;
        }

        /// <summary>
        /// Internally stores the value from the JSON.
        /// </summary>
        /// <param name="pFieldName">The field name.</param>
        /// <param name="pValue">The value to set.</param>
        private void StoreValue(string pFieldName, object pValue)
        {
            PropertyInfo[] lPropertyInfos = this.GetType().GetProperties().Where(pProp => pProp.IsDefined(typeof(DisplayNameAttribute), false)).ToArray();
            PropertyInfo lPropertyInfo = lPropertyInfos.FirstOrDefault(pProp => (pProp.GetCustomAttributes(typeof(DisplayNameAttribute), false).First() as DisplayNameAttribute)?.DisplayName == pFieldName);
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
        ///  Sets the value into Tuleap.
        /// </summary>
        /// <param name="pConnection">The connection.</param>
        /// <param name="pFieldName">The field name.</param>
        /// <param name="pValue">The value to set.</param>
        public bool CommitValue(Connection pConnection, string pFieldName, object pValue)
        {
            TrackerStructure lStructure = pConnection.TrackerStructures.FirstOrDefault(pTracker => pTracker.Id == this.TrackerId);
            if (lStructure != null)
            {
                TrackerField lTrackerField = lStructure.Fields.FirstOrDefault(pField => pField.Name.ToLower() == pFieldName.ToLower());
                if (lTrackerField != null)
                {
                    string lUpdateData = "{" +
                                         "  \"values\": [" +
                                         lTrackerField.EncodeValueField(pValue) +
                                         "  ]}";


                    pConnection.PutRequest("artifacts/" + this.Id, lUpdateData);

                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes the artifact.
        /// </summary>
        /// <param name="pConnection">The connection.</param>
        public void Delete(Connection pConnection)
        {
            pConnection.DeleteRequest("artifacts/" + this.Id, "");
        }


        /// <summary>
        /// Creates an artifact.
        /// </summary>
        /// <param name="pConnection">The connection.</param>
        /// <param name="pStructure">The tracker structure</param>
        /// <param name="pValues">The list of values</param>
        public void Create(Connection pConnection, TrackerStructure pStructure, Dictionary<string, object> pValues)
        {
            string lCreateData = "{\"tracker\": {\"id\" : " + pStructure.Id + "},";
            lCreateData += "\"values\": [";
            foreach (var lValue in pValues)
            {
                TrackerField lTrackerField = pStructure.Fields.FirstOrDefault(pField => pField.Name.ToLower() == lValue.Key.ToLower());
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

        /// <summary>
        /// Requests all artifacts of the tracker.
        /// </summary>
        /// <param name="pConnection">The connection</plCreateDataaram>
        /// <param name="pTracker">The host tracker</param>
        public void Request(Connection pConnection, ITracker pTracker = null)
        {
            string lArtifactContent = pConnection.GetRequest("artifacts/" + this.Id + "?values_format=collection&tracker_structure_format=complete", "");
            //File.WriteAllText(@"d:\temp\artifact_" + this.Id + ".json", lArtifactContent);
            TrackerStructure lTrackerStructure = null;
            if (pTracker != null)
            {
                lTrackerStructure = pTracker.Structure;
            }
            else
            {
                if (string.IsNullOrEmpty(lArtifactContent) == false)
                {
                    JObject lObject = JsonConvert.DeserializeObject(lArtifactContent) as JObject;
                    lTrackerStructure = pConnection.TrackerStructures.FirstOrDefault(pTrackerStructure => pTrackerStructure.Id == (int)lObject["tracker"]["id"]);
                }
            }

            if (lTrackerStructure != null)
            {
                if (string.IsNullOrEmpty(lArtifactContent) == false)
                {
                    JObject lObject = JsonConvert.DeserializeObject(lArtifactContent) as JObject;
                    this.TrackerName = lObject["tracker"].Value<string>("label");
                    this.TrackerId = lObject["tracker"].Value<int>("id");
                    this.mDataValues.Add("aid", lObject.Value<int>("id"));
                    this.mDataValues.Add("xref", lObject.Value<string>("xref"));
                    foreach (var lTrackerField in lTrackerStructure.Fields)
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
                                            List<string> lValues = new List<string>();
                                            foreach (var lValueItem in lToken["values"])
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
                                            this.StoreValue(lTrackerField.Name, lToken.Value<string>("value"));
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
                                            this.StoreValue(lTrackerField.Name, lToken.Value<string>("value"));
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
                                            string lDateFormat = "MM/dd/yyyy hh:mm:ss";
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
                                        catch (Exception e)
                                        {
                                            this.StoreValue(lTrackerField.Name, null);
                                        }
                                    }
                                    break;

                                case TrackerFieldType.Int:
                                    {
                                        try
                                        {
                                            this.StoreValue(lTrackerField.Name, lToken.Value<int>("value"));
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
                                            this.StoreValue(lTrackerField.Name, lToken.Value<float>("value"));
                                        }
                                        catch (Exception e)
                                        {
                                            this.StoreValue(lTrackerField.Name, null);
                                        }
                                    }
                                    break;

                                case TrackerFieldType.ArtifactLinks:
                                    {
                                        List<ArtifactLink> lLinks = new List<ArtifactLink>();
                                        foreach (var lSubToken in lToken["links"])
                                        {
                                            ArtifactLink lLink = new ArtifactLink
                                            {
                                                Id = (int)lSubToken["id"]
                                            };
                                            lLinks.Add(lLink);
                                        }
                                        this.StoreValue(lTrackerField.Name, lLinks);
                                    }
                                    break;

                                case TrackerFieldType.Cross:
                                    {
                                        List<ArtifactLink> lLinks = new List<ArtifactLink>();
                                        foreach (var lSubToken in lToken["value"])
                                        {
                                            ArtifactLink lLink = new ArtifactLink
                                            {
                                                Reference = lSubToken["ref"].ToString(),
                                                Url = lSubToken["url"].ToString()
                                            };
                                            lLinks.Add(lLink);
                                        }
                                        this.StoreValue(lTrackerField.Name, lLinks);
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
        /// Overrides ToString
        /// </summary>
        public override string ToString()
        {
            StringBuilder lBuilder = new StringBuilder();
            lBuilder.AppendLine("[aid] = " + this.Id);
            foreach (var lDataValue in this.mDataValues)
            {
                if (lDataValue.Key != "aid")
                {
                    if (lDataValue.Value is IEnumerable)
                    {
                        lBuilder.AppendLine("[" + lDataValue.Key + "] = " + string.Join(";", lDataValue.Value));
                    }
                    else
                    {
                        lBuilder.AppendLine("[" + lDataValue.Key + "] = " + lDataValue.Value);
                    }

                }
            }
            return lBuilder.ToString();
        }
    }
}
