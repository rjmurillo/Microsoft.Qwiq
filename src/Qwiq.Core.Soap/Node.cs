using System.Linq;
using JetBrains.Annotations;
using Tfs = Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Microsoft.Qwiq.Client.Soap
{
    internal class Node : Node<IWorkItemClassificationNode, int>
    {
        internal Node([NotNull] Tfs.Node node, [CanBeNull] INode<IWorkItemClassificationNode, int> parentNode = null)
            : base(
                new WorkItemClassificationNode(
                    node.Id,
                    node.IsAreaNode ? WorkItemClassificationNodeType.Area : node.IsIterationNode ? WorkItemClassificationNodeType.Iteration : WorkItemClassificationNodeType.None,
                    node.Name,
                    node.Uri,
                    node.Path),
                () => parentNode,
                parent => node.ChildNodes.Cast<Tfs.Node>().Select(item => new Node(item, parent)).ToList())
        {
        }
    }
}