using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stuart
{
    // UI codebehind for configuring an EditGroup.
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

            var newEffect = new Effect(edit);

            edit.Effects.Add(newEffect);
            edit.Parent.SelectedEffect = newEffect;
        }
    }
}
