using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stuart
{
    public sealed partial class EffectPropertiesControl : UserControl
    {
        public Effect CurrentEffect
        {
            get { return (Effect)GetValue(CurrentEffectProperty); }
            set { SetValue(CurrentEffectProperty, value); }
        }

        public static readonly DependencyProperty CurrentEffectProperty =
            DependencyProperty.Register(
                "CurrentEffect",
                typeof(Effect),
                typeof(EffectPropertiesControl),
                new PropertyMetadata(null, CurrentEffectChanged));


        public EffectPropertiesControl()
        {
            this.InitializeComponent();
        }


        static void CurrentEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (EffectPropertiesControl)d;

            self.CurrentEffectChanged();
        }


        void CurrentEffectChanged()
        {
            grid.Children.Clear();

            var effect = CurrentEffect;

            if (effect != null)
            {
                var metadata = EffectMetadata.Get(effect.Type);

                for (int i = 0; i < metadata.Count; i++)
                {
                    AddEffectParameter(metadata[i], i);
                }
            }
        }


        void AddEffectParameter(EffectParameter parameter, int row)
        {
            var text = new TextBlock { Text = parameter.Name };
            AddToGrid(text, row, 0);

            var slider = new Slider();
            AddToGrid(slider, row, 2);
        }


        void AddToGrid(UIElement element, int row, int column)
        {
            element.SetValue(Grid.RowProperty, row);
            element.SetValue(Grid.ColumnProperty, column);

            grid.Children.Add(element);
        }
    }
}
