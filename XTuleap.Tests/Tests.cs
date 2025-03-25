using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace XTuleap.Tests
{
    public class TuleapTests
    {
        enum Test
        {
            one,
            two,
            three,
        }
        private readonly string mKey = "tlp-k1-37.ddd";
        private readonly int mSimpleTrackerId = 1041;
        private readonly string mUri = "https://xxx.xxx.com/api/";

        public readonly string mContent = "Id: 33980 ProductId :33653 UpdateTag: 0 HardwareId: 556013433 Description: Created on 20/08/2023 ClientId: 33550 CreationDate: 20/08/2023 16:37:19 CreationLogin: nby77 UpdateDate: 20/08/2023 19:04:06 UpdateLogin: nby77 SupportYear: 0 LoanEndDate: 01/01/0001 00:00:00 Type: Pro Mode: NodeLocked Feature: 33555 Feature: 33556 Feature: 33561";

        [Fact]
        public void CreateFrom()
        {
            string data = "{ \"tracker\": { \"id\" : 1041}, \"values\": [  { \"field_id\": 24687, \"value\": \"Summary\"  },  { \"field_id\": 24699, \"bind_value_ids\": [7997]  },  { \"field_id\": 24689, \"type\": \"ttmstepdef\", \"value\": [{ \"id\" :1, \"description\" : \"Step1\", \"description_format\": \"text\", \"expected_results_format\": \"text\", \"expected_results\" : \"Expected1\", \"rank\" :1}]  },  { \"field_id\": 24693, \"value\": \"123\"  }]}";
            Connection lConnection = new Connection(this.mUri, this.mKey);
            string lResult = lConnection.PostRequest("artifacts", data);
            JObject lResponse = JObject.Parse(lResult);
        }

        [Fact]
        public void Request()
        {
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            //Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            //lTargetTracker.PreviewRequest(lConnection);

            Artifact lNewArtifact = new Artifact()
            {
                Id = 4843
            };
            lNewArtifact.Request(lConnection);
        }

        [Fact]
        public void CreateSimpleArtifact()
        {
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);

            Artifact lNewArtifact = new Artifact(this.mSimpleTrackerId);
            Dictionary<string, object> lValues = new Dictionary<string, object>
            {
                {"string", "string_value"}, {"int", 77}, {"text", "text value"}, {"float", 0.77},
                {"single_choice", "one"}
            };
            lNewArtifact.Create(lConnection, lValues);

            Connection lConnection1 = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure1 = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker1 = new Tracker<Artifact>(lTargetStructure1);
            lTargetTracker1.PreviewRequest(lConnection1);
            Assert.Equal(lTargetTracker1.ArtifactIds.Count, lTargetTracker.ArtifactIds.Count + 1);

            Artifact lResult = new Artifact {Id = lNewArtifact.Id};
            lResult.Request(lConnection1, lTargetTracker1);

            Assert.Equal(lNewArtifact.Id, lResult.Id);
            Assert.Equal(lResult.GetFieldValue<string>("string"), "string_value");
            Assert.Equal(lResult.GetFieldValue<int>("int"), 77);
            Assert.Equal(lResult.GetFieldValue<float>("float"), 0.77, 0.01);
        }

        [Fact]
        public void DeleteArtifact()
        {
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);

            Artifact lNewArtifact = new Artifact(this.mSimpleTrackerId);
            Dictionary<string, object> lValues = new Dictionary<string, object>
            {
                {"string", "string_value"}, {"int", 77}, {"text", "text value"}, {"float", 0.77},
                {"single_choice", "one"}
            };
            lNewArtifact.Create(lConnection, lValues);
            lNewArtifact.Delete(lConnection);
        }

        [Fact]
        public void CreateArtifactWithEnum()
        {
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);
            Artifact lNewArtifact = new Artifact(this.mSimpleTrackerId);
            Dictionary<string, object> lValues = new Dictionary<string, object>
            {
                {"single_choice", Test.one}
            };
            lNewArtifact.Create(lConnection, lValues);

            Connection lConnection1 = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure1 = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker1 = new Tracker<Artifact>(lTargetStructure1);
            lTargetTracker1.PreviewRequest(lConnection1);
            Assert.Equal(lTargetTracker1.ArtifactIds.Count, lTargetTracker.ArtifactIds.Count + 1);

            Artifact lResult = new Artifact { Id = lNewArtifact.Id };
            lResult.Request(lConnection1, lTargetTracker1);

            Assert.Equal(lNewArtifact.Id, lResult.Id);
            Assert.Equal(lResult.GetFieldValue<string>("single_choice"), "one");
        }

        [Fact]
        public void UpdateString()
        {
            string lNewValue = "Created on 02/11/2023";
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);

            Artifact lArtifactToUpdate = new Artifact(this.mSimpleTrackerId) {Id = lTargetTracker.ArtifactIds.First()};
            lArtifactToUpdate.Update(lConnection, "string", "Created on 02/11/2023");


            Connection lConnection1 = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure1 = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker1 = new Tracker<Artifact>(lTargetStructure1);
            lTargetTracker1.PreviewRequest(lConnection1);
            Assert.Equal(lTargetTracker1.ArtifactIds.Count, lTargetTracker.ArtifactIds.Count);

            Artifact lResult = new Artifact(this.mSimpleTrackerId) {Id = lArtifactToUpdate.Id};
            lResult.Request(lConnection1, lTargetTracker1);

            Assert.Equal(lArtifactToUpdate.Id, lResult.Id);
            Assert.Equal(lNewValue, lResult.GetFieldValue<string>("string"));
        }

        [Fact]
        public void UpdateText()
        {
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(863);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);

            Artifact lArtifactToUpdate = new Artifact(863) { Id = 33980};
            lArtifactToUpdate.Update(lConnection, "dongle_content", this.mContent);


            Connection lConnection1 = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure1 = lConnection.AddTrackerStructure(863);
            Tracker<Artifact> lTargetTracker1 = new Tracker<Artifact>(lTargetStructure1);
            lTargetTracker1.PreviewRequest(lConnection1);
            Assert.Equal(lTargetTracker1.ArtifactIds.Count, lTargetTracker.ArtifactIds.Count);

            Artifact lResult = new Artifact(863) { Id = 33980 };
            lResult.Request(lConnection1, lTargetTracker1);

            Assert.Equal(lArtifactToUpdate.Id, lResult.Id);
            Assert.Equal(lResult.GetFieldValue<string>("dongle_content"), this.mContent);
        }

        [Fact]
        public void UpdateDate()
        {
            int lTrackerId = 863;
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(lTrackerId); // Dongle tracker.
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);

            Artifact lArtifactToUpdate = new Artifact(lTrackerId) { Id = 35086 };
            lArtifactToUpdate.Update(lConnection, "loan_end_date", new DateTime(1901, 12, 14, 00, 00, 00));


            //Connection lConnection1 = new Connection(this.mUri, this.mKey);
            //TrackerStructure lTargetStructure1 = lConnection.AddTrackerStructure(863);
            //Tracker<Artifact> lTargetTracker1 = new Tracker<Artifact>(lTargetStructure1);
            //lTargetTracker1.PreviewRequest(lConnection1);
            //Assert.Equal(lTargetTracker1.ArtifactIds.Count, lTargetTracker.ArtifactIds.Count);

            //Artifact lResult = new Artifact(863) { Id = 33980 };
            //lResult.Request(lConnection1, lTargetTracker1);

            //Assert.Equal(lArtifactToUpdate.Id, lResult.Id);
            //Assert.Equal(lResult.GetFieldValue<string>("dongle_content"), this.mContent);
        }

        [Fact]
        public void UpdateStatus()
        {
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(897);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);
        }

        [Fact]
        public void UpdateReference()
        {
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);

            Artifact lArtifactToUpdate = new Artifact(this.mSimpleTrackerId) {Id = lTargetTracker.ArtifactIds.First()};
            lArtifactToUpdate.Update(lConnection, "references",
                new List<ArtifactLink> {new ArtifactLink {Id = lTargetTracker.ArtifactIds.Last()}});


            Connection lConnection1 = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure1 = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker1 = new Tracker<Artifact>(lTargetStructure1);
            lTargetTracker1.PreviewRequest(lConnection1);
            Assert.Equal(lTargetTracker1.ArtifactIds.Count, lTargetTracker.ArtifactIds.Count);

            Artifact lResult = new Artifact(this.mSimpleTrackerId) {Id = lArtifactToUpdate.Id};
            lResult.Request(lConnection1, lTargetTracker1);

            Assert.Equal(lArtifactToUpdate.Id, lResult.Id);
            Assert.Equal(lResult.GetFieldValue<List<ArtifactLink>>("references").First().Id,
                lTargetTracker.ArtifactIds.Last());
        }

        private static int RandomId(ObservableCollection<int> pArtifactIds)
        {
            Random lRandom = new Random();
            return pArtifactIds.ElementAt(lRandom.Next(0, pArtifactIds.Count));
        }

        [Fact]
        public void Display()
        {
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(813);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);
            lTargetTracker.Request(lConnection);
            foreach (var lTrackerArtifact in lTargetTracker.Artifacts)
            {
                Console.WriteLine(lTrackerArtifact);
            }
        }

        [Fact]
        public void UpdateReferences()
        {
            Connection lConnection = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);

            int lTargetId = RandomId(lTargetTracker.ArtifactIds);
            int lFirst = RandomId(lTargetTracker.ArtifactIds);
            int lSecond = RandomId(lTargetTracker.ArtifactIds);

            Artifact lArtifactToUpdate = new Artifact(this.mSimpleTrackerId) {Id = lTargetId};
            lArtifactToUpdate.Update(lConnection, "references", new List<ArtifactLink> {new ArtifactLink {Id = lFirst}, new ArtifactLink {Id = lSecond}});


            Connection lConnection1 = new Connection(this.mUri, this.mKey);
            TrackerStructure lTargetStructure1 = lConnection.AddTrackerStructure(this.mSimpleTrackerId);
            Tracker<Artifact> lTargetTracker1 = new Tracker<Artifact>(lTargetStructure1);
            lTargetTracker1.PreviewRequest(lConnection1);
            Assert.Equal(lTargetTracker1.ArtifactIds.Count, lTargetTracker.ArtifactIds.Count);

            Artifact lResult = new Artifact(this.mSimpleTrackerId) {Id = lArtifactToUpdate.Id};
            lResult.Request(lConnection1, lTargetTracker1);

            Assert.Equal(lArtifactToUpdate.Id, lResult.Id);
            Assert.Contains(lFirst, lResult.GetFieldValue<List<ArtifactLink>>("references").Select(pRes => pRes.Id));
            Assert.Contains(lSecond, lResult.GetFieldValue<List<ArtifactLink>>("references").Select(pRes => pRes.Id));
        }
    }
}