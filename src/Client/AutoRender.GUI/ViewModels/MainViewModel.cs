using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoRender.Data;

namespace AutoRender {

    public class MainViewModel : BaseViewModel {
        private ObservableCollection<WorkspaceItemViewModel> _lstWorkspaceItems = new ObservableCollection<WorkspaceItemViewModel>();

        public MainViewModel() {
        }

        public ObservableCollection<WorkspaceItemViewModel> WorkspaceItems {
            get { return _lstWorkspaceItems; }
            set { _lstWorkspaceItems = value; }
        }

        public ObservableCollection<string> SourceNames {
            get {
                var lstNames = new ObservableCollection<string>();
                WorkspaceItems.ToList().ForEach(i => {
                    if (!string.IsNullOrEmpty(i.SourceName) && !lstNames.Contains(i.SourceName)) {
                        lstNames.Add(i.SourceName);
                    }
                });
                return lstNames;
            }
        }

        public string Status { get; private set; }

        public bool AllSelected {
            get {
                var result = WorkspaceItems.Count > 0 && !WorkspaceItems.Any(i => i.Processable && !i.SelectedForHandling);
                return result;
            }
            set {
                if (value == true) {
                    _lstWorkspaceItems.ToList().ForEach(i => {
                        if (i.Processable) { i.SelectedForHandling = true; }
                    });
                } else {
                    _lstWorkspaceItems.ToList().ForEach(i => {
                        i.SelectedForHandling = false;
                    });
                }
                OnPropertyChanged("AllSelected");
            }
        }

        internal void SetStatus(WindowStatus pStatus, string pMessage) {
            Status = pMessage;
            OnPropertyChanged("Status");
        }

        internal void Update(List<WorkspaceItem> pItems) {
            if (pItems != null) {
                pItems.ForEach((i) => { Update(i); });
            } else {
                Console.WriteLine("pItems is null");
            }
        }

        internal void Update(WorkspaceItem pWorkspaceItem) {
            uiFactory.StartNew(() => {
                lock (WorkspaceItems) {
                    for (var i = 0; i < WorkspaceItems.Count; i++) {
                        if (WorkspaceItems[i].ID.Equals(pWorkspaceItem.ID)) {
                            WorkspaceItems[i].Update(pWorkspaceItem);
                            return;
                        }
                    }
                    WorkspaceItems.Add(new WorkspaceItemViewModel(pWorkspaceItem));
                    OnPropertyChanged("SourceFiles");
                }
            });
        }

        internal void Delete(WorkspaceItem pWorkspaceItem) {
            uiFactory.StartNew(() => {
                lock (WorkspaceItems) {
                    var objToRemove = WorkspaceItems.ToList().Where(i => i.ID.Equals(pWorkspaceItem.ID)).FirstOrDefault();
                    if (objToRemove != null) {
                        WorkspaceItems.Remove(objToRemove);
                        OnPropertyChanged("SourceFiles");
                    }
                }
            });
        }

        internal void Clear() {
            uiFactory.StartNew(() => {
                WorkspaceItems.Clear();
                OnPropertyChanged("SourceFiles");
            });
        }
    }
}