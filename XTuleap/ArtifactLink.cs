﻿using System;

namespace XTuleap
{
    /// <summary>
    ///     This class represents an artifact link.
    /// </summary>
    public class ArtifactLink : IComparable
    {
        /// <summary>
        /// Field to store the id of the link.
        /// </summary>
        private int mId = -1;
        
        /// <summary>
        ///    Gets or sets the id of the link.
        /// </summary>
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

        /// <summary>
        ///     Flag to indicate if the link is reverse.
        /// </summary>
        public bool IsReverse
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets reference of the link.
        /// </summary>
        public string Reference
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets url of the link.
        /// </summary>
        public string Url
        {
            get;
            set;
        }

        /// <summary>
        ///     Compare to.
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

        /// <summary>
        ///     ToString.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}