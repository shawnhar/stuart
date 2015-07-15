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


    // DOM type representing a single image processing effect.
    public class Effect : Observable, IDisposable
    {
        EditGroup parent;


        public EffectType Type
        {
            get { return type; }
            set { SetField(ref type, value); }
        }

        EffectType type;


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
