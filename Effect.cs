using System;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace Stuart
{
    public enum EffectType
    {
        Blur,
        Gray,
        Invert,
        Sepia,
        Vignette,
    }


    class Effect : Observable, IDisposable
    {
        // Fields.
        EditGroup parent;


        // Properties.
        EffectType type;

        public EffectType Type
        {
            get { return type; }
            set { SetField(ref type, value); }
        }


        // Methods.
        public Effect(EditGroup parent)
        {
            this.parent = parent;
        }


        public void Dispose()
        {
            parent.Effects.Remove(this);
        }


        public ICanvasImage Apply(ICanvasImage image)
        {
            switch (Type)
            {
                case EffectType.Blur:
                    return new GaussianBlurEffect
                    {
                        Source = image
                    };

                case EffectType.Gray:
                    return new GrayscaleEffect
                    {
                        Source = image
                    };

                case EffectType.Invert:
                    return new InvertEffect
                    {
                        Source = image
                    };

                case EffectType.Sepia:
                    return new SepiaEffect
                    {
                        Source = image
                    };

                case EffectType.Vignette:
                    return new VignetteEffect
                    {
                        Source = image
                    };

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
