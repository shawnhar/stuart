using System;

namespace Stuart
{
    enum EffectType
    {
        Blur,
        Grayscale,
        Invert,
        Sepia,
        Vignette,
    }


    class PhotoEffect : Observable, IDisposable
    {
        // Fields.
        PhotoEdit parent;


        // Properties.
        EffectType type;

        public EffectType Type
        {
            get { return type; }
            set { SetField(ref type, value); }
        }


        // Methods.
        public PhotoEffect(PhotoEdit parent)
        {
            this.parent = parent;
        }


        public void Dispose()
        {
            parent.Effects.Remove(this);
        }


        public override string ToString()
        {
            return type.ToString();
        }
    }
}
