using System;
using System.Linq;

namespace Microsoft.Qwiq.Mocks
{
    public class MockProject : Project
    {
        internal const string ProjectName = "Mock Project";

        public MockProject(Guid id, string name, Uri uri, IWorkItemTypeCollection wits, INodeCollection<IWorkItemClassificationNode, int> areas, INodeCollection<IWorkItemClassificationNode, int> iterations)
            : base(
                   id,
                   name,
                   uri,
                   new Lazy<IWorkItemTypeCollection>(() => wits),
                   new Lazy<INodeCollection<IWorkItemClassificationNode, int>>(() => areas),
                   new Lazy<INodeCollection<IWorkItemClassificationNode, int>>(() => iterations),
                   new Lazy<IQueryFolderCollection>(() => new QueryFolderCollection(Enumerable.Empty<IQueryFolder>)))
        {
        }

        public MockProject(IWorkItemStore store, INode<WorkItemClassificationNode, int> node)
            : base(
                   Guid.NewGuid(),
                   node.Value.Name,
                   node.Value.Uri,
                   new Lazy<IWorkItemTypeCollection>(() => new MockWorkItemTypeCollection(store)),
                   new Lazy<INodeCollection<IWorkItemClassificationNode, int>>(() => CreateNodes(true)),
                   new Lazy<INodeCollection<IWorkItemClassificationNode, int>>(() => CreateNodes(false)),
                   new Lazy<IQueryFolderCollection>(() => new QueryFolderCollection(Enumerable.Empty<IQueryFolder>)))
        {
        }

        public MockProject(IWorkItemStore store)
            :this(store, new Node<WorkItemClassificationNode, int>(new WorkItemClassificationNode(1, WorkItemClassificationNodeType.None, ProjectName, new Uri("http://localhost/projects/1"))))
        {
        }

        private static INodeCollection<IWorkItemClassificationNode, int> CreateNodes(bool area)
        {
            var root = 
                new Node<IWorkItemClassificationNode, int>(
                    new WorkItemClassificationNode(1, area ? WorkItemClassificationNodeType.Area : WorkItemClassificationNodeType.None, "Root", new Uri("http://localhost/nodes/1")));
            new Node<IWorkItemClassificationNode, int>(new WorkItemClassificationNode(
                    2,
                    area ? WorkItemClassificationNodeType.Area : WorkItemClassificationNodeType.None,
                    "L1",
                    new Uri("http://localhost/nodes/2")),
                () => root,
                n => new[]
                {
                    new Node<IWorkItemClassificationNode, int>(new WorkItemClassificationNode(
                            3,
                            area ? WorkItemClassificationNodeType.Area : WorkItemClassificationNodeType.None,
                            "L2",
                            new Uri("http://localhost/nodes/3")),
                        () => n,
                        c => Enumerable.Empty<INode<IWorkItemClassificationNode, int>>())
                });

            return new NodeCollection<IWorkItemClassificationNode, int>(new[] {root}.ToList().AsReadOnly());
        }
    }
}