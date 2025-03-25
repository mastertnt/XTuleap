using System;
using System.Collections.Generic;
using System.Text;

namespace XTuleap
{
    /// <summary>
    /// This class represents a step definition in Tuleap.
    /// </summary>
    public class StepDefinition
    {
        /// <summary>
        /// ID of the step definition.
        /// </summary>
        
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Description of the step definition.
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Expected results of the step definition.
        /// </summary>
        public string ExpectedResults
        {
            get;
            set;
        }

        /// <summary>
        /// Rank of the step definition.
        /// </summary>
        public int Rank
        {
            get;
            set;
        }
    }
}
