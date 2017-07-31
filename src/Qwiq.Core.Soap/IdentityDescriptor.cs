using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Tfs = Microsoft.TeamFoundation.Framework.Client;

namespace Microsoft.Qwiq.Client.Soap
{
    [Serializable]
    internal sealed class IdentityDescriptor : Qwiq.IdentityDescriptor, ISerializable
    {
        internal IdentityDescriptor([CanBeNull] Tfs.IdentityDescriptor descriptor)
            : base(descriptor?.IdentityType, descriptor?.Identifier)
        {
            Contract.Requires(descriptor != null);

            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
        }

        private IdentityDescriptor(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {
        }
    }
}