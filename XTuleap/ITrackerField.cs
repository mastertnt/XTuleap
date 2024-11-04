using System.Collections.Generic;

namespace XTuleap
{
    /// <summary>
    ///     This class represents a tracker field.
    /// </summary>
    public interface ITrackerField
    {
        public int Id
        {
            get;
            set;
        }

        public string? Label
        {
            get;
            set;
        }

        public string? Name
        {
            get;
            set;
        }

        public string? Type
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets the type as enum.
        /// </summary>
        public TrackerFieldType FieldType
        {
            get;
        }

        public List<EnumEntry> EnumValues
        {
            get;
            set;
        }
    }
}