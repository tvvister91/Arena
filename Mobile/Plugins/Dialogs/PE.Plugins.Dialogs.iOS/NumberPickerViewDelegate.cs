using System;
using UIKit;

namespace PE.Plugins.Dialogs.iOS
{
    class NumberPickerViewDelegate:UIPickerViewDelegate
    {
        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return row.ToString("00");
        }
    }
}