using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stuart
{
    public sealed partial class PhotoEditControl : UserControl
    {
        public PhotoEditControl()
        {
            this.InitializeComponent();
        }


        void NewEffect_Click(object sender, RoutedEventArgs e)
        {
            var edit = (PhotoEdit)DataContext;

            edit.Effects.Add(new PhotoEffect(edit));
        }


        void EffectList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            e.Data.Properties.Add("DragItems", e.Items.ToArray());
        }
    }
}
