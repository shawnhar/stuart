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
    }
}
