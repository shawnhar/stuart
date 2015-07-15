using System;

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
    }
}
