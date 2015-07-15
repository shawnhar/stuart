using System;
using System.ComponentModel;
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

            if (e.OldValue != null)
            {
                ((Effect)e.OldValue).PropertyChanged -= self.Effect_PropertyChanged;
            }

            if (e.NewValue != null)
            {
                ((Effect)e.NewValue).PropertyChanged += self.Effect_PropertyChanged;
            }

            self.RecreateWidgets();
        }


        void Effect_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Type")
            {
                RecreateWidgets();
            }
        }


        void RecreateWidgets()
        {
            grid.Children.Clear();

            var effect = CurrentEffect;

            if (effect != null)
            {
                var metadata = EffectMetadata.Get(effect.Type);

                for (int row = 0; row < metadata.Parameters.Count; row++)
                {
                    var parameter = metadata.Parameters[row];

                    var label = new TextBlock { Text = parameter.Name };

                    var widget = CreateParameterWidget(effect, parameter);

                    AddToGrid(label, row, 0);
                    AddToGrid(widget, row, 2);
                }
            }
        }


        static UIElement CreateParameterWidget(Effect effect, EffectParameter parameter)
        {
            if (parameter.Default is float)
                return CreateFloatWidget(effect, parameter);

            if (parameter.Default is bool)
                return CreateBoolWidget(effect, parameter);

            throw new NotImplementedException();
        }


        static UIElement CreateFloatWidget(Effect effect, EffectParameter parameter)
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


        static UIElement CreateBoolWidget(Effect effect, EffectParameter parameter)
        {
            var checkbox = new CheckBox()
            {
                IsChecked = (bool)effect.GetParameter(parameter)
            };

            checkbox.Checked   += (sender, e) => { effect.SetParameter(parameter, true);  };
            checkbox.Unchecked += (sender, e) => { effect.SetParameter(parameter, false); };

            return checkbox;
        }


        void AddToGrid(UIElement element, int row, int column)
        {
            element.SetValue(Grid.RowProperty, row);
            element.SetValue(Grid.ColumnProperty, column);

            grid.Children.Add(element);
        }
    }
}
