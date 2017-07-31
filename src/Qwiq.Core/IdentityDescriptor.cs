using JetBrains.Annotations;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Qwiq
{
    [Serializable]
    public class IdentityDescriptor : IIdentityDescriptor, IComparable<IdentityDescriptor>, IEquatable<IdentityDescriptor>, ISerializable
    {
        [NotNull] private string _identifier;

        /// <summary>
        /// </summary>
        /// <param name="identityType"></param>
        /// <param name="identifier"></param>
        /// <example>
        ///     User:
        ///     "Microsoft.IdentityModel.Claims.ClaimsIdentity", "2fa3a376-370f-4226-9fbb-d778e4b5bf74\\ftotten@fabrikam.com"
        ///     Service:
        ///     "Microsoft.TeamFoundation.ServiceIdentity",
        ///     "d9454f90-6587-4699-9357-3e83e331580a:Build:f2200ea9-52cf-4343-8c80-af2cfa409984"
        ///     TFS Identity:
        ///     "Microsoft.TeamFoundation.Identity",
        ///     "S-1-9-1234567890-1234567890-123456789-1234567890-1234567890-1-1234567890-1234567890-1234567890-1234567890"
        /// </example>
        /// <exception cref="ArgumentException">Value cannot be null or whitespace.</exception>
        public IdentityDescriptor([NotNull] string identityType, [NotNull] string identifier)
        {
            if (string.IsNullOrWhiteSpace(identityType))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(identityType));
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(identifier));

            IdentityType = identityType;
            Identifier = identifier;
        }

        protected IdentityDescriptor(SerializationInfo info, StreamingContext context)
        {
            IdentityType = (string)info.GetValue(nameof(IdentityType), typeof(string));
            Identifier = (string)info.GetValue(nameof(Identifier), typeof(string));
        }

        public string Identifier
        {
            get => _identifier;
            private set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
                if (value.Length > IdentityConstants.MaxIdLength) throw new ArgumentOutOfRangeException(nameof(value));
                _identifier = value;
            }
        }

        public string IdentityType
        {
            get => IdentityTypeMapper.Instance.GetTypeNameFromId(IdentityTypeId);
            private set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
                if (value.Length > IdentityConstants.MaxTypeLength) throw new ArgumentOutOfRangeException(nameof(value));
                IdentityTypeId = IdentityTypeMapper.Instance.GetTypeIdFromName(value);
            }
        }

        protected internal byte IdentityTypeId { get; private set; }

        public int CompareTo(IdentityDescriptor other)
        {
            if (this == other) return 0;
            if (this == null && other != null) return -1;
            if (this != null && other == null) return 1;

            var num = 0;
            if (IdentityTypeId > other.IdentityTypeId) num = 1;
            else if (IdentityTypeId < other.IdentityTypeId) num = -1;

            if (num == 0) num = StringComparer.OrdinalIgnoreCase.Compare(Identifier, other.Identifier);
            return num;
        }

        /// <inheritdoc />
        public bool Equals(IdentityDescriptor other)
        {
            return CompareTo(other) == 0;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as IdentityDescriptor);
        }

        public override int GetHashCode()
        {
            return IdentityTypeId ^ Comparer.OrdinalIgnoreCase.GetHashCode(Identifier);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(IdentityType), IdentityType);
            info.AddValue(nameof(Identifier), Identifier);
        }
        public override string ToString()
        {
            return IdentityTypeMapper.Instance.GetTypeNameFromId(IdentityTypeId) + ";" + _identifier;
        }
    }
}