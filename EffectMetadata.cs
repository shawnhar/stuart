using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;

namespace Stuart
{
    public enum EffectType
    {
        Exposure,
        Highlights,
        Temp,
        Contrast,
        Saturation,
        Gray,
        Sepia,
        Vignette,
        Blur,
        Motion,
        Sharpen,
        Edges,
        Emboss,
        Invert,
        Posterize,
        Straighten,
    }


    public class EffectParameter
    {
        public string Name;

        public object Default;

        public float Min;
        public float Max;
    }


    public class EffectMetadata
    {
        public Type ImplementationType;

        public readonly List<EffectParameter> Parameters = new List<EffectParameter>();
        public readonly Dictionary<string, object> Constants = new Dictionary<string, object>();


        public static EffectMetadata Get(EffectType effectType)
        {
            return metadata[effectType];
        }


        readonly static Dictionary<EffectType, EffectMetadata> metadata = new Dictionary<EffectType, EffectMetadata>
        {
            // Exposure metadata.
            {
                EffectType.Exposure, new EffectMetadata
                {
                    ImplementationType = typeof(ExposureEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "Exposure", Default = 0f, Min = -2, Max = 2 }
                    }
                }
            },

            // Highlights metadata.
            {
                EffectType.Highlights, new EffectMetadata
                {
                    ImplementationType = typeof(HighlightsAndShadowsEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "Highlights",     Default = 0f,    Min = -1, Max = 1  },
                        new EffectParameter { Name = "Shadows",        Default = 0f,    Min = -1, Max = 1  },
                        new EffectParameter { Name = "Clarity",        Default = 0f,    Min = -1, Max = 1  },
                        new EffectParameter { Name = "MaskBlurAmount", Default = 0.25f, Min =  0, Max = 10 },
                    }
                }
            },

            // Temperature metadata.
            {
                EffectType.Temp, new EffectMetadata
                {
                    ImplementationType = typeof(TemperatureAndTintEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "Temperature", Default = 0f, Min = -1, Max = 1 },
                        new EffectParameter { Name = "Tint",        Default = 0f, Min = -1, Max = 1 },
                    }
                }
            },

            // Contrast metadata.
            {
                EffectType.Contrast, new EffectMetadata
                {
                    ImplementationType = typeof(ContrastEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "Contrast", Default = 0f, Min = -1, Max = 1 }
                    }
                }
            },

            // Saturation metadata.
            {
                EffectType.Saturation, new EffectMetadata
                {
                    ImplementationType = typeof(SaturationEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "Saturation", Default = 0.5f, Min = 0, Max = 2 }
                    }
                }
            },

            // Grayscale metadata.
            {
                EffectType.Gray, new EffectMetadata
                {
                    ImplementationType = typeof(GrayscaleEffect)
                }
            },

            // Sepia metadata.
            {
                EffectType.Sepia, new EffectMetadata
                {
                    ImplementationType = typeof(SepiaEffect),

                    Parameters =
                    {
                       new EffectParameter { Name = "Intensity", Default = 0.5f, Min = 0, Max = 1 }
                    }
                }
            },

            // Vignette metadata.
            {
                EffectType.Vignette, new EffectMetadata
                {
                    ImplementationType = typeof(VignetteEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "Amount", Default = 0.1f, Min = 0, Max = 1 },
                        new EffectParameter { Name = "Curve",  Default = 0.5f, Min = 0, Max = 1 },
                    }
                }
            },

            // Blur metadata.
            {
                EffectType.Blur, new EffectMetadata
                {
                    ImplementationType = typeof(GaussianBlurEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "BlurAmount", Default = 8f, Min = 0, Max = 100 }
                    },

                    Constants =
                    {
                        { "BorderMode", EffectBorderMode.Hard }
                    }
                }
            },

            // Motion metadata.
            {
                EffectType.Motion, new EffectMetadata
                {
                    ImplementationType = typeof(DirectionalBlurEffect),
                    
                    Parameters =
                    {
                        new EffectParameter { Name = "BlurAmount", Default = 8f, Min = 0, Max = 100            },
                        new EffectParameter { Name = "Angle",      Default = 0f, Min = 0, Max = (float)Math.PI },
                    },

                    Constants =
                    {
                        { "BorderMode", EffectBorderMode.Hard }
                    }
                }
            },

            // Sharpen metadata.
            {
                EffectType.Sharpen, new EffectMetadata
                {
                    ImplementationType = typeof(SharpenEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "Amount",    Default = 0f, Min = 0, Max = 10 },
                        new EffectParameter { Name = "Threshold", Default = 0f, Min = 0, Max = 1  },
                    }
                }
            },

            // Edge detection metadata.
            {
                EffectType.Edges, new EffectMetadata
                {
                    ImplementationType = typeof(EdgeDetectionEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "Amount",       Default = 0.5f, Min = 0.01f, Max = 1 },
                        new EffectParameter { Name = "BlurAmount",   Default = 0f,   Min = 0,     Max = 2 },
                        new EffectParameter { Name = "OverlayEdges", Default = false                      },
                    }
                }
            },

            // Emboss metadata.
            {
                EffectType.Emboss, new EffectMetadata
                {
                    ImplementationType = typeof(EmbossEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "Amount", Default = 1f, Min = 0, Max = 10                 },
                        new EffectParameter { Name = "Angle",  Default = 0f, Min = 0, Max = (float)Math.PI * 2 },
                    }
                }
            },

            // Invert metadata.
            {
                EffectType.Invert, new EffectMetadata
                {
                    ImplementationType = typeof(InvertEffect)
                }
            },

            // Posterize metadata.
            {
                EffectType.Posterize, new EffectMetadata
                {
                    ImplementationType = typeof(PosterizeEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "RedValueCount",   Default = 4, Min = 2, Max = 16 },
                        new EffectParameter { Name = "GreenValueCount", Default = 4, Min = 2, Max = 16 },
                        new EffectParameter { Name = "BlueValueCount",  Default = 4, Min = 2, Max = 16 },
                    }
                }
            },

            // Straighten metadata.
            {
                EffectType.Straighten, new EffectMetadata
                {
                    ImplementationType = typeof(StraightenEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "Angle", Default = 0f, Min = -(float)Math.PI / 16, Max = (float)Math.PI / 16 }
                    },

                    Constants =
                    {
                        { "MaintainSize", true }
                    }
                }
            },
        };
    }
}
