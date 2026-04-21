using System;

namespace CodeImp.DoomBuilder.ColorPicker.Controls
{
    public class ColorPickerSliderEventArgs : EventArgs
    {
        public int Value { get; }

        public ColorPickerSliderEventArgs(int value)
        {
            this.Value = value;
        }
    }
}
