namespace Stuart
{
    class PhotoEdit : Observable
    {
        bool isEnabled = true;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetField(ref isEnabled, value); }
        }
    }
}
