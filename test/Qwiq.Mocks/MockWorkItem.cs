using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Microsoft.Qwiq.Mocks
{
    [Serializable]
    public class MockWorkItem : IWorkItem
    {
        internal bool PartialOpenWasCalled = false;
        private readonly ICollection<ILink> _links;
        private readonly Dictionary<string, IField> _properties;

        private IWorkItemType _type;

        [Obsolete(
            "This method has been deprecated and will be removed in a future release. See a constructor that takes IWorkItemType and fields.")]
        public MockWorkItem()
            : this((string)null, null as IEnumerable<IField>)
        {
        }

        [Obsolete(
            "This method has been deprecated and will be removed in a future release. See a constructor that takes IWorkItemType and fields.")]
        public MockWorkItem(string workItemType = null)
            : this(workItemType, null as IEnumerable<IField>)
        {
        }

        [Obsolete(
            "This method has been deprecated and will be removed in a future release. See a constructor that takes IWorkItemType and fields.")]
        public MockWorkItem(IDictionary<string, object> fields)
            : this((string)null, fields)
        {
        }

        [Obsolete(
            "This method has been deprecated and will be removed in a future release. See a constructor that takes IWorkItemType and fields.")]
        public MockWorkItem(string workItemType = null, IDictionary<string, object> fields = null)
            : this(workItemType, fields?.Select(p => new MockField(p.Value, p.Value) { Name = p.Key }))
        {
        }

        [Obsolete(
            "This method has been deprecated and will be removed in a future release. See a constructor that takes IWorkItemType and fields.")]
        internal MockWorkItem(string workItemType = null, IEnumerable<IField> fields = null)
            :this(string.IsNullOrEmpty(workItemType) ? new MockWorkItemType() : new MockWorkItemType(workItemType), fields)
        {
        }

        public MockWorkItem(IWorkItemType type)
            : this(type, type.FieldDefinitions.ToDictionary(p => p.Name, e => (object)null))
        {
        }

        public MockWorkItem(IWorkItemType type, IDictionary<string, object> fields = null)
            : this(type, fields?.Select(p => new MockField(p.Value, p.Value) { Name = p.Key }))
        {
        }

        public MockWorkItem(IWorkItemType type, IEnumerable<IField> fields)
        {
            _links = new MockLinkCollection();
            _properties = new Dictionary<string, IField>(StringComparer.OrdinalIgnoreCase);

            if (fields != null)
            {
                foreach (var prop in fields)
                {
                    _properties[prop.Name] = prop;
                }
            }
            Type = type;
            Revisions = Enumerable.Empty<IRevision>();
        }

        public string AreaPath
        {
            get { return (string)GetValue("Area Path"); }
            set
            {
                SetValue("Area Path", value);
                SetValue("System.AreaPath", value);
            }
        }

        public string AssignedTo
        {
            get
            {
                return (string)GetValue("Assigned To");
            }
            set
            {
                SetValue("Assigned To", value);
                SetValue("System.AssignedTo", value);
            }
        }

        public int AttachedFileCount
        {
            get { return (int)GetValue("Attached File Count"); }
            set { SetValue("Attached File Count", value); }
        }

        public IEnumerable<IAttachment> Attachments
        {
            get { throw new NotImplementedException(); }
        }

        public string ChangedBy
        {
            get { return (string)GetValue("Changed By"); }
            set
            {
                SetValue("Changed By", value);
                SetValue("System.ChangedBy", value);
            }
        }

        public DateTime ChangedDate
        {
            get { return (DateTime)GetValue("Changed Date"); }
            set
            {
                SetValue("Changed Date", value);
                SetValue("System.ChangedDate", value);
            }
        }

        public string CreatedBy
        {
            get { return (string)GetValue("Created By"); }
            set
            {
                SetValue("Created By", value);
                SetValue("Microsoft.VSTS.Common.CreatedBy", value);
            }
        }

        public DateTime CreatedDate
        {
            get { return (DateTime)GetValue("Created Date"); }
            set
            {
                SetValue("Created Date", value);
                SetValue("Microsoft.VSTS.Common.CreatedDate", value);
            }
        }

        public string Description
        {
            get { return (string)GetValue("Description"); }
            set
            {
                SetValue("Description", value);
                SetValue("System.Description", value);
            }
        }

        public int ExternalLinkCount
        {
            get { return (int)GetValue("External Link Count"); }
            set { SetValue("External Link Count", value); }
        }

        public IFieldCollection Fields => new MockFieldCollection(_properties);

        public string History
        {
            get { return GetValue("History") as string ?? string.Empty; }
            set
            {
                SetValue("History", value);
                SetValue("System.History", value);
            }
        }

        public int HyperLinkCount
        {
            get { return (int)GetValue("Hyper Link Count"); }
            set { SetValue("Hyper Link Count", value); }
        }

        public int Id
        {
            get { return ((int?)GetValue("Id")).GetValueOrDefault(0); }
            set
            {
                SetValue("Id", value);
                SetValue("System.Id", value);
            }
        }

        public bool IsDirty
        {
            get { return _properties.Select(p => p.Value.IsDirty).Any(); }
        }

        public string IterationPath
        {
            get
            {
                return (string)GetValue("Iteration Path");
            }
            set
            {
                SetValue("Iteration Path", value);
                SetValue("System.IterationPath", value);
            }
        }

        public string Keywords
        {
            get { return (string)GetValue("Keywords"); }
            set { SetValue("Keywords", value); }
        }

        public ICollection<ILink> Links { get; set; }

        public int RelatedLinkCount => Links.OfType<IRelatedLink>().Count();

        public string ReproSteps
        {
            get { return (string)GetValue("Repro Steps"); }
            set
            {
                SetValue("Repro Steps", value);
                SetValue("Microsoft.VSTS.TCM.ReproSteps", value);
            }
        }

        public long Rev
        {
            get
            {
                return (long)GetValue("Rev");
            }
            set
            {
                SetValue("Rev", value);
                SetValue("System.Rev", value);
            }
        }

        public DateTime RevisedDate
        {
            get { return (DateTime)GetValue("Revised Date"); }
            set { SetValue("Revised Date", value); }
        }

        public long Revision
        {
            get { return (long)GetValue("Revision"); }
            set { SetValue("Revision", value); }
        }

        public IEnumerable<IRevision> Revisions
        {
            get { return (IEnumerable<IRevision>)GetValue("Revisions"); }
            set { SetValue("Revisions", value); }
        }

        public string State
        {
            get { return (string)GetValue("State"); }
            set
            {
                SetValue("State", value);
                SetValue("System.State", value);
            }
        }

        public string Tags
        {
            get { return (string)GetValue("Tags"); }
            set { SetValue("Tags", value); }
        }

        public string Title
        {
            get { return (string)GetValue("Title"); }
            set
            {
                SetValue("Title", value);
                SetValue("System.Title", value);
            }
        }

        public IWorkItemType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                SetValue("System.WorkItemType", value.Name);
                SetValue("Work Item Type", value.Name);
            }
        }

        public Uri Uri
        {
            get { return (Uri)GetValue("Uri"); }
            set { SetValue("Uri", value); }
        }

        public object this[string name]
        {
            get { return GetValue(name); }
            set { SetValue(name, value); }
        }

        public void Close()
        {
        }

        public IWorkItem Copy()
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Position = 0;

                var newItem = (MockWorkItem)formatter.Deserialize(stream);
                newItem.Id = 0;

                var link = newItem.CreateRelatedLink(this);
                newItem.Links.Add(link);

                return newItem;
            }
        }

        public IHyperlink CreateHyperlink(string location)
        {
            throw new NotImplementedException();
        }

        public IRelatedLink CreateRelatedLink(IWorkItem target)
        {
            return CreateRelatedLink(new MockWorkItemStore().WorkItemLinkTypes.Single(s => s.ReferenceName == CoreLinkTypeReferenceNames.Related).ForwardEnd, target);
        }

        public IRelatedLink CreateRelatedLink(IWorkItemLinkTypeEnd linkTypeEnd, IWorkItem relatedWorkItem)
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            return Validate() == null;
        }

        public void Open()
        {
        }

        public void PartialOpen()
        {
            PartialOpenWasCalled = true;
        }

        public void Reset()
        {
        }

        public void Save()
        {
        }

        public void Save(SaveFlags flags)
        {
        }

        public IEnumerable<IField> Validate()
        {
            var invalidFields = _properties.Where(p => !p.Value.IsValid).Select(p => p.Value).ToArray();
            return invalidFields.Any()
                ? invalidFields
                : null;
        }
        //public IRelatedLink CreateRelatedLink(IWorkItemLinkTypeEnd end, IWorkItem target)
        //{
        //    return new MockWorkItemLink(end)
        //    {
        //        RelatedWorkItemId = target.Id,
        //        LinkSubType = "Related"
        //    };
        //}

        //public IHyperlink CreateHyperlink(string location)
        //{
        //    return new MockHyperlink(location);
        //}

        private object GetValue(string field)
        {
            IField val;
            return _properties.TryGetValue(field, out val)
                ? val.Value
                : null;
        }

        private void SetValue(string field, object value)
        {
            IField val;
            if (_properties.TryGetValue(field, out val))
            {
                val.Value = value;
            }
            else
            {
                _properties.Add(field, new MockField(value, value) { Name = field });
            }
        }

        public void ApplyRules(bool doNotUpdateChangedBy = false)
        {
        }
    }
}
