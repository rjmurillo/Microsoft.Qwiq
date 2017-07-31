using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Tfs = Microsoft.TeamFoundation.Framework.Client;

namespace Microsoft.Qwiq.Client.Soap
{
    [Serializable]
    internal sealed class TeamFoundationIdentity : Qwiq.TeamFoundationIdentity
    {
        //private readonly Tfs.TeamFoundationIdentity _identity;

        private readonly Dictionary<string, object> _properties;

        internal TeamFoundationIdentity(Tfs.TeamFoundationIdentity identity)
            : base(
                  identity.IsActive,
                  identity.TeamFoundationId,
                  identity.UniqueUserId)
        {
            Descriptor = new IdentityDescriptor(identity.Descriptor);
            MemberOf = identity.MemberOf?.Select(s => new IdentityDescriptor(s)).ToArray()
                        ?? ZeroLengthArrayOfIdentityDescriptor;
            Members = identity.Members?.Select(s => new IdentityDescriptor(s)).ToArray()
                        ?? ZeroLengthArrayOfIdentityDescriptor;
            DisplayName = identity.DisplayName;
            IsContainer = identity.IsContainer;
            UniqueName = identity.UniqueName;

            _properties = new Dictionary<string, object>(Comparer.OrdinalIgnoreCase);

            foreach (var p in identity.GetProperties())
            {
                _properties[p.Key] = p.Value;
            }
        }

#pragma warning disable CS0628 // New protected member declared in sealed class
        protected TeamFoundationIdentity(SerializationInfo info, StreamingContext context)
#pragma warning restore CS0628 // New protected member declared in sealed class
            : base(info, context)
        {
            Descriptor = (IdentityDescriptor) info.GetValue(nameof(Descriptor), typeof(IdentityDescriptor));
            DisplayName = info.GetString(nameof(DisplayName));
            IsContainer = info.GetBoolean(nameof(IsContainer));
            UniqueName = info.GetString(nameof(UniqueName));
            _properties = (Dictionary<string, object>) info.GetValue("Properties", typeof(Dictionary<string, object>));
            MemberOf = (IEnumerable<IIdentityDescriptor>) info.GetValue(nameof(MemberOf), typeof(IEnumerable<IIdentityDescriptor>)) ?? ZeroLengthArrayOfIdentityDescriptor;
            Members = (IEnumerable<IIdentityDescriptor>)info.GetValue(nameof(Members), typeof(IEnumerable<IIdentityDescriptor>)) ?? ZeroLengthArrayOfIdentityDescriptor;

        }

        public override IIdentityDescriptor Descriptor { get; }

        public override string DisplayName { get; }

        public override bool IsContainer { get; }

        public override IEnumerable<IIdentityDescriptor> MemberOf { get; }

        public override IEnumerable<IIdentityDescriptor> Members { get; }

        public override string UniqueName { get; }

        public override string GetAttribute(string name, string defaultValue)
        {
            return _properties.TryGetValue(name, out object obj2)
                ? obj2.ToString()
                : defaultValue;
        }

        public override IEnumerable<KeyValuePair<string, object>> GetProperties()
        {
            return _properties;
        }

        public override object GetProperty(string name)
        {
            return _properties[name];
        }
    }
}