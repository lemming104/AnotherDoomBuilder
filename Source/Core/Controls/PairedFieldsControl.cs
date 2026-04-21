#region ================== Namespaces

using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Properties;
using System;
using System.Globalization;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
    public partial class PairedFieldsControl : UserControl
    {
        #region ================== Events

        public event EventHandler OnValuesChanged;

        #endregion

        #region ================== Variables

        private bool allowValueLinking;
        private bool linkValues;
        private bool blockUpdate;
        private readonly int bResetOffsetX;

        #endregion

        #region ================== Properties

        public bool NonDefaultValue { get; private set; }
        public float DefaultValue { get; set; }
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public bool AllowDecimal { get { return value1.AllowDecimal; } set { value1.AllowDecimal = value; value2.AllowDecimal = value; } }
        public int ButtonStep { get { return value1.ButtonStep; } set { value1.ButtonStep = value; value2.ButtonStep = value; } }
        public float ButtonStepFloat { get { return value1.ButtonStepFloat; } set { value1.ButtonStepFloat = value; value2.ButtonStepFloat = value; } }
        public float ButtonStepBig { get { return value1.ButtonStepBig; } set { value1.ButtonStepBig = value; value2.ButtonStepBig = value; } }
        public float ButtonStepSmall { get { return value1.ButtonStepSmall; } set { value1.ButtonStepSmall = value; value2.ButtonStepSmall = value; } }
        public bool ButtonStepsUseModifierKeys { get { return value1.ButtonStepsUseModifierKeys; } set { value1.ButtonStepsUseModifierKeys = value; value2.ButtonStepsUseModifierKeys = value; } }
        public bool AllowValueLinking { get { return allowValueLinking; } set { allowValueLinking = value; UpdateButtons(); } }
        public bool LinkValues { get { return linkValues; } set { linkValues = value; UpdateButtons(); } }

        #endregion

        #region ================== Constructor

        public PairedFieldsControl()
        {
            InitializeComponent();
            bResetOffsetX = this.Width - bReset.Left;
        }

        #endregion

        #region ================== Methods

        public void SetValuesFrom(UniFields fields, bool first)
        {
            blockUpdate = true;

            string newValue1;
            string newValue2;

            if (AllowDecimal)
            {
                newValue1 = UniFields.GetFloat(fields, Field1, DefaultValue).ToString();
                newValue2 = UniFields.GetFloat(fields, Field2, DefaultValue).ToString();
            }
            else
            {
                newValue1 = Math.Round(UniFields.GetFloat(fields, Field1, DefaultValue)).ToString();
                newValue2 = Math.Round(UniFields.GetFloat(fields, Field2, DefaultValue)).ToString();
            }

            if (first)
            {
                value1.Text = newValue1;
                value2.Text = newValue2;
            }
            else
            {
                if (!string.IsNullOrEmpty(value1.Text)) value1.Text = value1.Text != newValue1 ? string.Empty : newValue1;
                if (!string.IsNullOrEmpty(value2.Text)) value2.Text = value2.Text != newValue2 ? string.Empty : newValue2;
            }
            CheckValues();

            blockUpdate = false;
        }

        public void ApplyTo(UniFields fields, int min, int max, double oldValue1, double oldValue2)
        {
            if (!string.IsNullOrEmpty(value1.Text))
                UniFields.SetFloat(fields, Field1, General.Clamp(value1.GetResultFloat(oldValue1), min, max), DefaultValue);
            else
                UniFields.SetFloat(fields, Field1, oldValue1, DefaultValue);

            if (!string.IsNullOrEmpty(value2.Text))
                UniFields.SetFloat(fields, Field2, General.Clamp(value2.GetResultFloat(oldValue2), min, max), DefaultValue);
            else
                UniFields.SetFloat(fields, Field2, oldValue2, DefaultValue);
        }

        private void CheckValues()
        {
            NonDefaultValue = string.IsNullOrEmpty(value1.Text) || string.IsNullOrEmpty(value2.Text)
                || value1.GetResultFloat(DefaultValue, 0) != DefaultValue || value2.GetResultFloat(DefaultValue, 0) != DefaultValue;
            bReset.Visible = NonDefaultValue;

            if (!blockUpdate && OnValuesChanged != null) OnValuesChanged(this, EventArgs.Empty);
        }

        private void UpdateButtons()
        {
            bLink.Visible = allowValueLinking;

            if (!allowValueLinking)
            {
                bReset.Left = bLink.Left;
            }
            else
            {
                bReset.Left = this.Width - bResetOffsetX;
                bLink.Image = linkValues ? Resources.Link : Resources.Unlink;
            }
        }

        public void ResetIncrementStep()
        {
            value1.ResetIncrementStep();
            value2.ResetIncrementStep();
        }

        #endregion

        #region ================== Events

        private void bLink_Click(object sender, EventArgs e)
        {
            linkValues = !linkValues;
            bLink.Image = linkValues ? Resources.Link : Resources.Unlink;
        }

        private void bReset_Click(object sender, EventArgs e)
        {
            value1.Text = DefaultValue.ToString(CultureInfo.CurrentCulture);
            value2.Text = DefaultValue.ToString(CultureInfo.CurrentCulture);
            CheckValues();
        }

        private void value1_WhenTextChanged(object sender, EventArgs e)
        {
            if (blockUpdate) return;

            if (linkValues)
            {
                blockUpdate = true;
                value2.Text = value1.Text;
                blockUpdate = false;
            }

            CheckValues();
        }

        private void value2_WhenTextChanged(object sender, EventArgs e)
        {
            if (blockUpdate) return;

            if (linkValues)
            {
                blockUpdate = true;
                value1.Text = value2.Text;
                blockUpdate = false;
            }

            CheckValues();
        }

        #endregion
    }
}
