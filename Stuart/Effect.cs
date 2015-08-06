using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;

namespace Stuart
{
    // DOM type representing a single image processing effect.
    public class Effect : Observable, IDisposable
    {
        public EditGroup Parent { get; private set; }

        readonly Dictionary<string, object> parameters = new Dictionary<string, object>();


        public EffectType Type
        {
            get { return type; }
            set { SetField(ref type, value); }
        }

        EffectType type;


        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetField(ref isEnabled, value); }
        }

        bool isEnabled = true;


        public Effect(EditGroup parent)
        {
            Parent = parent;
        }


        public void Dispose()
        {
            Parent.Effects.Remove(this);
        }


        public object GetParameter(EffectParameter parameter)
        {
            var parameterName = ParameterName(parameter);

            object result;

            return parameters.TryGetValue(parameterName, out result) ? result : parameter.Default;
        }


        public void SetParameter(EffectParameter parameter, object value)
        {
            var parameterName = ParameterName(parameter);

            parameters[parameterName] = value;

            NotifyPropertyChanged(parameterName);
        }


        string ParameterName(EffectParameter parameter)
        {
            return Type.ToString() + '.' + parameter.Name;
        }


        public ICanvasImage Apply(ICanvasImage image, ref Rect? bounds)
        {
            if (!IsEnabled)
                return image;

            var metadata = EffectMetadata.Get(type);

            // Instantiate the effect.
            var effect = (ICanvasImage)Activator.CreateInstance(metadata.ImplementationType);

            // Set the effect input.
            SetProperty(effect, "Source", image);

            // Set configurable parameter values.
            foreach (var parameter in metadata.Parameters)
            {
                var value = GetParameter(parameter);

                SetProperty(effect, parameter.Name, value);

                // Track the image bounds if cropping changes them.
                if (this.Type == EffectType.Crop && parameter.Name == "SourceRectangle")
                {
                    bounds = bounds.HasValue ? RectHelper.Intersect(bounds.Value, (Rect)value) : (Rect)value;
                }
            }

            // Set any constant values.
            foreach (var constant in metadata.Constants)
            {
                SetProperty(effect, constant.Key, constant.Value);
            }

            return effect;
        }


        static void SetProperty(object instance, string propertyName, object value)
        {
            instance.GetType()
                    .GetRuntimeProperty(propertyName)
                    .SetValue(instance, value);
        }


        public void SaveSuspendedState(BinaryWriter writer)
        {
            writer.Write(IsEnabled);
            writer.Write((int)Type);

            writer.WriteCollection(parameters, parameter =>
            {
                writer.Write(parameter.Key);
                writer.Write(parameter.Value.GetType().Name);

                if (parameter.Value is Color)
                {
                    writer.WriteColor((Color)parameter.Value);
                }
                else if (parameter.Value is Rect)
                {
                    writer.WriteRect((Rect)parameter.Value);
                }
                else
                {
                    writer.Write(parameter.Value as dynamic);
                }
            });
        }


        public static Effect RestoreSuspendedState(EditGroup parent, BinaryReader reader)
        {
            var effect = new Effect(parent);

            effect.IsEnabled = reader.ReadBoolean();
            effect.Type = (EffectType)reader.ReadInt32();

            reader.ReadCollection(effect.parameters, () =>
            {
                string key = reader.ReadString();
                object value;

                switch (reader.ReadString())
                {
                    case "Single":
                        value = reader.ReadSingle();
                        break;

                    case "Int32":
                        value = reader.ReadInt32();
                        break;

                    case "Boolean":
                        value = reader.ReadBoolean();
                        break;

                    case "Color":
                        value = reader.ReadColor();
                        break;

                    case "Rect":
                        value = reader.ReadRect();
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return new KeyValuePair<string, object>(key, value);
            });

            return effect;
        }
    }
}
