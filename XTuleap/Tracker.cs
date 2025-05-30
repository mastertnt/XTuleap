﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace XTuleap
{
    /// <summary>
    ///     This class represents a tracker in Tuleap.
    /// </summary>
    public class Tracker<TArtifactType> : INotifyPropertyChanged, ITracker where TArtifactType : Artifact, new()
    {
        /// <summary>
        /// Logger of the class.
        /// </summary>
        private static readonly Logger msLogger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="pStructure">The structure of the tracker.</param>
        public Tracker(TrackerStructure pStructure)
        {
            this.Structure = pStructure;
            this.Artifacts = new ObservableCollection<TArtifactType>();
            this.Name = "Tracker " + this.Structure.Id;
        }

        /// <summary>
        ///     Gets the artifacts.
        /// </summary>
        public ObservableCollection<TArtifactType> Artifacts
        {
            get;
        }

        /// <summary>
        ///     Gets or sets the name of the tracker.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        ///     Gets or sets the item name.
        /// </summary>
        public string ItemName
        {
            get;
            private set;
        }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        ///     Gets the structure of the tracker.
        /// </summary>
        public TrackerStructure Structure
        {
            get;
        }

        /// <summary>
        ///     Gets the enumeration of base artifacts (untyped).
        /// </summary>
        public IEnumerable<Artifact> BaseArtifacts
        {
            get { return this.Artifacts; }
        }

        /// <summary>
        ///     Gets the artifacts.
        /// </summary>
        public ObservableCollection<int> ArtifactIds
        {
            get;
            private set;
        }

        /// <summary>
        ///     Requests all artifacts of the tracker.
        /// </summary>
        /// <param name="pConnection">The connection</param>
        public void PreviewRequest(Connection pConnection)
        {
            this.Artifacts.Clear();
            this.ArtifactIds = new ObservableCollection<int>();

            try
            {
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
            }
            catch (Exception e)
            {
                msLogger.Error("Error while retrieving tracker info", e);
                throw;
            }

            try
            {
                bool lContinue = true;
                int lOffset = 0;
                while (lContinue)
                {
                    var lIds = pConnection.GetRequest("trackers/" + this.Structure.Id + "/artifacts?limit=100&offset="+ lOffset, "");
                    if (string.IsNullOrEmpty(lIds) == false)
                    {
                        msLogger.Debug("Content to deserialize : " + lIds);
                        List<TArtifactType> lResult = JsonConvert.DeserializeObject<List<TArtifactType>>(lIds);
                        if (lResult == null || lResult.Count < 100)
                        {
                            lContinue = false;
                        }
                        else
                        {
                            lOffset += 100;
                        }

                        foreach (var lId in lResult.Select(pItem => pItem.Id))
                        {
                            this.ArtifactIds.Add(lId);
                        }
                    }
                    else
                    {
                        lContinue = false;
                    }
                }
                msLogger.Debug("Read artifacts count " + this.ArtifactIds.Count);
                
            }
            catch (Exception e)
            {
                msLogger.Error("Error while retrieving tracker artifacts", e);
                throw;
            }
           
        }

        /// <summary>
        ///     Requests all artifacts of the tracker.
        /// </summary>
        /// <param name="pConnection">The connection</param>
        public void Request(Connection pConnection)
        {
            foreach (int lId in this.ArtifactIds)
            {
                TArtifactType lResult = new TArtifactType {Id = lId};
                lResult.Request(pConnection, this);
                this.Artifacts.Add(lResult);
                if (this.ArtifactRetrieved != null)
                {
                    this.ArtifactRetrieved(lResult);
                }
            }
        }

        /// <summary>
        ///     Event raised when an artifact has been retrieved.
        /// </summary>
        public event Action<Artifact> ArtifactRetrieved;

        /// <summary>
        ///     Delete al artifacts from a tracker.
        /// </summary>
        /// <param name="pConnection">The connection.</param>
        public void DeleteAllArtifacts(Connection pConnection)
        {
            this.PreviewRequest(pConnection);
            foreach (int lArtifactId in this.ArtifactIds)
            {
                Artifact lArtifact = new Artifact(this.Structure.Id) {Id = lArtifactId};
                lArtifact.Delete(pConnection);
            }

            this.PreviewRequest(pConnection);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}