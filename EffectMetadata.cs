﻿using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;

namespace Stuart
{
    public enum EffectType
    {
        Exposure,
        Highlights,
        Temperature,
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

        public float Min;
        public float Max;

        public float Default;
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
                        new EffectParameter { Name = "Exposure", Min = -2, Max = 2, Default = 0 }
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
                        new EffectParameter { Name = "Highlights",     Min = -1, Max = 1,  Default = 0     },
                        new EffectParameter { Name = "Shadows",        Min = -1, Max = 1,  Default = 0     },
                        new EffectParameter { Name = "Clarity",        Min = -1, Max = 1,  Default = 0     },
                        new EffectParameter { Name = "MaskBlurAmount", Min =  0, Max = 10, Default = 0.25f },
                    }
                }
            },

            // Temperature metadata.
            {
                EffectType.Temperature, new EffectMetadata
                {
                    ImplementationType = typeof(TemperatureAndTintEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "Temperature", Min = -1, Max = 1, Default = 0 },
                        new EffectParameter { Name = "Tint",        Min = -1, Max = 1, Default = 0 },
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
                        new EffectParameter { Name = "Contrast", Min = -1, Max = 1, Default = 0 }
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
                        new EffectParameter { Name = "Saturation", Min = 0, Max = 2, Default = 0.5f }
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
                       new EffectParameter { Name = "Intensity", Min = 0, Max = 1, Default = 0.5f }
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
                        new EffectParameter { Name = "Amount", Min = 0, Max = 1, Default = 0.1f },
                        new EffectParameter { Name = "Curve",  Min = 0, Max = 1, Default = 0.5f },
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
                        new EffectParameter { Name = "BlurAmount", Min = 0, Max = 100, Default = 8 }
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
                        new EffectParameter { Name = "BlurAmount", Min = 0, Max = 100,            Default = 8 },
                        new EffectParameter { Name = "Angle",      Min = 0, Max = (float)Math.PI, Default = 0 },
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
                        new EffectParameter { Name = "Amount",    Min = 0, Max = 10, Default = 0 },
                        new EffectParameter { Name = "Threshold", Min = 0, Max = 1,  Default = 0 },
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
                        new EffectParameter { Name = "Amount",     Min = 0, Max = 1, Default = 0.5f },
                        new EffectParameter { Name = "BlurAmount", Min = 0, Max = 2, Default = 0    },
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
                        new EffectParameter { Name = "Amount", Min = 0, Max = 10,                 Default = 1 },
                        new EffectParameter { Name = "Angle",  Min = 0, Max = (float)Math.PI * 2, Default = 0 },
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
                    ImplementationType = typeof(PosterizeEffect)
                }
            },

            // Straighten metadata.
            {
                EffectType.Straighten, new EffectMetadata
                {
                    ImplementationType = typeof(StraightenEffect),

                    Parameters =
                    {
                        new EffectParameter { Name = "Angle", Min = -(float)Math.PI / 16, Max = (float)Math.PI / 16, Default = 0 }
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
