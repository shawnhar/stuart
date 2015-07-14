using System;
using System.Collections.ObjectModel;

namespace Stuart
{
    class PhotoEdit : Observable, IDisposable
    {
        // Fields.
        Photo parent;


        // Properties.
        public ObservableCollection<PhotoEffect> Effects { get; } = new ObservableCollection<PhotoEffect>();


        bool isEnabled = true;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetField(ref isEnabled, value); }
        }


        // Methods.
        public PhotoEdit(Photo parent)
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
