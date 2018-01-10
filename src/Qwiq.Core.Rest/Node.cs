using System;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Microsoft.Qwiq.Client.Rest
{
    internal class Node : Node<IWorkItemClassificationNode, int>
    {
        internal Node([NotNull] TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemClassificationNode node)
            : this(node, null)
        {
        }

        internal Node([NotNull] TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemClassificationNode node, [CanBeNull] INode<IWorkItemClassificationNode, int> parentNode)
            : base(
                  new WorkItemClassificationNode(
                    node.Id,
                    node.StructureType == TreeNodeStructureType.Area ? WorkItemClassificationNodeType.Area : node.StructureType == TreeNodeStructureType.Iteration ? WorkItemClassificationNodeType.Iteration : WorkItemClassificationNodeType.None,
                    node.Name,
                    new Uri(node.Url),
                    new Lazy<INode<IWorkItemClassificationNode, int>>(() => parentNode)),
                () => parentNode,
                n => node.Children?.Any() ?? false
                    ? node.Children.Select(s => new Node(s, n)).ToList()
                    : Enumerable.Empty<INode<IWorkItemClassificationNode, int>>())
        {
        }
    }
}