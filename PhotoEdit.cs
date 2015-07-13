namespace Stuart
{
    class PhotoEdit : Observable
    {
        Photo parent;


        bool isEnabled = true;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetField(ref isEnabled, value); }
        }


        public PhotoEdit(Photo parent)
        {
            this.parent = parent;
        }


        public void Remove()
        {
            parent.Edits.Remove(this);
        }
    }
}
