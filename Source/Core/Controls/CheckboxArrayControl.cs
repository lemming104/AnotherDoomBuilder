
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
    public partial class CheckboxArrayControl : UserControl
    {
        // Events
        public event EventHandler OnValueChanged; //mxd

        // Variables

        // Properties
        public List<CheckBox> Checkboxes { get; }
        public int Columns { get; set; }
        public int VerticalSpacing { get; set; } = 1; //mxd

        // Constructor
        public CheckboxArrayControl()
        {
            // Initialize
            InitializeComponent();

            // Setup
            Checkboxes = new List<CheckBox>();
        }

        // This adds a checkbox
        public CheckBox Add(string text, object tag)
        {
            // Make new checkbox
            CheckBox c = new CheckBox();
            c.AutoSize = true;
            //c.FlatStyle = FlatStyle.System;
            c.UseVisualStyleBackColor = true;
            c.Text = text;
            c.Tag = tag;
            c.CheckStateChanged += checkbox_OnCheckStateChanged; //mxd

            // Add to list
            this.Controls.Add(c);
            Checkboxes.Add(c);

            // Return checkbox
            return c;
        }

        //mxd
        public int GetWidth()
        {
            if (Columns < 1 || Checkboxes.Count < 1) return 0;
            int maxwidth = 0;
            foreach (CheckBox cb in Checkboxes)
            {
                if (cb.Width > maxwidth) maxwidth = cb.Width;
            }

            return maxwidth * Columns;
        }

        //mxd
        public int GetHeight()
        {
            if (Columns < 1 || Checkboxes.Count < 1) return 0;
            int col = (int)Math.Ceiling(Checkboxes.Count / (float)Columns);
            return (col * Checkboxes[0].Height) + (col * VerticalSpacing) + VerticalSpacing;
        }

        // This positions the checkboxes
        public void PositionCheckboxes()
        {
            int boxheight = 0;
            int row = 0;
            int col = 0;

            // Checks
            if (Columns < 1 || Checkboxes.Count < 1) return;

            // Calculate column width
            int columnwidth = this.ClientSize.Width / Columns;

            // Check what the biggest checkbox height is
            foreach (CheckBox c in Checkboxes) if (c.Height > boxheight) boxheight = c.Height;

            // Check what the preferred column length is
            int columnlength = 1 + (int)Math.Floor((this.ClientSize.Height - boxheight) / (float)(boxheight + VerticalSpacing));

            // When not all items fit with the preferred column length
            // we have to extend the column length to make it fit
            if ((int)Math.Ceiling(Checkboxes.Count / (float)columnlength) > Columns)
            {
                // Make a column length which works for all items
                columnlength = (int)Math.Ceiling(Checkboxes.Count / (float)Columns);
            }

            // Go for all items
            foreach (CheckBox c in Checkboxes)
            {
                // Position checkbox
                c.Location = new Point(col * columnwidth, (row * boxheight) + ((row - 1) * VerticalSpacing) + VerticalSpacing);

                // Next position
                if (++row == columnlength)
                {
                    row = 0;
                    col++;
                }
            }
        }

        //mxd
        public void Sort()
        {
            Checkboxes.Sort(CheckboxesComparison);
        }

        //mxd
        private static int CheckboxesComparison(CheckBox cb1, CheckBox cb2)
        {
            return String.Compare(cb1.Text, cb2.Text, StringComparison.Ordinal);
        }

        // When layout must change
        private void CheckboxArrayControl_Layout(object sender, LayoutEventArgs e)
        {
            PositionCheckboxes();
        }

        private void CheckboxArrayControl_Paint(object sender, PaintEventArgs e)
        {
            if (this.DesignMode)
            {
                using (Pen p = new Pen(SystemColors.ControlDark, 1) { DashStyle = DashStyle.Dash })
                {
                    e.Graphics.DrawRectangle(p, 0, 0, this.ClientRectangle.Width - 1, this.ClientRectangle.Height - 1);
                }
            }
        }

        //mxd
        private void checkbox_OnCheckStateChanged(object sender, EventArgs eventArgs)
        {
            if (OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
        }
    }
}
