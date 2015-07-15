using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;

namespace Stuart
{
    public enum EffectType
    {
        Sepia,
        Gray,
        Invert,
        Vignette,
        Blur,
    }


    public class EffectParameter
    {
        public string Name;

        public float Min;
        public float Max;


        public EffectParameter(string name)
        {
            Name = name;
        }
    }


    public class EffectMetadata : List<EffectParameter>
    {
        public Type ImplementationType;


        EffectMetadata(Type implementationType)
        {
            ImplementationType = implementationType;
        }


        public static EffectMetadata Get(EffectType effectType)
        {
            return metadata[effectType];
        }


        readonly static Dictionary<EffectType, EffectMetadata> metadata = new Dictionary<EffectType, EffectMetadata>
        {
            // Sepia metadata.
            {
                EffectType.Sepia, new EffectMetadata(typeof(SepiaEffect))
                {
                    new EffectParameter("Intensity") { Min = 0, Max = 1 }
                }
            },

            // Gray metadata.
            {
                EffectType.Gray, new EffectMetadata(typeof(GrayscaleEffect))
            },

            // Invert metadata.
            {
                EffectType.Invert, new EffectMetadata(typeof(InvertEffect))
            },

            // Vignette metadata.
            {
                EffectType.Vignette, new EffectMetadata(typeof(VignetteEffect))
                {
                    new EffectParameter("Amount") { Min = 0, Max = 1 },
                    new EffectParameter("Curve")  { Min = 0, Max = 1 }
                }
            },

            // Blur metadata.
            {
                EffectType.Blur, new EffectMetadata(typeof(GaussianBlurEffect))
                {
                    new EffectParameter("BlurAmount") { Min = 0, Max = 250 }
                }
            },
        };
    }
}
