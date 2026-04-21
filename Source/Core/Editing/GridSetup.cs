
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

using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Windows;
using System;
using System.IO;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Editing
{
    public class GridSetup : IDisposable
    {
        #region ================== Constants

        private const float DEFAULT_GRID_SIZE = 32f;
        internal const float MINIMUM_GRID_SIZE_UDMF = 0.125f; //mxd
        internal const float MINIMUM_GRID_SIZE = 1.0f; //mxd

        public const int SOURCE_TEXTURES = 0;
        public const int SOURCE_FLATS = 1;
        public const int SOURCE_FILE = 2;

        #endregion

        #region ================== Variables

        // Grid
        private double gridsizefinv;

        // Background

        // Disposing

        #endregion

        #region ================== Properties

        public int GridSize { get; private set; } //mxd
        public double GridSizeF { get; private set; }
        public double GridRotate { get; private set; }
        public double GridOriginX { get; private set; }
        public double GridOriginY { get; private set; }
        internal string BackgroundName { get; private set; } = "";
        internal int BackgroundSource { get; private set; }
        internal ImageData Background { get; private set; } = new UnknownImage();
        internal int BackgroundX { get; private set; }
        internal int BackgroundY { get; private set; }
        internal double BackgroundScaleX { get; private set; }
        internal double BackgroundScaleY { get; private set; }
        internal bool Disposed { get; private set; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal GridSetup()
        {
            // Initialize
            SetGridSize(DEFAULT_GRID_SIZE);
            BackgroundScaleX = 1.0f;
            BackgroundScaleY = 1.0f;
            GridRotate = 0.0f;
            GridOriginX = 0;
            GridOriginY = 0;

            // Register actions
            General.Actions.BindMethods(this);

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Disposer
        public void Dispose()
        {
            if (!Disposed)
            {
                // Dispose image if needed
                if (Background is FileImage) Background.Dispose();

                // Clean up
                Background = null;

                // Unregister actions
                General.Actions.UnbindMethods(this);

                // Done
                Disposed = true;
            }
        }

        #endregion

        #region ================== Methods

        // Write settings to configuration
        internal void WriteToConfig(Configuration cfg, string path)
        {
            // Write settings
            cfg.WriteSetting(path + ".background", BackgroundName);
            cfg.WriteSetting(path + ".backsource", BackgroundSource);
            cfg.WriteSetting(path + ".backoffsetx", BackgroundX);
            cfg.WriteSetting(path + ".backoffsety", BackgroundY);
            cfg.WriteSetting(path + ".backscalex", (int)(BackgroundScaleX * 100.0f));
            cfg.WriteSetting(path + ".backscaley", (int)(BackgroundScaleY * 100.0f));
            cfg.WriteSetting(path + ".gridsize", GridSizeF);
            cfg.WriteSetting(path + ".gridrotate", GridRotate);
            cfg.WriteSetting(path + ".gridoriginx", GridOriginX);
            cfg.WriteSetting(path + ".gridoriginy", GridOriginY);
        }

        // Read settings from configuration
        internal void ReadFromConfig(Configuration cfg, string path)
        {
            // Read settings
            BackgroundName = cfg.ReadSetting(path + ".background", "");
            BackgroundSource = cfg.ReadSetting(path + ".backsource", 0);
            BackgroundX = cfg.ReadSetting(path + ".backoffsetx", 0);
            BackgroundY = cfg.ReadSetting(path + ".backoffsety", 0);
            BackgroundScaleX = cfg.ReadSetting(path + ".backscalex", 100) / 100.0f;
            BackgroundScaleY = cfg.ReadSetting(path + ".backscaley", 100) / 100.0f;
            GridSizeF = cfg.ReadSetting(path + ".gridsize", DEFAULT_GRID_SIZE);
            GridOriginX = cfg.ReadSetting(path + ".gridoriginx", 0);
            GridOriginY = cfg.ReadSetting(path + ".gridoriginy", 0);
            GridRotate = cfg.ReadSetting(path + ".gridrotate", 0.0f);

            // Setup
            SetGridSize(GridSizeF);
            LinkBackground();
        }

        // This sets the grid size
        internal void SetGridSize(double size)
        {
            //mxd. Bad things happen when size <= 0
            size = Math.Max(size, (General.Map != null && General.Map.UDMF) ? MINIMUM_GRID_SIZE_UDMF : MINIMUM_GRID_SIZE);

            // Change grid
            GridSizeF = size;
            GridSize = (int)Math.Max(1, Math.Round(GridSizeF)); //mxd
            gridsizefinv = 1f / GridSizeF;

            // Update in main window
            General.MainWindow.UpdateGrid(GridSizeF);
        }

        // Set the rotation angle of the grid
        public void SetGridRotation(double angle)
        {
            GridRotate = angle;
        }

        // Set the origin of the grid
        public void SetGridOrigin(double x, double y)
        {
            GridOriginX = x;
            GridOriginY = y;
        }

        // This sets the background
        internal void SetBackground(string name, int source)
        {
            // Set background
            if (name == null) name = "";
            this.BackgroundSource = source;
            this.BackgroundName = name;

            // Find this image
            LinkBackground();
        }

        // This sets the background view
        internal void SetBackgroundView(int offsetx, int offsety, float scalex, float scaley)
        {
            // Set background offset
            this.BackgroundX = offsetx;
            this.BackgroundY = offsety;
            this.BackgroundScaleX = scalex;
            this.BackgroundScaleY = scaley;
        }

        // This finds and links the background image
        internal void LinkBackground()
        {
            // Dispose image if needed
            if (Background is FileImage) Background.Dispose();

            // Where to load background from?
            switch (BackgroundSource)
            {
                case SOURCE_TEXTURES:
                    Background = General.Map.Data.GetTextureImage(BackgroundName);
                    break;

                case SOURCE_FLATS:
                    Background = General.Map.Data.GetFlatImage(BackgroundName);
                    break;

                case SOURCE_FILE:
                    Background = new FileImage(Path.GetFileNameWithoutExtension(BackgroundName), BackgroundName, false, 1.0f, 1.0f);
                    break;
            }

            // Make sure it is loaded
            Background.LoadImageNow();
        }

        // This returns the next higher coordinate
        public double GetHigher(double offset)
        {
            return Math.Round((offset + (GridSizeF * 0.5f)) * gridsizefinv) * GridSizeF;
        }

        // This returns the next lower coordinate
        public double GetLower(double offset)
        {
            return Math.Round((offset - (GridSizeF * 0.5f)) * gridsizefinv) * GridSizeF;
        }

        // This snaps to the nearest grid coordinate
        public Vector2D SnappedToGrid(Vector2D v)
        {
            return SnappedToGrid(v, GridSizeF, gridsizefinv, GridRotate, GridOriginX, GridOriginY);
        }

        // This snaps to the nearest grid coordinate
        public static Vector2D SnappedToGrid(Vector2D v, double gridsize, double gridsizeinv, double gridrotate = 0.0f, double gridoriginx = 0, double gridoriginy = 0)
        {
            Vector2D origin = new Vector2D(gridoriginx, gridoriginy);
            bool transformed = Math.Abs(gridrotate) > 1e-4 || gridoriginx != 0 || gridoriginy != 0;
            if (transformed)
            {
                // Grid is transformed, so reverse the transformation first
                v = (v - origin).GetRotated(-gridrotate);
            }

            Vector2D sv = new Vector2D(Math.Round(v.x * gridsizeinv) * gridsize,
                                Math.Round(v.y * gridsizeinv) * gridsize);

            if (transformed)
            {
                // Put back into original frame
                sv = sv.GetRotated(gridrotate) + origin;
            }

            if (sv.x < General.Map.Config.LeftBoundary) sv.x = General.Map.Config.LeftBoundary;
            else if (sv.x > General.Map.Config.RightBoundary) sv.x = General.Map.Config.RightBoundary;

            if (sv.y > General.Map.Config.TopBoundary) sv.y = General.Map.Config.TopBoundary;
            else if (sv.y < General.Map.Config.BottomBoundary) sv.y = General.Map.Config.BottomBoundary;

            return sv;
        }

        #endregion

        #region ================== Actions

        // This shows the grid setup dialog
        internal static void ShowGridSetup()
        {
            // Show preferences dialog
            GridSetupForm gridform = new GridSetupForm();
            if (gridform.ShowDialog(General.MainWindow) == DialogResult.OK)
            {
                // Redraw display
                General.MainWindow.RedrawDisplay();
            }

            // Done
            gridform.Dispose();
        }

        // This changes grid size
        // Note: these were incorrectly swapped before, hence the wrong action name
        [BeginAction("gridinc")]
        internal void DecreaseGrid()
        {
            //mxd. Not lower than 0.125 in UDMF or 1 otherwise
            float preminsize = General.Map.UDMF ? MINIMUM_GRID_SIZE_UDMF * 2 : MINIMUM_GRID_SIZE * 2;
            if (GridSizeF >= preminsize)
            {
                //mxd. Disable automatic grid resizing
                General.MainWindow.DisableDynamicGridResize();

                // Change grid
                SetGridSize(GridSizeF / 2);

                // Redraw display
                General.MainWindow.RedrawDisplay();
            }
        }

        // This changes grid size
        // Note: these were incorrectly swapped before, hence the wrong action name
        [BeginAction("griddec")]
        internal void IncreaseGrid()
        {
            // Not higher than 1024
            if (GridSizeF <= 512)
            {
                //mxd. Disable automatic grid resizing
                General.MainWindow.DisableDynamicGridResize();

                // Change grid
                SetGridSize(GridSizeF * 2);

                // Redraw display
                General.MainWindow.RedrawDisplay();
            }
        }

        #endregion
    }
}
