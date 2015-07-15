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

                for (int row = 0; row < metadata.Parameters.Count; row++)
                {
                    var parameter = metadata.Parameters[row];

                    var label = new TextBlock { Text = parameter.Name + ':' };

                    var editor = CreateParameterEditor(effect, parameter);

                    AddToGrid(label, row, 0);
                    AddToGrid(editor, row, 2);
                }
            }
        }


        static UIElement CreateParameterEditor(Effect effect, EffectParameter parameter)
        {
            float valueScale = (parameter.Max - parameter.Min) / 100;

            var slider = new Slider()
            {
                Value = ((float)effect.GetParameter(parameter) - parameter.Min) / valueScale
            };

            slider.ValueChanged += (sender, e) =>
            {
                effect.SetParameter(parameter, parameter.Min + (float)e.NewValue * valueScale);
            };

            return slider;
        }


        void AddToGrid(UIElement element, int row, int column)
        {
            element.SetValue(Grid.RowProperty, row);
            element.SetValue(Grid.ColumnProperty, column);

            grid.Children.Add(element);
        }
    }
}
