using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XTuleap
{
    public interface ITracker
    {
        /// <summary>
        ///     Gets or sets the name of the tracker.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        ///     Gets the structure of the tracker.
        /// </summary>
        TrackerStructure Structure
        {
            get;
        }

        /// <summary>
        ///     Gets or sets the item name.
        /// </summary>
        string ItemName
        {
            get;
        }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        ///     Gets the artifacts.
        /// </summary>
        ObservableCollection<int> ArtifactIds
        {
            get;
        }

        /// <summary>
        ///     Gets the list of base artifacts (untyped).
        /// </summary>
        IEnumerable<Artifact> BaseArtifacts
        {
            get;
        }

        /// <summary>
        ///     Requests all artifacts of the tracker.
        /// </summary>
        /// <param name="pConnection">The connection</param>
        void PreviewRequest(Connection pConnection);

        /// <summary>
        ///     Requests all artifacts of the tracker.
        /// </summary>
        /// <param name="pConnection">The connection</param>
        void Request(Connection pConnection);
    }
}
