using System;
using System.Collections.ObjectModel;

namespace Stuart
{
    class EditGroup : Observable, IDisposable
    {
        // Fields.
        Photo parent;


        // Properties.
        public ObservableCollection<Effect> Effects { get; } = new ObservableCollection<Effect>();


        bool isEnabled = true;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetField(ref isEnabled, value); }
        }


        // Methods.
        public EditGroup(Photo parent)
        {
            this.parent = parent;

            Effects.CollectionChanged += (sender, e) => NotifyCollectionChanged(sender, e, "Effects");
        }


        public void Dispose()
        {
            parent.Edits.Remove(this);
        }
    }
}
