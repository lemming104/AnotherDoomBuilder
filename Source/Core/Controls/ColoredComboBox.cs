using System.Drawing;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Controls
{
    public class ColoredComboBox : ComboBox
    {
        public ColoredComboBox()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.KeyPress += ColoredComboBox_KeyPress;
        }

        private void ColoredComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                e.Handled = true;
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);
            e.DrawBackground();
            ColoredComboBoxItem item = (ColoredComboBoxItem)this.Items[e.Index];
            using (Brush brush = new SolidBrush(((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Color.White : item.ForeColor))
            {
                e.Graphics.DrawString(item.Text, this.Font, brush, e.Bounds.X, e.Bounds.Y);
            }
        }
    }

    public class ColoredComboBoxItem
    {
        public string Text { get; set; } = "";
        public object Value { get; set; }
        public Color ForeColor { get; set; } = Color.Black;

        public ColoredComboBoxItem() { }

        public ColoredComboBoxItem(object value)
        {
            this.Text = value.ToString();
            this.Value = value;
        }

        public ColoredComboBoxItem(object value, Color forecolor)
        {
            this.Text = value.ToString();
            this.Value = value;
            this.ForeColor = forecolor;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
