using System;
using System.Diagnostics.Contracts;

using JetBrains.Annotations;

namespace Microsoft.Qwiq.Client.Rest
{
    internal sealed class IdentityDescriptor : Qwiq.IdentityDescriptor
    {
        internal IdentityDescriptor([CanBeNull] VisualStudio.Services.Identity.IdentityDescriptor descriptor)
            : base(descriptor?.IdentityType, descriptor?.Identifier)
        {
            Contract.Requires(descriptor != null);

            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
        }
    }
}