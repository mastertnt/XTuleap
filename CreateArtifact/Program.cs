using System;
using System.Collections.Generic;
using System.Linq;
using XTuleap;

namespace CreateArtifact
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Create an artifact");

            Connection lConnection = new Connection("https://tuleap.net/api/", "tlp-k1-47.4c7cdc77dbb8f14a1c63d1b47b339b8c7e2d026cdd32fb1a4179a14f181af5cb");
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(861);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);
            lTargetTracker.Request(lConnection);

            Artifact lNewArtifact = new Artifact();

            Dictionary<string, object> lValues = new Dictionary<string, object>
            {
                {"title", "mytitle"}, {"id_testlink", "MP-1"}, {"status", "OK"}
            };


            lNewArtifact.Create(lConnection, lTargetStructure, lValues);

            
        }
    }
}
