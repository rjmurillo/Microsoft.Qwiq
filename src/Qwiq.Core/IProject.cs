using System;

namespace Microsoft.Qwiq
{
    public interface IProject : IIdentifiable<Guid>
    {
        INodeCollection<IWorkItemClassificationNode, int> AreaRootNodes { get; }

        Guid Guid { get; }

        INodeCollection<IWorkItemClassificationNode, int> IterationRootNodes { get; }

        string Name { get; }

        Uri Uri { get; }

        IWorkItemTypeCollection WorkItemTypes { get; }

        INodeCollection<IQueryFolder, Guid> QueryFolders { get; }
    }
}