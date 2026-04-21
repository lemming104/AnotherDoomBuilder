#region ================== Copyright (c) 2022 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

#region ================== Namespaces

using Jint;
using Jint.Runtime.Interop;
using System;
using System.Dynamic;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
    internal class UDBWrapper
    {
        #region ================== Variables


        // Version 5
        private IProgress<int> progress;
        private IProgress<string> status;
        private IProgress<string> logger;

        #endregion

        #region ================== Properties

        /// <summary>
        /// Class containing methods related to the game configuration. See [GameConfiguration](GameConfiguration.md) for more information.
        /// </summary>
        public GameConfigurationWrapper GameConfiguration { get; }

        /// <summary>
        /// Class containing methods and properties related to querying options from the user at runtime. See [QueryOptions](QueryOptions.md) for more information.
        /// </summary>
        public TypeReference QueryOptions { get; }

        /// <summary>
        /// Object containing the script options. See [Setting script options](gettingstarted.md#setting-script-options).
        /// </summary>
        public ExpandoObject ScriptOptions { get; }

        /// <summary>
        /// Class containing methods related to angles. See [Angle2D](Angle2D.md) for more information.
        /// ```js
        /// let rad = UDB.Angle2D.degToRad(46);
        /// ```
        /// </summary>
        public TypeReference Angle2D { get; }

        /// <summary>
        /// Class containing methods related to the game data. See [Data](Data.md) for more information.
        /// ```js
        /// let hasfireblu = UDB.Data.textureExists('FIREBLU1');
        /// ```
        /// </summary>
        public DataWrapper Data { get; }

        /// <summary>
        /// Instantiable class that contains methods related to two-dimensional lines. See [Line2D](Line2D.md) for more information.
        /// ```js
        /// let line = new UDB.Line2D([ 32, 64 ], [ 96, 128 ]);
        /// ```
        /// </summary>
        public TypeReference Line2D { get; }

        /// <summary>
        /// Object containing methods related to the map. See [Map](Map.md) for more information.
        /// ```js
        /// let sectors = UDB.Map.getSelectedOrHighlightedSectors();
        /// ```
        /// </summary>
        public MapWrapper Map { get; }

        /// <summary>
        /// The `UniValue` class. Is only needed when trying to assign integer values to UDMF fields.
        /// ```js
        /// s.fields.user_myintfield = new UDB.UniValue(0, 25);
        /// ```
        /// </summary>
        public TypeReference UniValue { get; }

        /// <summary>
        /// Instantiable class that contains methods related to two-dimensional vectors. See [Vector2D](Vector2D.md) for more information.
        /// ```js
        /// let v = new UDB.Vector2D(32, 64);
        /// ```
        /// </summary>
        public TypeReference Vector2D { get; }

        /// <summary>
        /// Instantiable class that contains methods related to three-dimensional vectors. See [Vector3D](Vector3D.md) for more information.
        /// ```js
        /// let v = new UDB.Vector3D(32, 64, 128);
        /// ```
        /// </summary>
        public TypeReference Vector3D { get; }

        public TypeReference Linedef { get; }
        public TypeReference Sector { get; }
        public TypeReference Sidedef { get; }
        public TypeReference Thing { get; }
        public TypeReference Vertex { get; }

        /// <summary>
        /// Instantiable class that contains methods related to a three-dimensional Plane. See [Plane](Plane.md) for more information.
        /// </summary>
        [UDBScriptSettings(MinVersion = 5)]
        public TypeReference Plane { get; }

        /// <summary>
        /// Instantiable class that contains methods related to blockmaps. See [BlockMap][BlockMap.md) for more information.
        /// </summary>
        [UDBScriptSettings(MinVersion = 5)]
        public TypeReference BlockMap { get; }

        #endregion

        #region ================== Constructors

        internal UDBWrapper(Engine engine, ScriptInfo scriptinfo, IProgress<int> progress, IProgress<string> status, IProgress<string> logger)
        {
            GameConfiguration = new GameConfigurationWrapper();
            QueryOptions = TypeReference.CreateTypeReference(engine, typeof(QueryOptions));
            ScriptOptions = scriptinfo.GetScriptOptionsObject();

            Angle2D = TypeReference.CreateTypeReference(engine, typeof(Angle2DWrapper));
            Data = new DataWrapper();
            Line2D = TypeReference.CreateTypeReference(engine, typeof(Line2DWrapper));
            Map = new MapWrapper();
            UniValue = TypeReference.CreateTypeReference(engine, typeof(CodeImp.DoomBuilder.Map.UniValue));
            Vector2D = TypeReference.CreateTypeReference(engine, typeof(Vector2DWrapper));
            Vector3D = TypeReference.CreateTypeReference(engine, typeof(Vector3DWrapper));

            // These can not be directly instanciated and don't have static method, but it's required to
            // for example use "instanceof" in scripts
            Linedef = TypeReference.CreateTypeReference(engine, typeof(LinedefWrapper));
            Sector = TypeReference.CreateTypeReference(engine, typeof(SectorWrapper));
            Sidedef = TypeReference.CreateTypeReference(engine, typeof(SidedefWrapper));
            Thing = TypeReference.CreateTypeReference(engine, typeof(ThingWrapper));
            Vertex = TypeReference.CreateTypeReference(engine, typeof(VertexWrapper));

            // Version 5
            Plane = TypeReference.CreateTypeReference(engine, typeof(PlaneWrapper));
            BlockMap = TypeReference.CreateTypeReference(engine, typeof(BlockMapWrapper));

            this.progress = progress;
            this.status = status;
            this.logger = logger;
        }

        #endregion

        #region ================== Methods

        /// <summary>
        /// Set the progress of the script in percent. Value can be between 0 and 100. Also shows the script running dialog.
        /// </summary>
        /// <param name="value">Number between 0 and 100</param>
        public void setProgress(int value)
        {
            progress.Report(value);
        }

        /*
		public void setStatus(string text)
		{
			status.Report(text);
		}
		*/

        /// <summary>
        /// Adds a line to the script log. Also shows the script running dialog.
        /// </summary>
        /// <param name="text">Line to add to the script log</param>
        public void log(object text)
        {
            if (text == null)
                return;

            logger.Report(text.ToString());
        }

        /// <summary>
        /// Shows a message box with an "OK" button.
        /// </summary>
        /// <param name="message">Message to show</param>
        public void showMessage(object message)
        {
            BuilderPlug.Me.ScriptRunnerForm.InvokePaused(new Action(() =>
            {
                if (message == null)
                    message = string.Empty;

                MessageForm mf = new MessageForm("OK", null, message.ToString());
                DialogResult result = mf.ShowDialog();

                if (result == DialogResult.Abort)
                    throw new UserScriptAbortException();
            }));
        }

        /// <summary>
        /// Shows a message box with an "Yes" and "No" button.
        /// </summary>
        /// <param name="message">Message to show</param>
        /// <returns>true if "Yes" was clicked, false if "No" was clicked</returns>
        public bool showMessageYesNo(object message)
        {
            return (bool)BuilderPlug.Me.ScriptRunnerForm.InvokePaused(new Func<bool>(() =>
            {
                if (message == null)
                    message = string.Empty;

                MessageForm mf = new MessageForm("Yes", "No", message.ToString());
                DialogResult result = mf.ShowDialog();

                if (result == DialogResult.Abort)
                    throw new UserScriptAbortException();

                return result == DialogResult.OK;
            }));
        }

        /// <summary>
        /// Exist the script prematurely without undoing its changes.
        /// </summary>
        /// <param name="s">Text to show in the status bar (optional)</param>
        public void exit(string s = null)
        {
            if (string.IsNullOrEmpty(s))
                throw new ExitScriptException();

            throw new ExitScriptException(s);
        }

        /// <summary>
        /// Exist the script prematurely with undoing its changes.
        /// </summary>
        /// <param name="s">Text to show in the status bar (optional)</param>
        public void die(string s = null)
        {
            if (string.IsNullOrEmpty(s))
                throw new DieScriptException();

            throw new DieScriptException(s);
        }

        #endregion
    }
}
