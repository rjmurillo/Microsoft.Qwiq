using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Qwiq
{
    [Serializable]
    public abstract class TeamFoundationIdentity : ITeamFoundationIdentity, IEquatable<ITeamFoundationIdentity>, ISerializable
    {
        protected internal static readonly IIdentityDescriptor[] ZeroLengthArrayOfIdentityDescriptor = new IIdentityDescriptor[0];
        private string _uniqueName;

        protected internal TeamFoundationIdentity(
            bool isActive,
            Guid teamFoundationId,
            int uniqueUserId
            )
        {
            IsActive = isActive;
            TeamFoundationId = teamFoundationId;
            UniqueUserId = uniqueUserId;
        }

        protected TeamFoundationIdentity(SerializationInfo info, StreamingContext context)
        {
            IsActive = info.GetBoolean(nameof(IsActive));
            UniqueUserId = info.GetInt32(nameof(UniqueUserId));
            TeamFoundationId = (Guid)info.GetValue(nameof(TeamFoundationId), typeof(Guid));
        }

        public abstract IIdentityDescriptor Descriptor { get; }

        public abstract string DisplayName { get; }

        public bool IsActive { get; }

        public virtual bool IsContainer
        {
            get
            {
                var schema = GetAttribute(IdentityAttributeTags.SchemaClassName, null);
                if (!string.IsNullOrEmpty(schema) && string.Equals(
                        schema,
                        IdentityConstants.SchemaClassGroup,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }
        }

        public abstract IEnumerable<IIdentityDescriptor> MemberOf { get; }

        public abstract IEnumerable<IIdentityDescriptor> Members { get; }

        public Guid TeamFoundationId { get; }

        public virtual string UniqueName
        {
            get
            {
                if (!string.IsNullOrEmpty(_uniqueName)) return _uniqueName;

                var domain = GetAttribute(IdentityAttributeTags.Domain, string.Empty);
                var account = GetAttribute(IdentityAttributeTags.AccountName, string.Empty);

                if (UniqueUserId == IdentityConstants.ActiveUniqueId)
                {
                    _uniqueName = string.IsNullOrEmpty(domain)
                                      ? account
                                      : string.Format(
                                          IdentityConstants.DomainQualifiedAccountNameFormat,
                                          domain,
                                          account);
                }
                else
                {
                    _uniqueName = string.IsNullOrEmpty(domain)
                                      ? $"{account}:{UniqueUserId.ToString(CultureInfo.InvariantCulture)}"
                                      : $"{string.Format(IdentityConstants.DomainQualifiedAccountNameFormat, domain, account)}:{UniqueUserId.ToString(CultureInfo.InvariantCulture)}";
                }

                return _uniqueName;
            }
        }

        public int UniqueUserId { get; }

        public bool Equals(ITeamFoundationIdentity other)
        {
            return TeamFoundationIdentityComparer.Default.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ITeamFoundationIdentity);
        }

        public abstract string GetAttribute(string name, string defaultValue);

        public override int GetHashCode()
        {
            return Comparer.TeamFoundationIdentity.GetHashCode(this);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(UniqueUserId), UniqueUserId);
            info.AddValue(nameof(TeamFoundationId), TeamFoundationId);
            info.AddValue(nameof(IsActive), IsActive);
            info.AddValue(nameof(Descriptor), Descriptor);
            info.AddValue(nameof(IsContainer), IsContainer);
            info.AddValue(nameof(DisplayName), DisplayName);
            info.AddValue(nameof(UniqueName), UniqueName);
            info.AddValue(nameof(Members), Members);
            info.AddValue(nameof(MemberOf), MemberOf);
            info.AddValue("Properties", GetProperties());
        }

        public abstract IEnumerable<KeyValuePair<string, object>> GetProperties();

        public abstract object GetProperty(string name);

        public override string ToString()
        {
            // Call of .ToString to avoid boxing Guid to Object
            // ReSharper disable RedundantToStringCallForValueType
            return $"Identity {TeamFoundationId.ToString()} (IdentityType: {(Descriptor == null ? string.Empty : Descriptor.IdentityType)}; Identifier: {(Descriptor == null ? string.Empty : Descriptor.Identifier)}; DisplayName: {DisplayName})";
            // ReSharper restore RedundantToStringCallForValueType
        }
    }
}