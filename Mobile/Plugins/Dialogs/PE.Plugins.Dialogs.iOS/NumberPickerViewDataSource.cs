using System;
using UIKit;

namespace PE.Plugins.Dialogs.iOS
{
    class NumberPickerViewDataSource : UIPickerViewDataSource
    {
        private int _MaxNumber = 1;

        public NumberPickerViewDataSource(int maxNumber)
        {
            _MaxNumber = maxNumber;
        }

        public override nint GetComponentCount(UIPickerView pickerView) => 1;

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return _MaxNumber;
        }
    }
}