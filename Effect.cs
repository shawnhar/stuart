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


        public EffectType Type
        {
            get { return type; }
            set { SetField(ref type, value); }
        }

        EffectType type;


        public object this[string parameterName]
        {
            get
            {
                object result;
                return parameters.TryGetValue(parameterName, out result) ? result : null;
            }

            set
            {
                parameters[parameterName] = value;
                NotifyPropertyChanged(parameterName);
            }
        }

        readonly Dictionary<string, object> parameters = new Dictionary<string, object>();


        public Effect(EditGroup parent)
        {
            Parent = parent;
        }


        public void Dispose()
        {
            Parent.Effects.Remove(this);
        }


        public ICanvasImage Apply(ICanvasImage image)
        {
            var metadata = EffectMetadata.Get(type);

            var effect = (ICanvasImage)Activator.CreateInstance(metadata.ImplementationType);

            SetProperty(effect, "Source", image);

            foreach (var parameter in metadata)
            {
                SetProperty(effect, parameter.Name, this[Type.ToString() + '.' + parameter.Name] ?? parameter.Default);
            }

            return effect;
        }


        static void SetProperty(object instance, string propertyName, object value)
        {
            instance.GetType().GetRuntimeProperty(propertyName).SetValue(instance, value);
        }
    }
}
