using System;
using System.Diagnostics.Contracts;

using JetBrains.Annotations;

using Microsoft.Qwiq.Client.Soap;
using Microsoft.Qwiq.Exceptions;
using Microsoft.TeamFoundation.Framework.Client;

namespace Microsoft.Qwiq.Identity.Soap
{
    public static class Extensions
    {
        /// <summary>
        /// Gets the identity management service from an instance of <see cref="ITeamProjectCollection"/>.
        /// </summary>
        /// <param name="teamProjectCollection">An instance of <see cref="ITeamProjectCollection"/></param>
        /// <returns><see cref="IIdentityManagementService" />.</returns>
        /// <exception cref="ArgumentNullException">teamProjectCollection</exception>
        [NotNull]
        [JetBrains.Annotations.Pure]
        [PublicAPI]
        public static IIdentityManagementService GetIdentityManagementService([NotNull] this ITeamProjectCollection teamProjectCollection)
        {
            return GetIdentityManagementService(teamProjectCollection, true);
        }

        /// <summary>
        /// Gets the identity management service from an instance of <see cref="ITeamProjectCollection"/>.
        /// </summary>
        /// <param name="teamProjectCollection">An instance of <see cref="ITeamProjectCollection"/></param>
        /// <param name="exceptionHandlingProxyCreationEnabled">Determines if exception handling proxies should be created.</param>
        /// <returns><see cref="IIdentityManagementService" />.</returns>
        /// <exception cref="ArgumentNullException">teamProjectCollection</exception>
        [NotNull]
        [JetBrains.Annotations.Pure]
        [PublicAPI]
        public static IIdentityManagementService GetIdentityManagementService([NotNull] this ITeamProjectCollection teamProjectCollection, bool exceptionHandlingProxyCreationEnabled)
        {
            Contract.Requires(teamProjectCollection != null);

            if (teamProjectCollection == null) throw new ArgumentNullException(nameof(teamProjectCollection));
            var retval = ((IInternalTeamProjectCollection)teamProjectCollection).GetService<IIdentityManagementService2>();
            return !exceptionHandlingProxyCreationEnabled
                ? retval.AsProxy()
                : retval.AsExceptionHandlingProxy();
        }

        /// <summary>
        /// Gets the identity management service from an instance of <see cref="IWorkItemStore"/>.
        /// </summary>
        /// <param name="workItemStore">An instance of <see cref="IWorkItemStore"/>.</param>
        /// <returns><see cref="IIdentityManagementService" />.</returns>
        /// <exception cref="ArgumentNullException">workItemStore</exception>
        [NotNull]
        [JetBrains.Annotations.Pure]
        [PublicAPI]
        public static IIdentityManagementService GetIdentityManagementService([NotNull] this IWorkItemStore workItemStore)
        {
            if (workItemStore == null) throw new ArgumentNullException(nameof(workItemStore));
            return workItemStore.TeamProjectCollection.GetIdentityManagementService(workItemStore.Configuration.ProxyCreationEnabled);
        }

        [NotNull]
        [JetBrains.Annotations.Pure]
        internal static IIdentityDescriptor AsProxy([NotNull] this TeamFoundation.Framework.Client.IdentityDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            return ExceptionHandlingDynamicProxyFactory.Create<IIdentityDescriptor>(new Client.Soap.IdentityDescriptor(descriptor));
        }

        [NotNull]
        [JetBrains.Annotations.Pure]
        internal static IIdentityDescriptor AsExceptionHandlingProxy([NotNull] this TeamFoundation.Framework.Client.IdentityDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            return ExceptionHandlingDynamicProxyFactory.Create(descriptor.AsProxy());
        }

        [JetBrains.Annotations.Pure]
        [CanBeNull]
        internal static IIdentityManagementService AsProxy([CanBeNull] this IIdentityManagementService2 ims)
        {
            return ims == null
                       ? null
                       : new IdentityManagementService(ims, false);
        }

        [JetBrains.Annotations.Pure]
        [CanBeNull]
        internal static IIdentityManagementService AsExceptionHandlingProxy([CanBeNull] this IIdentityManagementService2 ims)
        {
            return ims == null
                ? null
                : ExceptionHandlingDynamicProxyFactory.Create(new IdentityManagementService(ims, true));
        }
    }
}