using System;

namespace XTuleap.API2
{
    /// <summary>
    /// Attribute used to declare an attribute on a field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TuleapLink : TuleapField
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="pFieldName"></param>
        public TuleapLink(string pFieldName)
        : base(pFieldName)
        {
        }
    }
}
