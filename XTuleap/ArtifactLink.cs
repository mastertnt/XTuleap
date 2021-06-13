using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XTuleap
{
    /// <summary>
    /// This class represents an artifact link.
    /// </summary>
    public class ArtifactLink : IComparable
    {
        private int mId = -1;

        public int Id
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Reference) == false)
                {
                    string[] lArray = this.Reference.Split(new char[] {'#'}, StringSplitOptions.RemoveEmptyEntries);
                    return Convert.ToInt32(lArray[1]);
                }

                return this.mId;
            }
            set { this.mId = value; }
        }

        public string Reference
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        /// <summary>
        /// ToString.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return this.Id.ToString();
        }

        /// <summary>
        /// Compare to.
        /// </summary>
        /// <param name="pSecond">The second member.</param>
        /// <returns></returns>
        public int CompareTo(object pSecond)
        {
            if (pSecond == null)
            {
                return 1;
            }

            ArtifactLink lOther = pSecond as ArtifactLink;
            if (lOther != null)
            {
                return this.Id.CompareTo(lOther.Id);
            }

            throw new ArgumentException("Object is not an ArtifactLink");
        }
    }
}
