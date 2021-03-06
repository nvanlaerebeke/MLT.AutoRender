﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AutoRender {

    public abstract class BaseViewModel : INotifyPropertyChanged {
        protected TaskFactory uiFactory;

        public event PropertyChangedEventHandler PropertyChanged;

        protected readonly Dispatcher _dispatcher;

        public BaseViewModel() {
            uiFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}