

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


using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.BuilderModes.ClassicModes
{
    [EditMode(DisplayName = "Floor Align Mode",
              SwitchAction = "flooralignmode",
              ButtonImage = "FloorAlign.png",
              ButtonOrder = int.MinValue + 310,
              ButtonGroup = "000_editing",
              UseByDefault = true, //mxd
              SupportedMapFormats = new[] { "UniversalMapSetIO" }, //mxd
              Volatile = true)]

    public class FloorAlignMode : FlatAlignMode
    {

        protected override string XScaleName { get { return "xscalefloor"; } }
        protected override string YScaleName { get { return "yscalefloor"; } }
        protected override string XOffsetName { get { return "xpanningfloor"; } }
        protected override string YOffsetName { get { return "ypanningfloor"; } }
        protected override string RotationName { get { return "rotationfloor"; } }
        protected override string UndoDescription { get { return "Floor Alignment"; } }

        // Get the texture data to align
        protected override ImageData GetTexture(Sector editsector)
        {
            return DoomBuilder.General.Map.Data.GetFlatImage(editsector.LongFloorTexture);
        }

        // Mode engages
        public override void OnEngage()
        {
            base.OnEngage();
            DoomBuilder.General.Actions.InvokeAction("builder_viewmodefloors");
        }
    }
}
