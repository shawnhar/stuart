using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Windows.Foundation;
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

            self.CreateWidgets();
        }


        void Effect_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Type")
            {
                CreateWidgets();
            }
        }


        void CreateWidgets()
        {
            grid.Children.Clear();

            var effect = CurrentEffect;

            if (effect == null)
                return;

            var metadata = EffectMetadata.Get(effect.Type);

            // Create XAML elements and choose their label strings.
            var widgets = new List<UIElement>();
            var widgetNames = new List<string>();

            foreach (var parameter in metadata.Parameters)
            {
                CreateParameterWidgets(effect, parameter, widgets, widgetNames);

                if (widgetNames.Count < widgets.Count)
                {
                    widgetNames.Add(FormatParameterName(parameter.Name));
                }
            }

            // Populate the grid.
            for (int row = 0; row < widgets.Count; row++)
            {
                var label = new TextBlock { Text = widgetNames[row] };

                AddToGrid(label, row, 0);
                AddToGrid(widgets[row], row, 2);
            }
        }


        static void CreateParameterWidgets(Effect effect, EffectParameter parameter, List<UIElement> widgets, List<string> widgetNames)
        {
            if (parameter.Default is float)
            {
                widgets.Add(CreateFloatWidget(effect, parameter));
            }
            else if (parameter.Default is int)
            {
                widgets.Add(CreateIntWidget(effect, parameter));
            }
            else if (parameter.Default is bool)
            {
                widgets.Add(CreateBoolWidget(effect, parameter));
            }
            else if (parameter.Default is Color)
            {
                widgets.Add(CreateColorWidget(effect, parameter));
            }
            else if (parameter.Default is Rect)
            {
                CreateRectWidgets(effect, parameter, widgets, widgetNames);
            }
            else
            {
                throw new NotImplementedException();
            }
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


        static void CreateRectWidgets(Effect effect, EffectParameter parameter, List<UIElement> widgets, List<string> widgetNames)
        {
            Photo photo = effect.Parent.Parent;

            // Read the current rectangle (infinity means not initialized).
            var initialValue = (Rect)effect.GetParameter(parameter);

            if (double.IsInfinity(initialValue.Width))
            {
                initialValue = photo.SourceBitmap.Bounds;
            }

            var topLeft = new Vector2((float)initialValue.Left, (float)initialValue.Top);
            var bottomRight = new Vector2((float)initialValue.Right, (float)initialValue.Bottom);

            // Create four sliders.
            for (int i = 0; i < 4; i++)
            {
                int whichSlider = i;

                var slider = new Slider();

                // Initialize the slider position.
                switch (whichSlider)
                {
                    case 0:
                        slider.Value = topLeft.X * 100 / photo.Size.X;
                        break;

                    case 1:
                        slider.Value = bottomRight.X * 100 / photo.Size.X;
                        break;

                    case 2:
                        slider.Value = topLeft.Y * 100 / photo.Size.Y;
                        break;

                    case 3:
                        slider.Value = bottomRight.Y * 100 / photo.Size.Y;
                        break;
                }

                // Respond to slider changes.
                slider.ValueChanged += (sender, e) =>
                {
                    switch (whichSlider)
                    {
                        case 0:
                            topLeft.X = (float)e.NewValue * photo.Size.X / 100;
                            break;

                        case 1:
                            bottomRight.X = (float)e.NewValue * photo.Size.X / 100;
                            break;

                        case 2:
                            topLeft.Y = (float)e.NewValue * photo.Size.Y / 100;
                            break;

                        case 3:
                            bottomRight.Y = (float)e.NewValue * photo.Size.Y / 100;
                            break;
                    }

                    // Make sure the rectangle never goes zero or negative.
                    var tl = Vector2.Min(topLeft, photo.Size - Vector2.One);
                    var br = Vector2.Max(bottomRight, tl + Vector2.One);

                    effect.SetParameter(parameter, new Rect(tl.ToPoint(), br.ToPoint()));
                };

                widgets.Add(slider);
            }

            widgetNames.AddRange(new string[] { "Left", "Right", "Top", "Bottom" });
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
