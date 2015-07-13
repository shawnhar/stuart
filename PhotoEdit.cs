using System.ComponentModel;

namespace Stuart
{
    class PhotoEdit : INotifyPropertyChanged
    {
        bool isEnabled = true;

        public bool IsEnabled
        {
            get { return isEnabled; }

            set
            {
                if (value != isEnabled)
                {
                    isEnabled = value;
                    NotifyPropertyChanged("IsEnabled");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
