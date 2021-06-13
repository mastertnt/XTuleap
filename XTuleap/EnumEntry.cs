using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
