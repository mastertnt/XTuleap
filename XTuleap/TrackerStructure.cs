using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace XTuleap
{
    /// <summary>
    /// This class represents a tracker structure.
    /// </summary>
    public class TrackerStructure 
    {
        /// <summary>
        /// Gets the identifier of a tracker.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets the list of field of a tracker.
        /// </summary>
        [JsonProperty("fields")]
        public List<TrackerField> Fields { get; }

        /// <summary>
        /// Gets the item name.
        /// </summary>
        [JsonProperty("item_name")]
        public string ItemName { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TrackerStructure()
        {
            this.Fields = new List<TrackerField>();
        }
    }
}
