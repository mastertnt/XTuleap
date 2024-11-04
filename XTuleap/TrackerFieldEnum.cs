using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using NLog;

namespace XTuleap
{
    /// <summary>
    ///     This class represents a tracker field.
    /// </summary>
    public class TrackerFieldEnum : TrackerField
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public TrackerFieldEnum()
        {
            this.EnumValues = new List<EnumEntry>();
        }

        [JsonProperty("values")]
        public override List<EnumEntry> EnumValues
        {
            get;
            set;
        }        
    }
}