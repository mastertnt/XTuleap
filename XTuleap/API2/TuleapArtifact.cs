using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using NLog;
using TuleapTestTransfer.Extensions;

namespace XTuleap.API2
{
    /// <summary>
    /// Base class to create/update/delete an artifact.
    /// </summary>
    public class TuleapArtifact : Artifact, IIdentifiable
    {
        /// <summary>
        /// Logger of the class.
        /// </summary>
        private static readonly Logger msLogger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Raised when the synchronization state has changed.
        /// </summary>
        public event Action SynchronizationChanged;

        /// <summary>
        /// Flag to know if the data has been modified locally.
        /// </summary>
        public bool NeedsSynchronization
        {
            get;
            set;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TuleapArtifact()
        {
            this.NeedsSynchronization = false;
            this.PropertyChanged += this.OnPropertyChanged;
        }

        /// <summary>
        /// Checks if the artifact is synchronized with tuleap.
        /// </summary>
        /// <param name="pEventSender">The event sender.</param>
        /// <param name="pEventArgs">The event arguments.</param>
        protected virtual void OnPropertyChanged(object? pEventSender, PropertyChangedEventArgs pEventArgs)
        {
            if (pEventArgs.PropertyName != nameof(this.NeedsSynchronization))
            {
                msLogger.Trace("Property changed : " + pEventArgs.PropertyName);
                this.NeedsSynchronization = true;
            }
            else
            {
                if (this.SynchronizationChanged != null)
                {
                    this.SynchronizationChanged();
                }

            }
        }

        /// <inheritdoc />
        public override void Request(Connection pConnection, ITracker? pTracker = null)
        {
            base.Request(pConnection, pTracker);
            this.InitializeFromTuleap(this);
        }

        /// <summary>
        /// Initialize the artifact from a core artifact.
        /// </summary>
        /// <param name="pArtifact">The core artifact.</param>
        public virtual void InitializeFromTuleap(Artifact pArtifact)
        {
            this.Id = pArtifact.Id;
            IEnumerable<PropertyInfo> lProperties = this.GetType().GetProperties().Where(pProperty => pProperty.IsDefined(typeof(TuleapField), false));
            foreach (PropertyInfo lPropertyInfo in lProperties)
            {
                TuleapField[] lAttributes = (TuleapField[])lPropertyInfo.GetCustomAttributes(typeof(TuleapField), false);
                msLogger.Log(LogLevel.Trace, "TuleapArtifact.InitializeFromTuleap " + lPropertyInfo.Name + " with " + lAttributes.First().FieldName);
                object lTuleapValue = pArtifact.GetFieldValue(lAttributes.First().FieldName);
                if (lAttributes.First() is TuleapLink)
                {
                    if (lTuleapValue is IEnumerable lTuleapEnumerable)
                    {
                        List<int> lIds = new List<int>();
                        foreach (object? lElement in lTuleapEnumerable)
                        {
                            if (lElement is ArtifactLink lLink)
                            {
                                lIds.Add(lLink.Id);
                            }
                        }

                        object? lLinkObject = lPropertyInfo.GetValue(this);
                        if (lLinkObject is IEnumerable<int>)
                        {
                            lPropertyInfo.SetValue(this, lIds);
                        }
                        else if (lLinkObject is int)
                        {
                            if (lIds.Count == 1)
                            {
                                lPropertyInfo.SetValue(this, lIds[0]);
                            }
                        }
                        else
                        {
                            msLogger.Error("The type of link must be int or IEnumerable<int>. Please change : " + lAttributes.First().FieldName);
                        }
                    }
                }
                else
                {
                    if (lPropertyInfo.PropertyType.IsEnum)
                    {
                        Enum.TryParse(lPropertyInfo.PropertyType, lTuleapValue.ToString(), true, out object? lEnumValue);
                        if (lEnumValue != null)
                        {
                            lPropertyInfo.SetValue(this, lEnumValue);
                        }
                    }
                    else
                    {
                        lPropertyInfo.SetValue(this, lTuleapValue);
                    }
                }
            }
            this.NeedsSynchronization = false;
        }

        /// <summary>
        /// Creates the data on tuleap.
        /// </summary>
        /// <param name="pConnection">The tuleap connection</param>
        /// <param name="pTrackerId">Id of the tracker</param>
        public virtual void CreateTuleap(Connection pConnection, int pTrackerId = 0)
        {
            Artifact lArtifact = new Artifact(pTrackerId);
            IEnumerable<PropertyInfo> lProperties = this.GetType().GetProperties().Where(pProperty => pProperty.IsDefined(typeof(TuleapField), false));
            Dictionary<string, object?> lValues = new Dictionary<string, object?>();
            string lArtifactLink = string.Empty;
            List<ArtifactLink> lLinks = new List<ArtifactLink>();
            foreach (PropertyInfo lPropertyInfo in lProperties)
            {
                if (lPropertyInfo.IsDefined(typeof(ReadOnlyAttribute), false) == false)
                {
                    msLogger.Log(LogLevel.Trace, "TuleapArtifact.CreateTuleap " + lPropertyInfo.Name + " with " + lPropertyInfo.GetValue(this));
                    TuleapField[] lAttributes = (TuleapField[])lPropertyInfo.GetCustomAttributes(typeof(TuleapField), false);
                    if (lAttributes.First() is TuleapLink)
                    {
                        if (string.IsNullOrEmpty(lArtifactLink))
                        {
                            lArtifactLink = lAttributes.First().FieldName;
                        }
                        else
                        {
                            if (lArtifactLink != lAttributes.First().FieldName)
                            {
                                msLogger.Error("Cannot have different field name for artifact links. Please change : " + lAttributes.First().FieldName);
                            }
                        }

                        object? lLinkObject = lPropertyInfo.GetValue(this);
                        if (lLinkObject is IEnumerable<int> lIds)
                        {
                            foreach (int lId in lIds)
                            {
                                ArtifactLink lLink = new ArtifactLink { Id = lId };
                                lLinks.Add(lLink);
                            }
                        }
                        else if (lLinkObject is int lId)
                        {
                            ArtifactLink lLink = new ArtifactLink { Id = lId };
                            lLinks.Add(lLink);
                        }
                        else
                        {
                            msLogger.Error("The type of link must be int or IEnumerable<int>. Please change : " + lAttributes.First().FieldName);
                        }
                    }
                    else
                    {
                        lValues.Add(lAttributes.First().FieldName, lPropertyInfo.GetValue(this));
                    }
                }
            }
            if (string.IsNullOrEmpty(lArtifactLink) == false)
            {
                lValues.Add(lArtifactLink, lLinks);
            }

            lArtifact.Create(pConnection, lValues);
            this.Id = lArtifact.Id;
            this.NeedsSynchronization = false;
        }


        /// <summary>
        /// Updates the data on tuleap.
        /// </summary>
        /// <param name="pConnection">The tuleap connection</param>
        /// <param name="pTrackerId">Id of the tracker</param>
        public virtual void UpdateTuleap(Connection pConnection, int pTrackerId = 0)
        {
            Artifact lArtifact = new Artifact(pTrackerId)
            {
                Id = this.Id
            };

            IEnumerable<PropertyInfo> lProperties = this.GetType().GetProperties().Where(pProperty => pProperty.IsDefined(typeof(TuleapField), false));
            string lArtifactLink = string.Empty;
            List<ArtifactLink> lLinks = new List<ArtifactLink>();
            foreach (PropertyInfo lPropertyInfo in lProperties)
            {
                if (lPropertyInfo.IsDefined(typeof(ReadOnlyAttribute), false) == false)
                {
                    try
                    {
                        msLogger.Log(LogLevel.Trace, "TuleapArtifact.UpdateTuleap " + lPropertyInfo.Name + " to " + lPropertyInfo.GetValue(this));
                        TuleapField[] lAttributes = (TuleapField[])lPropertyInfo.GetCustomAttributes(typeof(TuleapField), false);
                        if (lAttributes.First() is TuleapLink)
                        {
                            if (string.IsNullOrEmpty(lArtifactLink))
                            {
                                lArtifactLink = lAttributes.First().FieldName;
                            }
                            else
                            {
                                if (lArtifactLink != lAttributes.First().FieldName)
                                {
                                    msLogger.Error("Cannot have different field name for artifact links. Please change : " + lAttributes.First().FieldName);
                                }
                            }

                            object? lLinkObject = lPropertyInfo.GetValue(this);
                            if (lLinkObject is IEnumerable<int> lIds)
                            {
                                foreach (int lId in lIds)
                                {
                                    ArtifactLink lLink = new ArtifactLink { Id = lId };
                                    lLinks.Add(lLink);
                                }
                            }
                            else if (lLinkObject is int lId)
                            {
                                ArtifactLink lLink = new ArtifactLink { Id = lId };
                                lLinks.Add(lLink);
                            }
                            else
                            {
                                msLogger.Error("The type of link must be int or IEnumerable<int>. Please change : " + lAttributes.First().FieldName);
                            }
                        }
                        else
                        {
                            object? lValue = lPropertyInfo.GetValue(this);
                            string lFieldName = lAttributes.First().FieldName;
                            lArtifact.Update(pConnection, lFieldName, lValue);
                        }
                    }
                    catch (Exception lE)
                    {
                        msLogger.Log(LogLevel.Error, "Cannot update field " + lPropertyInfo.Name);
                    }
                }
            }
            if (string.IsNullOrEmpty(lArtifactLink) == false)
            {
                lArtifact.Update(pConnection, lArtifactLink, lLinks);
            }
            this.NeedsSynchronization = false;
        }

        /// <summary>
        /// Deletes the data on tuleap.
        /// </summary>
        /// <param name="pConnection">The tuleap connection</param>
        /// <param name="pTrackerId">Id of the tracker</param>
        public virtual bool DeleteTuleap(Connection pConnection, int pTrackerId = 0)
        {
            Artifact lArtifact = new Artifact(pTrackerId)
            {
                Id = this.Id
            };
            return lArtifact.Delete(pConnection);
        }

        protected int GetLinkFromList<T>(ObservableCollection<T> pSource, List<int> pIds) where T : IIdentifiable
        {
            return this.GetLinksFromList(pSource, pIds).FirstOrDefault();
        }

        protected ObservableCollection<int> GetLinksFromList<T>(ObservableCollection<T> pSource, List<int> pIds) where T : IIdentifiable
        {
            List<int> lIds = pSource.Select(pItem => pItem.Id).ToList();
            return pIds.Where(pId => lIds.Contains(pId)).ToObservableCollection();
        }

        protected void SetLinkFromList<T>(ObservableCollection<T> pSource, List<int> pIds, int pNewValue) where T : IIdentifiable
        {
            this.SetLinksFromList(pSource, pIds, new ObservableCollection<int>() { pNewValue });
        }

        protected void SetLinksFromList<T>(ObservableCollection<T> pSource, List<int> pIds, ObservableCollection<int> pNewValues) where T : IIdentifiable
        {
            List<int> lIds = pSource.Select(pItem => pItem.Id).ToList();
            pIds.RemoveAll(pId => lIds.Contains(pId));
            foreach (int lValue in pNewValues)
            {
                pIds.Add(lValue);
            }
        }
    }
}
