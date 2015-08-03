using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Windows.UI;
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

            // Unsubscribe events from the previous effect.
            if (e.OldValue != null)
            {
                ((Effect)e.OldValue).PropertyChanged -= self.Effect_PropertyChanged;
            }

            // Listen for property changes on the newly selected effect.
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

                    var label = new TextBlock { Text = FormatParameterName(parameter.Name) };

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

            if (parameter.Default is int)
                return CreateIntWidget(effect, parameter);

            if (parameter.Default is bool)
                return CreateBoolWidget(effect, parameter);

            if (parameter.Default is Color)
                return CreateColorWidget(effect, parameter);

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


        static UIElement CreateIntWidget(Effect effect, EffectParameter parameter)
        {
            var slider = new Slider()
            {
                Minimum = parameter.Min,
                Maximum = parameter.Max,

                Value = (int)effect.GetParameter(parameter)
            };

            slider.ValueChanged += (sender, e) =>
            {
                effect.SetParameter(parameter, (int)e.NewValue);
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


        static UIElement CreateColorWidget(Effect effect, EffectParameter parameter)
        {
            var colorProperties = typeof(Colors).GetRuntimeProperties();
            var colorNames = colorProperties.Select(p => p.Name).ToList();
            var colorValues = colorProperties.Select(p => p.GetValue(null)).ToList();

            var combo = new ComboBox()
            {
                ItemsSource = colorNames,
                SelectedIndex = colorValues.IndexOf(effect.GetParameter(parameter))
            };

            combo.SelectionChanged += (sender, e) =>
            {
                effect.SetParameter(parameter, colorValues[combo.SelectedIndex]);
            };

            return combo;
        }


        void AddToGrid(UIElement element, int row, int column)
        {
            element.SetValue(Grid.RowProperty, row);
            element.SetValue(Grid.ColumnProperty, column);

            grid.Children.Add(element);
        }


        static string FormatParameterName(string name)
        {
            return new Regex("(?<!^)([A-Z])").Replace(name, " $1");
        }
    }
}
