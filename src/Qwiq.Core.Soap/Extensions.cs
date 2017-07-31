using JetBrains.Annotations;
using Microsoft.Qwiq.Exceptions;

namespace Microsoft.Qwiq.Client.Soap
{
    internal static class Extensions
    {
        [CanBeNull]
        [Pure]
        internal static ITeamFoundationIdentity AsProxy([CanBeNull] this TeamFoundation.Framework.Client.TeamFoundationIdentity identity)
        {
            return identity == null
                ? null
                : new TeamFoundationIdentity(identity);
        }

        [CanBeNull]
        [Pure]
        internal static ITeamFoundationIdentity AsExceptionHandlingProxy([CanBeNull] this TeamFoundation.Framework.Client.TeamFoundationIdentity identity)
        {
            return identity == null
                ? null
                : ExceptionHandlingDynamicProxyFactory.Create(identity.AsProxy());
        }

        [CanBeNull]
        [Pure]
        internal static IInternalTeamProjectCollection AsProxy([CanBeNull] this TeamFoundation.Client.TfsTeamProjectCollection tfsNative)
        {
            return tfsNative == null
                ? null
                : new TfsTeamProjectCollection(tfsNative);
        }

        [CanBeNull]
        [Pure]
        internal static IInternalTeamProjectCollection AsExceptionHandlingProxy([CanBeNull] this TeamFoundation.Client.TfsTeamProjectCollection tfsNative)
        {
            return tfsNative == null
                       ? null
                       : ExceptionHandlingDynamicProxyFactory.Create(tfsNative.AsProxy());
        }
    }
}
