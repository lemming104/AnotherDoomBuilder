
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

using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Types;
using System;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
    internal enum FieldsEditorRowType //mxd
    {
        // This is a fixed field defined in the game configuration
        // The field cannot be deleted (delete will result in a reset)
        // and cannot change type.
        FIXED,

        // This is an abstartct variable field enetered by user
        // The field can be deleted and can change type.
        DYNAMIC,

        // This is a user variable field defined in actor's DECORATE
        // The field cannot be deleted (delete will result in a reset)
        // but can change type.
        USERVAR,
    }

    internal class FieldsEditorRow : DataGridViewRow
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        //mxd. Row type
        // Field information (only for fixed fields)

        // This is true when the field is defined. Cannot be false when this field
        // is not fixed, because non-fixed fields are deleted from the list when undefined.

        // Type

        #endregion

        #region ================== Properties

        public FieldsEditorRowType RowType { get; } //mxd
        public bool IsDefined { get; private set; }
        public bool IsEmpty { get { return (this.Cells[2].Value == null) || (this.Cells[2].Value.ToString().Length == 0); } }
        public string Name { get { return this.Cells[0].Value.ToString(); } }
        public TypeHandler TypeHandler { get; private set; }
        public UniversalFieldInfo Info { get; }

        #endregion

        #region ================== Constructor

        // Constructor for a fixed, undefined field
        public FieldsEditorRow(DataGridView view, UniversalFieldInfo fixedfield)
        {
            // Undefined
            this.DefaultCellStyle.ForeColor = SystemColors.GrayText;
            IsDefined = false;

            // Fixed
            this.Info = fixedfield;
            this.RowType = FieldsEditorRowType.FIXED; //mxd

            // Type
            this.TypeHandler = General.Types.GetFieldHandler(fixedfield);

            // Make all cells
            base.CreateCells(view);

            // Setup property cell
            this.Cells[0].Value = fixedfield.Name;
            this.Cells[0].ReadOnly = true;

            // Setup type cell
            this.Cells[1].Value = TypeHandler.GetDisplayType();
            this.Cells[1].ReadOnly = true;

            // Setup value cell
            this.Cells[2].Value = TypeHandler.GetStringValue();

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Constructor for a non-fixed, defined field
        //mxd. Also for a user variable field.
        public FieldsEditorRow(DataGridView view, string name, int type, object value, bool isuservar)
        {
            //mxd. Row type
            this.RowType = isuservar ? FieldsEditorRowType.USERVAR : FieldsEditorRowType.DYNAMIC;

            // Type
            this.TypeHandler = General.Types.GetFieldHandler(type, value);

            // Make all cells
            base.CreateCells(view);

            //mxd. Our path splits here...
            if (isuservar)
            {
                // Not defined
                this.DefaultCellStyle.ForeColor = SystemColors.GrayText;
                IsDefined = false;
                //fieldtype.ApplyDefaultValue(); // [ZZ] don't do this. this is only done for int, and not a very good place to do it...

                // We need to remember the default value of the user var
                Info = new UniversalFieldInfo(name, type, value);

                // Setup property cell
                this.Cells[0].Value = name;
                this.Cells[0].ReadOnly = true;

                // Setup type cell
                this.Cells[1].Value = TypeHandler.GetDisplayType();
                this.Cells[1].ReadOnly = true;

                // Setup value cell
                this.Cells[2].Value = TypeHandler.GetStringValue();
            }
            else
            {
                // Defined
                this.DefaultCellStyle.ForeColor = SystemColors.WindowText;
                IsDefined = true;

                // Setup property cell
                this.Cells[0].Value = name;
                this.Cells[0].ReadOnly = true;

                // Setup type cell
                this.Cells[1].Value = TypeHandler.GetDisplayType();
                this.Cells[1].ReadOnly = false;

                // Setup value cell
                this.Cells[2].Value = TypeHandler.GetStringValue();
            }

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        #endregion

        #region ================== Methods

        // Browse for value
        public void Browse(IWin32Window parent)
        {
            if (TypeHandler != null)
            {
                // Browse for value
                TypeHandler.Browse(parent);

                // This is a fixed field?
                if (RowType == FieldsEditorRowType.FIXED)
                {
                    // Does this match the default setting?
                    if (TypeHandler.GetValue().Equals(Info.Default))
                    {
                        // Undefine this field!
                        Undefine();
                    }
                    else
                    {
                        // Define
                        Define(TypeHandler.GetValue());
                    }
                }
                else
                {
                    // Define
                    Define(TypeHandler.GetValue());
                }
            }
        }

        // This is called when a cell is edited
        public void CellChanged()
        {
            // This gdmn grid thing returns the chosen value as string instead
            // of the object type I added to the combobox...
            if (this.Cells[1].Value is string)
            {
                // Find the TypeHandlerAttribute with this name
                TypeHandlerAttribute attrib = General.Types.GetNamedAttribute(this.Cells[1].Value.ToString());

                // Different?
                if (attrib.Index != TypeHandler.Index)
                {
                    // Change field type!
                    this.ChangeType(attrib.Index);
                }
            }

            // Anything in the box?
            if ((this.Cells[2].Value != null) && (this.Cells[2].Value.ToString().Length > 0))
            {
                // Validate value
                TypeHandler.SetValue(this.Cells[2].Value);
                this.Cells[2].Value = TypeHandler.GetStringValue();

                // This is a fixed field?
                if (RowType == FieldsEditorRowType.FIXED)
                {
                    // Does this match the default setting?
                    if (TypeHandler.GetValue().Equals(Info.Default))
                    {
                        // Undefine this field!
                        Undefine();
                    }
                }
            }
        }

        // This undefines the field
        // ONLY VALID FOR FIXED AND USERVAR FIELDS
        // You should just delete non-fixed fields
        public void Undefine()
        {
            // Must be fixed!
            if (RowType != FieldsEditorRowType.FIXED && RowType != FieldsEditorRowType.USERVAR) throw new InvalidOperationException();

            // Now undefined
            if (RowType == FieldsEditorRowType.USERVAR)
                TypeHandler.ApplyDefaultValue();
            else
                TypeHandler.SetValue(Info.Default);

            this.Cells[2].Value = TypeHandler.GetStringValue();
            this.DefaultCellStyle.ForeColor = SystemColors.GrayText;
            IsDefined = false;
        }

        // This defines the field
        public void Define(object value)
        {
            TypeHandler.SetValue(value);
            this.Cells[2].Value = TypeHandler.GetStringValue();

            //mxd. Don't count as defined when default value is passed
            if (value.ToString() == TypeHandler.GetDefaultValue().ToString()) return;

            this.DefaultCellStyle.ForeColor = RowType == FieldsEditorRowType.USERVAR ? SystemColors.HotTrack : SystemColors.WindowText;
            IsDefined = true;
        }

        // This changes the type
        public void ChangeType(int typeindex)
        {
            // Can't do this for a fixed field!
            if (RowType == FieldsEditorRowType.FIXED) throw new InvalidOperationException();

            // Different?
            if (typeindex != TypeHandler.Index)
            {
                // Change field type!
                TypeHandler = General.Types.GetFieldHandler(typeindex, this.Cells[2].Value);
                this.Cells[1].Value = TypeHandler.GetDisplayType();
            }
        }

        // This clears the field
        public void Clear()
        {
            this.Cells[2].Value = "";
        }

        // This returns the result
        public object GetResult(object value)
        {
            // Anything in the box?
            if ((this.Cells[2].Value != null) && (this.Cells[2].Value.ToString().Length > 0))
            {
                // Return validated value
                TypeHandler.SetValue(this.Cells[2].Value);
                return TypeHandler.GetValue();
            }
            else
            {
                // Return old value
                return value;
            }
        }

        #endregion
    }
}
