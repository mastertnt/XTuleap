using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace XTuleap.Tests
{
    [TestFixture]
    public class TuleapTests
    {
        private string mUri = "https://tuleap.net/api/";

        private string mKey = "tlp-k1-74.0938e677298d61a90d7a50246dfbce060eaa752b7298de23f5b233569aca766a";

        private int mSimpleTrackerId = 867;

        [Test]
        public void CreateSimpleArtifact()
        {
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);
            
            Artifact lNewArtifact = new Artifact(this.mSimpleTrackerId);
            Dictionary<string, object> lValues = new Dictionary<string, object>
            {
                {"string", "string_value"}, {"int", 77}, {"text", "text value"}, {"float", 0.77}, {"single_choice", "one"},
            };
            lNewArtifact.Create(lConnection, lValues);

            Connection lConnection1 = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure1 = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker1 = new Tracker<Artifact>(lTargetStructure1);
            lTargetTracker1.PreviewRequest(lConnection1);
            Assert.AreEqual(lTargetTracker1.ArtifactIds.Count, lTargetTracker.ArtifactIds.Count + 1);

            Artifact lResult = new Artifact { Id = lNewArtifact.Id };
            lResult.Request(lConnection1, lTargetTracker1);

            Assert.AreEqual(lNewArtifact.Id, lResult.Id);
            Assert.AreEqual(lResult.GetFieldValue<string>("string"), "string_value");
            Assert.AreEqual(lResult.GetFieldValue<int>("int"), 77);
            Assert.AreEqual(lResult.GetFieldValue<float>("float"), 0.77, 0.01);
        }

        public void UpdateString()
        {
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);

            Artifact lArtifactToUpdate = new Artifact(this.mSimpleTrackerId) { Id = lTargetTracker.ArtifactIds.First() };
            lArtifactToUpdate.Update(lConnection, "string", "updated_string_value");


            Connection lConnection1 = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure1 = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker1 = new Tracker<Artifact>(lTargetStructure1);
            lTargetTracker1.PreviewRequest(lConnection1);
            Assert.AreEqual(lTargetTracker1.ArtifactIds.Count, lTargetTracker.ArtifactIds.Count);

            Artifact lResult = new Artifact(this.mSimpleTrackerId) { Id = lArtifactToUpdate.Id };
            lResult.Request(lConnection1, lTargetTracker1);

            Assert.AreEqual(lArtifactToUpdate.Id, lResult.Id);
            Assert.AreEqual(lResult.GetFieldValue<string>("string"), "updated_string_value");
        }

        public void UpdateReference()
        {
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);

            Artifact lArtifactToUpdate = new Artifact(this.mSimpleTrackerId) { Id = lTargetTracker.ArtifactIds.First() };
            lArtifactToUpdate.Update(lConnection, "references", new List<ArtifactLink>() { new ArtifactLink() { Id = lTargetTracker.ArtifactIds.Last() }} );


            Connection lConnection1 = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure1 = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker1 = new Tracker<Artifact>(lTargetStructure1);
            lTargetTracker1.PreviewRequest(lConnection1);
            Assert.AreEqual(lTargetTracker1.ArtifactIds.Count, lTargetTracker.ArtifactIds.Count);

            Artifact lResult = new Artifact(this.mSimpleTrackerId) { Id = lArtifactToUpdate.Id };
            lResult.Request(lConnection1, lTargetTracker1);

            Assert.AreEqual(lArtifactToUpdate.Id, lResult.Id);
            Assert.AreEqual(lResult.GetFieldValue<List<ArtifactLink>>("references").First().Id, lTargetTracker.ArtifactIds.Last());
        }
    }
}
