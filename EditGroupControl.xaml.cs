using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stuart
{
    // UI codebehind for configuring an EditGroup.
    public sealed partial class EditGroupControl : UserControl
    {
        public event Action<object, Effect> EffectSelectionChanged;


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


        void EffectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
