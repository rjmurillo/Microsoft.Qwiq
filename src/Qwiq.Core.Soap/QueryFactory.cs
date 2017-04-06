using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Qwiq.Exceptions;

namespace Microsoft.Qwiq.Soap
{
    internal class QueryFactory : IQueryFactory
    {
        private readonly WorkItemStore _store;

        private QueryFactory(WorkItemStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public static IQueryFactory GetInstance(WorkItemStore store)
        {
            return new QueryFactory(store);
        }

        public IQuery Create(string wiql, bool dayPrecision)
        {
            return ExceptionHandlingDynamicProxyFactory.Create<IQuery>(
                new Query(new TeamFoundation.WorkItemTracking.Client.Query(_store.NativeWorkItemStore, wiql, null, dayPrecision), _store.PageSize));
        }

        public IQuery Create(IEnumerable<int> ids, string wiql)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            if (string.IsNullOrWhiteSpace(wiql)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(wiql));

            return ExceptionHandlingDynamicProxyFactory.Create<IQuery>(new Query(new TeamFoundation.WorkItemTracking.Client.Query(_store.NativeWorkItemStore, wiql, ids.ToArray()), _store.PageSize));
        }

        public IQuery Create(IEnumerable<int> ids, DateTime? asOf = null)
        {
            // The WIQL's WHERE and ORDER BY clauses are not used to filter (as we have specified IDs).
            // It is used for ASOF
            var wiql = $"SELECT {string.Join(", ", CoreFieldRefNames.All)} FROM WorkItems";
            if (asOf.HasValue) wiql += $" ASOF \'{asOf.Value:u}\'";
            return Create(ids, wiql);
        }
    }
}