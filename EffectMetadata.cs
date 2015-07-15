using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;

namespace Stuart
{
    public enum EffectType
    {
        Vignette,
        Sepia,
        Gray,
        Saturation,
        Exposure,
        Highlights,
        Temperature,
        Contrast,
        Sharpen,
        Edges,
        Emboss,
        Invert,
        Blur,
        Motion,
        Posterize,
        Straighten,
    }


    public class EffectParameter
    {
        public string Name;

        public float Min;
        public float Max;

        public float Default;


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
            // Vignette metadata.
            {
                EffectType.Vignette, new EffectMetadata(typeof(VignetteEffect))
                {
                    new EffectParameter("Amount") { Min = 0, Max = 1, Default = 0.1f },
                    new EffectParameter("Curve")  { Min = 0, Max = 1, Default = 0.5f },
                }
            },

            // Sepia metadata.
            {
                EffectType.Sepia, new EffectMetadata(typeof(SepiaEffect))
                {
                    new EffectParameter("Intensity") { Min = 0, Max = 1, Default = 0.5f }
                }
            },

            // Grayscale metadata.
            {
                EffectType.Gray, new EffectMetadata(typeof(GrayscaleEffect))
            },

            // Saturation metadata.
            {
                EffectType.Saturation, new EffectMetadata(typeof(SaturationEffect))
                {
                    new EffectParameter("Saturation") { Min = 0, Max = 2, Default = 0.5f }
                }
            },

            // Exposure metadata.
            {
                EffectType.Exposure, new EffectMetadata(typeof(ExposureEffect))
                {
                    new EffectParameter("Exposure") { Min = -2, Max = 2, Default = 0 }
                }
            },

            // Highlights metadata.
            {
                EffectType.Highlights, new EffectMetadata(typeof(HighlightsAndShadowsEffect))
                {
                    new EffectParameter("Highlights")     { Min = -1, Max = 1,  Default = 0     },
                    new EffectParameter("Shadows")        { Min = -1, Max = 1,  Default = 0     },
                    new EffectParameter("Clarity")        { Min = -1, Max = 1,  Default = 0     },
                    new EffectParameter("MaskBlurAmount") { Min =  0, Max = 10, Default = 0.25f },
                }
            },

            // Temperature metadata.
            {
                EffectType.Temperature, new EffectMetadata(typeof(TemperatureAndTintEffect))
                {
                    new EffectParameter("Temperature") { Min = -1, Max = 1, Default = 0 },
                    new EffectParameter("Tint")        { Min = -1, Max = 1, Default = 0 },
                }
            },

            // Contrast metadata.
            {
                EffectType.Contrast, new EffectMetadata(typeof(ContrastEffect))
                {
                    new EffectParameter("Contrast") { Min = -1, Max = 1, Default = 0 }
                }
            },

            // Sharpen metadata.
            {
                EffectType.Sharpen, new EffectMetadata(typeof(SharpenEffect))
                {
                    new EffectParameter("Amount")    { Min = 0, Max = 10, Default = 0 },
                    new EffectParameter("Threshold") { Min = 0, Max = 2,  Default = 0 },
                }
            },

            // Edges metadata.
            {
                EffectType.Edges, new EffectMetadata(typeof(EdgeDetectionEffect))
                {
                    new EffectParameter("Amount")     { Min = 0, Max = 1, Default = 0.5f },
                    new EffectParameter("BlurAmount") { Min = 0, Max = 2, Default = 0    },
                }
            },

            // Emboss metadata.
            {
                EffectType.Emboss, new EffectMetadata(typeof(EmbossEffect))
                {
                    new EffectParameter("Amount") { Min = 0, Max = 10,                 Default = 1 },
                    new EffectParameter("Angle")  { Min = 0, Max = (float)Math.PI * 2, Default = 0 },
                }
            },

            // Invert metadata.
            {
                EffectType.Invert, new EffectMetadata(typeof(InvertEffect))
            },

            // Blur metadata.
            {
                EffectType.Blur, new EffectMetadata(typeof(GaussianBlurEffect))
                {
                    new EffectParameter("BlurAmount") { Min = 0, Max = 100, Default = 8 }
                }
            },

            // Motion metadata.
            {
                EffectType.Motion, new EffectMetadata(typeof(DirectionalBlurEffect))
                {
                    new EffectParameter("BlurAmount") { Min = 0, Max = 100,            Default = 8 },
                    new EffectParameter("Angle")      { Min = 0, Max = (float)Math.PI, Default = 0 },
                }
            },

            // Posterize metadata.
            {
                EffectType.Posterize, new EffectMetadata(typeof(PosterizeEffect))
            },

            // Straighten metadata.
            {
                EffectType.Straighten, new EffectMetadata(typeof(StraightenEffect))
                {
                    new EffectParameter("Angle") { Min = -(float)Math.PI / 16, Max = (float)Math.PI / 16, Default = 0 }
                }
            },
        };
    }
}
