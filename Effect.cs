using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Reflection;

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


        public ICanvasImage Apply(ICanvasImage image)
        {
            var metadata = EffectMetadata.Get(type);

            var effect = (ICanvasImage)Activator.CreateInstance(metadata.ImplementationType);

            SetProperty(effect, "Source", image);

            foreach (var parameter in metadata.Parameters)
            {
                SetProperty(effect, parameter.Name, GetParameter(parameter));
            }

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
    }
}
