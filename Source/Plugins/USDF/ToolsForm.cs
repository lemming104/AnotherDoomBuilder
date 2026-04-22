
using CodeImp.DoomBuilder.Windows;
using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.USDF
{
    public partial class ToolsForm : Form
    {

        // Constructor
        public ToolsForm()
        {
            InitializeComponent();

            General.Interface.AddButton(dialogbutton, ToolbarSection.Script);
            General.Interface.AddMenu(dialogitem, MenuSection.ViewScriptEdit);
        }

        // Disposer
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            General.Interface.RemoveButton(dialogbutton);
            General.Interface.RemoveMenu(dialogitem);

            base.Dispose(disposing);
        }

        // This invokes an action from control event
        private void InvokeTaggedAction(object sender, EventArgs e)
        {
            General.Interface.InvokeTaggedAction(sender, e);
        }
    }
}
