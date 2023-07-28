using XTuleap.Tests;

namespace XTuleap.TestLauncher;

internal class Program
{
    private static void Main(string[] args)
    {
        TuleapTests lTest = new();
        lTest.Request();
        lTest.CreateSimpleArtifact();
        lTest.UpdateString();
        lTest.UpdateReferences();
        lTest.CreateArtifactWithEnum();
    }
}