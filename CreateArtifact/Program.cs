﻿using XTuleap.Tests;

namespace XTuleap.TestLauncher;

internal class Program
{
    private static void Main(string[] args)
    {
        TuleapTests lTest = new();
        lTest.Preview();
        //lTest.Clone();
        //lTest.CreateSimpleArtifact();
        //lTest.UpdateText();
        //lTest.UpdateReferences();
        //lTest.CreateArtifactWithEnum();
        //lTest.Display();
        //lTest.UpdateString();
    }
}