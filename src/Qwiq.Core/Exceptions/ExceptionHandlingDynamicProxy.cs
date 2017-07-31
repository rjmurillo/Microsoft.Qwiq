using Castle.DynamicProxy;
using JetBrains.Annotations;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.ExceptionServices;

namespace Microsoft.Qwiq.Exceptions
{
    [DebuggerStepThrough]
    public class ExceptionHandlingDynamicProxy : IInterceptor
    {
        [NotNull]
        private readonly IExceptionMapper _exceptionMapper;

        public ExceptionHandlingDynamicProxy([NotNull] IExceptionMapper exceptionMapper)
        {
            Contract.Requires(exceptionMapper != null);

            _exceptionMapper = exceptionMapper;
        }

        /// <exception cref="ArgumentNullException"><paramref name="invocation"/> is <see langword="null"/></exception>
        public void Intercept([NotNull] IInvocation invocation)
        {
            Contract.Requires(invocation != null);
            if (invocation == null) throw new ArgumentNullException(nameof(invocation));
            try
            {
                invocation.Proceed();
            }
            // ReSharper disable CatchAllClause
            catch (Exception e)
            // ReSharper restore CatchAllClause
            {
                // .NET 4.5 feature: Capture an exception and re-throw it without changing the stack trace
                ExceptionDispatchInfo.Capture(_exceptionMapper.Map(e)).Throw();
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_exceptionMapper != null);
        }
    }
}