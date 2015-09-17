namespace FalloutSnip.Domain.Model
{
    using System;

    public class RecordChangeEventArgs : EventArgs
    {
        private readonly BaseRecord record;

        public RecordChangeEventArgs(BaseRecord rec)
        {
            this.record = rec;
        }

        public BaseRecord Record
        {
            get
            {
                return this.record;
            }
        }
    }
}