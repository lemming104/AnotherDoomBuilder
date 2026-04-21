
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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
#if !NO_FORMS_DESIGN
    [Designer(typeof(ButtonsNumericTextboxDesigner))]
#endif
    public partial class ButtonsNumericTextbox : UserControl
    {
        #region ================== Events

        public event EventHandler WhenTextChanged;
        public event EventHandler WhenButtonsClicked;
        public event EventHandler WhenEnterPressed;

        #endregion

        #region ================== Variables

        private bool ignorebuttonchange;
        private StepsList steps;
        private bool usemodifierkeys; //mxd

        #endregion

        #region ================== Properties

        public bool AllowDecimal { get { return Textbox.AllowDecimal; } set { Textbox.AllowDecimal = value; UpdateButtonsTooltip(); } }
        public bool AllowNegative { get { return Textbox.AllowNegative; } set { Textbox.AllowNegative = value; } }
        public bool AllowRelative { get { return Textbox.AllowRelative; } set { Textbox.AllowRelative = value; } }
        public bool AllowExpressions { get { return Textbox.AllowExpressions; } set { Textbox.AllowExpressions = value; } } //mxd/mgr_inz_rafal
        public int ButtonStep { get; set; } = 1;
        public float ButtonStepFloat { get; set; } = 1.0f; //mxd. This is used when AllowDecimal is true
        public float ButtonStepBig { get; set; } = 10.0f; //mxd
        public float ButtonStepSmall { get; set; } = 0.1f; //mxd
        override public string Text { get { return Textbox.Text; } set { Textbox.Text = value; } }
        internal NumericTextbox Textbox { get; private set; }
        public StepsList StepValues { get { return steps; } set { steps = value; UpdateButtonsTooltip(); } }
        public bool ButtonStepsWrapAround { get; set; }
        public bool ButtonStepsUseModifierKeys { get { return usemodifierkeys; } set { usemodifierkeys = value; UpdateButtonsTooltip(); } }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        public ButtonsNumericTextbox()
        {
            InitializeComponent();
            buttons.Value = 0;
            Textbox.MouseWheel += textbox_MouseWheel;
            UpdateButtonsTooltip(); //mxd
        }

        #endregion

        #region ================== Interface

        // Client size changes
        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            ClickableNumericTextbox_Resize(this, e);
        }

        // Layout changes
        private void ClickableNumericTextbox_Layout(object sender, LayoutEventArgs e)
        {
            ClickableNumericTextbox_Resize(sender, e);
        }

        // Control resizes
        private void ClickableNumericTextbox_Resize(object sender, EventArgs e)
        {
            buttons.Height = Textbox.Height + 4;
            Textbox.Width = ClientRectangle.Width - buttons.Width - 2;
            buttons.Left = Textbox.Width + 2;
            this.Height = buttons.Height;
        }

        // Text in textbox changes
        private void textbox_TextChanged(object sender, EventArgs e)
        {
            if (WhenTextChanged != null) WhenTextChanged(sender, e);
            buttons.Enabled = !Textbox.CheckIsRelative();
        }

        // Buttons changed
        private void buttons_ValueChanged(object sender, EventArgs e)
        {
            if (!ignorebuttonchange)
            {
                ignorebuttonchange = true;
                if (!Textbox.CheckIsRelative())
                {
                    bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control; //mxd
                    bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift; //mxd

                    if (steps != null && (!usemodifierkeys || (!ctrl && !shift)))
                    {
                        if (buttons.Value < 0)
                            Textbox.Text = steps.GetNextHigherWrap(Textbox.GetResult(0), ButtonStepsWrapAround).ToString(); //mxd
                        else if (buttons.Value > 0)
                            Textbox.Text = steps.GetNextLowerWrap(Textbox.GetResult(0), ButtonStepsWrapAround).ToString(); //mxd
                    }
                    else if (Textbox.AllowDecimal)
                    {
                        double stepsizemod; //mxd
                        if (usemodifierkeys)
                            stepsizemod = ctrl ? ButtonStepSmall : (shift ? ButtonStepBig : ButtonStepFloat);
                        else
                            stepsizemod = ButtonStepFloat;

                        double newvalue = Math.Round(Textbox.GetResultFloat(0.0f) - (buttons.Value * stepsizemod), General.Map.FormatInterface.VertexDecimals);
                        if ((newvalue < 0.0f) && !Textbox.AllowNegative) newvalue = 0.0f;
                        Textbox.Text = newvalue.ToString();
                    }
                    else
                    {
                        int stepsizemod; //mxd
                        if (usemodifierkeys)
                            stepsizemod = ctrl ? (int)ButtonStepSmall : (shift ? (int)ButtonStepBig : ButtonStep);
                        else
                            stepsizemod = ButtonStep;

                        int newvalue = Textbox.GetResult(0) - (buttons.Value * stepsizemod);
                        if ((newvalue < 0) && !Textbox.AllowNegative) newvalue = 0;
                        Textbox.Text = newvalue.ToString();
                    }
                }

                buttons.Value = 0;

                if (WhenButtonsClicked != null)
                    WhenButtonsClicked(this, EventArgs.Empty);

                ignorebuttonchange = false;
            }
        }

        // Mouse wheel used
        private void textbox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (steps != null && (!usemodifierkeys || ((ModifierKeys & Keys.Control) != Keys.Control && (ModifierKeys & Keys.Shift) != Keys.Shift)))
            {
                if (e.Delta > 0)
                    Textbox.Text = steps.GetNextHigher(Textbox.GetResult(0)).ToString();
                else if (e.Delta < 0)
                    Textbox.Text = steps.GetNextLower(Textbox.GetResult(0)).ToString();
            }
            else
            {
                buttons.Value -= Math.Sign(e.Delta);
            }
        }

        // Key pressed in textbox
        private void textbox_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter key?
            if ((e.KeyData == Keys.Enter) && (WhenEnterPressed != null))
                WhenEnterPressed(this, EventArgs.Empty);
        }

        #endregion

        #region ================== Methods

        // This checks if the number is relative
        public bool CheckIsRelative()
        {
            return Textbox.CheckIsRelative();
        }

        // This determines the result value
        public int GetResult(int original)
        {
            return Textbox.GetResult(original);
        }

        //mxd. This determines the result value at given inremental step
        public int GetResult(int original, int step)
        {
            return Textbox.GetResult(original, step);
        }

        // This determines the result value
        public double GetResultFloat(double original)
        {
            return Textbox.GetResultFloat(original);
        }

        //mxd. This determines the result value at given inremental step
        public double GetResultFloat(double original, int step)
        {
            return Textbox.GetResultFloat(original, step);
        }

        //mxd
        public void UpdateButtonsTooltip()
        {
            if (usemodifierkeys)
            {
                string tip = "Hold Ctrl to change value by " + ButtonStepSmall.ToString(CultureInfo.CurrentCulture) + "." + Environment.NewLine +
                             "Hold Shift to change value by " + ButtonStepBig.ToString(CultureInfo.CurrentCulture) + ".";
                tooltip.SetToolTip(buttons, tip);
                Textbox.UpdateTextboxStyle(tip);
            }
            else
            {
                tooltip.RemoveAll();
                Textbox.UpdateTextboxStyle();
            }
        }

        // biwa
        public void ResetIncrementStep()
        {
            Textbox.ResetIncrementStep();
        }

        public void SelectAll()
        {
            Textbox.SelectAll();
        }

        #endregion
    }
}
