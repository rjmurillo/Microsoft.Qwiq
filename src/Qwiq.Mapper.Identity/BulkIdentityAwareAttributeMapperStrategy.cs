using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

using FastMember;

using Microsoft.Qwiq.Identity;
using Microsoft.Qwiq.Mapper.Attributes;

namespace Microsoft.Qwiq.Mapper
{
    /// <summary>
    /// Class BulkIdentityAwareAttributeMapperStrategy.
    /// </summary>
    /// <seealso cref="WorkItemMapperStrategyBase" />
    public class BulkIdentityAwareAttributeMapperStrategy : WorkItemMapperStrategyBase
    {
        private readonly IPropertyInspector _inspector;
        private readonly IIdentityValueConverter _displayNameToAliasValueConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkIdentityAwareAttributeMapperStrategy" /> class.
        /// </summary>
        /// <param name="inspector">The inspector.</param>
        /// <param name="identityManagementService">The identity management service.</param>
        /// <exception cref="ArgumentNullException">
        /// inspector
        /// or
        /// identityManagementService
        /// </exception>
        public BulkIdentityAwareAttributeMapperStrategy(IPropertyInspector inspector, IIdentityManagementService identityManagementService)
            :this(inspector, new DisplayNameToAliasValueConverter(identityManagementService))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkIdentityAwareAttributeMapperStrategy"/> class.
        /// </summary>
        /// <param name="inspector">The inspector.</param>
        /// <param name="identityValueConverter">The identity value converter.</param>
        /// <exception cref="ArgumentNullException">inspector</exception>
        /// <autogeneratedoc />
        public BulkIdentityAwareAttributeMapperStrategy(
            IPropertyInspector inspector,
            IIdentityValueConverter identityValueConverter
            )
        {
            Contract.Requires(inspector != null);
            Contract.Requires(identityValueConverter != null);

            _inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
            _displayNameToAliasValueConverter = identityValueConverter ?? throw new ArgumentNullException(nameof(identityValueConverter));
        }

        /// <summary>
        /// Maps the specified target work item type.
        /// </summary>
        /// <param name="targeWorkItemType">Type of the targe work item.</param>
        /// <param name="workItemMappings">The work item mappings.</param>
        /// <param name="workItemMapper">The work item mapper.</param>
        public override void Map(Type targeWorkItemType, IEnumerable<KeyValuePair<IWorkItem, IIdentifiable<int?>>> workItemMappings, IWorkItemMapper workItemMapper)
        {
            if (!workItemMappings.Any()) return;

            var workingSet = workItemMappings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, WorkItemComparer.Default);
            var validIdentityProperties = GetWorkItemIdentityFieldNameToIdentityPropertyMap(targeWorkItemType, _inspector);
            if (!validIdentityProperties.Any())return;

            var accessor = TypeAccessor.Create(targeWorkItemType, true);
            var validIdentityFieldsWithWorkItems = GetWorkItemsWithIdentityFieldValues(workingSet.Keys, validIdentityProperties.Keys.ToList());
            var identitySearchTerms = GetIdentitySearchTerms(validIdentityFieldsWithWorkItems).ToList();

            var identitySearchResults = (Dictionary<string, string>)_displayNameToAliasValueConverter.Map(identitySearchTerms);



            foreach (var workItem in validIdentityFieldsWithWorkItems)
            {
                var targetObject = workingSet[workItem.WorkItem];
                foreach (var sourceField in workItem.ValidFields)
                {
                    var targetProperty = validIdentityProperties[sourceField.Name];
                    var mappedValue = identitySearchResults[sourceField.Value];

                    if (!string.IsNullOrEmpty(mappedValue))
                    {
                        accessor[targetObject, targetProperty.Name] = mappedValue;
                    }
                }
            }
        }



        private static ICollection<WorkItemWithFields> GetWorkItemsWithIdentityFieldValues(
            IEnumerable<IWorkItem> workItems,
            IReadOnlyCollection<string> witFieldNames)
        {
            return
                workItems.Select(
                    wi =>
                        new WorkItemWithFields
                        {
                            WorkItem = wi,
                            ValidFields =
                                witFieldNames
                                    .Where(fn => wi.Fields.Contains(fn))
                                    .Select(fn =>
                                                new WorkItemField
                                                {
                                                    Name = fn,
                                                    Value = wi[fn] as string
                                                })
                                    .Where(f => !string.IsNullOrEmpty(f.Value))
                        }).ToList();
        }

        internal static IEnumerable<string> GetIdentitySearchTerms(ICollection<WorkItemWithFields> workItemsWithIdentityFields)
        {
            return workItemsWithIdentityFields.SelectMany(wiwf => wiwf.ValidFields.Select(f => f.Value)).Distinct();
        }

        internal static IDictionary<string, PropertyInfo> GetWorkItemIdentityFieldNameToIdentityPropertyMap(Type targetWorkItemType, IPropertyInspector propertyInspector)
        {
            var identityProperties = propertyInspector.GetAnnotatedProperties(targetWorkItemType, typeof(IdentityFieldAttribute));
            return
                identityProperties
                .Select(
                    p =>
                        new
                        {
                            IdentityProperty = p,
                            WitFieldName = propertyInspector.GetAttribute<FieldDefinitionAttribute>(p)?.FieldName
                        })
                .Where(p => !string.IsNullOrEmpty(p.WitFieldName) && p.IdentityProperty.CanWrite)
                .ToDictionary(x => x.WitFieldName, x => x.IdentityProperty);
        }
    }
}

