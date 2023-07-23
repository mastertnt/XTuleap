using XTuleap.Tests;

namespace XTuleap.TestLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            TuleapTests lTest = new TuleapTests();
            //lTest.CreateSimpleArtifact();
            //lTest.UpdateString();
            lTest.UpdateReferences();
        }
    }
}
