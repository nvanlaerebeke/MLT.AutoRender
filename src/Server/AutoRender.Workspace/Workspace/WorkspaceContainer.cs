using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AutoRender.Data;

namespace AutoRender.Workspace {

    internal class WorkspaceContainer {
        private readonly ConcurrentDictionary<string, WorkspaceItem> WorkspaceItems;

        public event EventHandler<WorkspaceUpdatedEventArgs> WorkspaceItemUpdated;

        public WorkspaceContainer() {
            WorkspaceItems = new ConcurrentDictionary<string, WorkspaceItem>();
        }

        public void Add(WorkspaceItem pItem) {
            _ = WorkspaceItems.TryAdd(pItem.ID.ToString(), pItem);
            pItem.Updated += Updated;

            WorkspaceItemUpdated?.Invoke(
                this,
                new WorkspaceUpdatedEventArgs(
                    new Data.WorkspaceItem(pItem.ID) {
                        New = pItem.New,
                        Final = pItem.Final,
                        Project = (pItem.Project != null) ? pItem.Project.GetProject() : null
                    },
                    WorkspaceAction.New
                )
            );
        }

        public void Remove(WorkspaceItem pItem) {
            if (WorkspaceItems.TryRemove(pItem.ID.ToString(), out var o)) {
                o.Updated -= Updated;
                WorkspaceItemUpdated?.Invoke(
                    this,
                    new WorkspaceUpdatedEventArgs(
                        new Data.WorkspaceItem(pItem.ID) {
                            New = pItem.New,
                            Final = pItem.Final,
                            Project = (pItem.Project != null) ? pItem.Project.GetProject() : null
                        },
                        WorkspaceAction.Deleted
                    )
                );
            }
        }

        public WorkspaceItem Get(Guid pKey) {
            if (WorkspaceItems.TryGetValue(pKey.ToString(), out var i)) {
                return i;
            }
            return null;
        }

        public List<WorkspaceItem> GetAll() {
            return new List<WorkspaceItem>(WorkspaceItems.Values);
        }

        public void Clear() {
            foreach (var o in WorkspaceItems) {
                Remove(o.Value);
            }
        }

        private void Updated(object sender, WorkspaceItem pItem) {
            WorkspaceItemUpdated?.Invoke(
                this,
                new WorkspaceUpdatedEventArgs(
                    new Data.WorkspaceItem(pItem.ID) {
                        New = pItem.New,
                        Final = pItem.Final,
                        Project = (pItem.Project != null) ? pItem.Project.GetProject() : null
                    },
                    WorkspaceAction.Updated
                )
            );
        }
    }
}