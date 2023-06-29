using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XTuleap
{
    public interface ITracker
    {
        /// <summary>
        /// Gets or sets the name of the tracker.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the structure of the tracker.
        /// </summary>
        TrackerStructure Structure { get; }

        /// <summary>
        /// Gets or sets the item name.
        /// </summary>
        string ItemName
        {
            get;
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        /// Gets the artifacts.
        /// </summary>
        List<int> ArtifactIds { get; }

        /// <summary>
        /// Gets the list of base artifacts (untyped).
        /// </summary>
        IEnumerable<Artifact> BaseArtifacts { get; }

        /// <summary>
        /// Requests all artifacts of the tracker.
        /// </summary>
        /// <param name="pConnection">The connection</param>
        void PreviewRequest(Connection pConnection);

        /// <summary>
        /// Requests all artifacts of the tracker.
        /// </summary>
        /// <param name="pConnection">The connection</param>
        void Request(Connection pConnection);
    }

    /// <summary>
    /// This class represents a tracker in TULEAP.
    /// </summary>
    public class Tracker<TArtifactType> : ITracker where TArtifactType : Artifact, new()
    {
        /// <summary>
        /// Gets or sets the name of the tracker.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the item name.
        /// </summary>
        public string ItemName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the structure of the tracker.
        /// </summary>
        public TrackerStructure Structure { get; }

        /// <summary>
        /// Gets the artifacts.
        /// </summary>
        public List<TArtifactType> Artifacts { get; private set; }

        /// <summary>
        /// Gets the enumeration of base artifacts (untyped).
        /// </summary>
        public IEnumerable<Artifact> BaseArtifacts
        {
            get
            {
                return this.Artifacts;
            }
        }

        /// <summary>
        /// Gets the artifacts.
        /// </summary>
        public List<int> ArtifactIds { get; private set; }

        /// <summary>
        /// Event raised when an artifact has been retrieved.
        /// </summary>
        public event Action<Artifact> ArtifactRetrieved;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="pStructure">The structure of the tracker.</param>
        public Tracker(TrackerStructure pStructure)
        {
            this.Structure = pStructure;
            this.Artifacts = new List<TArtifactType>();
            this.Name = "Tracker " + this.Structure.Id;
        }

        /// <summary>
        /// Requests all artifacts of the tracker.
        /// </summary>
        /// <param name="pConnection">The connection</param>
        public void PreviewRequest(Connection pConnection)
        {
            this.Artifacts.Clear();
            string lTrackerInfo = pConnection.GetRequest("trackers/" + this.Structure.Id, "");
            if (string.IsNullOrEmpty(lTrackerInfo) == false)
            {
                JObject lTrackerObject = JsonConvert.DeserializeObject(lTrackerInfo) as JObject;
                if (lTrackerObject != null)
                {
                    this.Name = lTrackerObject.Value<string>("label");
                    this.Description = lTrackerObject.Value<string>("description");
                    this.ItemName = lTrackerObject.Value<string>("item_name");
                }
            }

            string lIds = pConnection.GetRequest("trackers/" + this.Structure.Id + "/artifacts?limit=1000", "");
            if (string.IsNullOrEmpty(lIds) == false)
            {
                List<TArtifactType> lResult = JsonConvert.DeserializeObject<List<TArtifactType>>(lIds);
                this.ArtifactIds = lResult.Select(pItem => pItem.Id).ToList();
            }
        }

        /// <summary>
        /// Requests all artifacts of the tracker.
        /// </summary>
        /// <param name="pConnection">The connection</param>
        public void Request(Connection pConnection)
        {
            foreach (var lId in this.ArtifactIds)
            {
                TArtifactType lResult = new TArtifactType {Id = lId};
                lResult.Request(pConnection, this);
                this.Artifacts.Add(lResult);
                if (ArtifactRetrieved != null)
                {
                    this.ArtifactRetrieved(lResult);
                }
            }
        }

        /// <summary>
        /// Delete al artifacts from a tracker.
        /// </summary>
        /// <param name="pConnection">The connection.</param>
        public void DeleteAllArtifacts(Connection pConnection)
        {
            this.PreviewRequest(pConnection);
            foreach (var lArtifactId in this.ArtifactIds)
            {
                Artifact lArtifact = new Artifact(this.Structure.Id) { Id = lArtifactId};
                lArtifact.Delete(pConnection);
            }
            this.PreviewRequest(pConnection);
        }
    }
}
