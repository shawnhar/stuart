using Microsoft.Graphics.Canvas;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;

namespace Stuart
{
    class Photo : INotifyPropertyChanged
    {
        CanvasDevice device;
        CanvasBitmap sourceBitmap;

        public ObservableCollection<PhotoEdit> Edits { get; } = new ObservableCollection<PhotoEdit>();

        public Vector2 Size => sourceBitmap.Size.ToVector2();

        public event PropertyChangedEventHandler PropertyChanged;


        public Photo(CanvasDevice device)
        {
            this.device = device;

            Edits.CollectionChanged += Edits_CollectionChanged;

            Edits.Add(new PhotoEdit());
        }


        public async Task Load(string filename)
        {
            sourceBitmap = await CanvasBitmap.LoadAsync(device, filename);
        }


        public void Draw(CanvasDrawingSession ds)
        {
            ds.Units = CanvasUnits.Pixels;

            ds.DrawImage(sourceBitmap);
        }


        void Edits_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (PhotoEdit old in e.OldItems)
                {
                    old.PropertyChanged -= NotifyPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (PhotoEdit item in e.NewItems)
                {
                    item.PropertyChanged += NotifyPropertyChanged;
                }
            }

            NotifyPropertyChanged(this, new PropertyChangedEventArgs("Edits"));
        }


        void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, e);
            }
        }
    }
}
