﻿using System;
using System.Collections.Generic;
using System.Linq;
using XTuleap;
using XTuleap.Tests;

namespace CreateArtifact
{
    class Program
    {
        static void Main(string[] args)
        {
            TuleapTests lTest = new TuleapTests();
            lTest.CreateSimpleArtifact();
            lTest.UpdateString();
            lTest.UpdateReference();
        }
    }
}
