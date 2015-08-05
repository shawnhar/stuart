using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Stuart
{
    // Helper for implementing INotifyPropertyChanged.
    public abstract class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        protected void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, e);
            }
        }


        protected void NotifyPropertyChanged(string propertyName)
        {
            NotifyPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        protected void NotifyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, string propertyName)
        {
            // Unsubscribe property change events of items that were removed from the collection.
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged old in e.OldItems)
                {
                    old.PropertyChanged -= NotifyPropertyChanged;
                }
            }

            // Subscribe to the property change events of newly added items.
            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                {
                    item.PropertyChanged += NotifyPropertyChanged;
                }
            }

            // Also notify listeners that the collection itself has changed.
            NotifyPropertyChanged(propertyName);
        }


        protected void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;

            NotifyPropertyChanged(propertyName);
        }
    }
}
