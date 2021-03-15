using System;
using System.Collections;
using System.Collections.Generic;

namespace vpet
{
    public  class Parameter<T>
    {
        private string name;
        private T value;
        public event EventHandler hasChanged;

        public class changeEventArgs : EventArgs
        {
            public T value;
        }

        public void connectToChangeValue(EventHandler<changeEventArgs> ev)
        {
            ev += changeValue;
        }

        public void removeFromChangeValue(EventHandler<changeEventArgs> ev)
        {
            ev -= changeValue;
        }

        private void changeValue(object sender, changeEventArgs a)
        {
            value = a.value;
            if (hasChanged != null)
                hasChanged(this, null);
        }
    }

}
