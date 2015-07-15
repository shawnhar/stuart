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

                for (int i = 0; i < metadata.Parameters.Count; i++)
                {
                    var parameter = metadata.Parameters[i];

                    string parameterName = effect.Type.ToString() + '.' + parameter.Name;

                    var text = new TextBlock { Text = parameter.Name };
                    AddToGrid(text, i, 0);

                    var slider = new Slider();

                    slider.Value = ((float)(effect[parameterName] ?? parameter.Default) - parameter.Min) / (parameter.Max - parameter.Min) * 100;

                    slider.ValueChanged += (sender, e) =>
                    {
                        effect[parameterName] = parameter.Min + (float)e.NewValue / 100 * (parameter.Max - parameter.Min);
                    };

                    AddToGrid(slider, i, 2);
                }
            }
        }


        void AddToGrid(UIElement element, int row, int column)
        {
            element.SetValue(Grid.RowProperty, row);
            element.SetValue(Grid.ColumnProperty, column);

            grid.Children.Add(element);
        }
    }
}
