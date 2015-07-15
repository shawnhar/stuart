using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stuart
{
    public sealed partial class EditGroupControl : UserControl
    {
        public static EffectType[] EffectTypes
        {
            get { return Enum.GetValues(typeof(EffectType)).Cast<EffectType>().ToArray(); }
        }


        public EditGroupControl()
        {
            this.InitializeComponent();
        }


        void NewEffect_Click(object sender, RoutedEventArgs e)
        {
            var edit = (EditGroup)DataContext;

            edit.Effects.Add(new Effect(edit));
        }


        void EffectList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            e.Data.Properties.Add("DragItems", e.Items.ToArray());
        }
    }
}
