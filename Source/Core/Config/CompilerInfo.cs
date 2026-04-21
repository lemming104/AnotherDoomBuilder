
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

using CodeImp.DoomBuilder.Compilers;
using CodeImp.DoomBuilder.IO;
using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.Config
{
    public sealed class CompilerInfo
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        #endregion

        #region ================== Properties

        public string FileName { get; }
        public string Name { get; }
        public string Path { get; }
        public string ProgramFile { get; }
        public string ProgramInterface { get; }
        public HashSet<string> Files { get; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal CompilerInfo(string filename, string name, string path, Configuration cfg)
        {
            General.WriteLogLine("Registered compiler configuration \"" + name + "\" from \"" + filename + "\"");

            // Initialize
            this.FileName = filename;
            this.Path = path;
            this.Name = name;
            this.Files = new HashSet<string>(StringComparer.OrdinalIgnoreCase); //mxd. List -> HashSet

            // Read program file and interface
            this.ProgramFile = cfg.ReadSetting("compilers." + name + ".program", "");
            this.ProgramInterface = cfg.ReadSetting("compilers." + name + ".interface", "");

            // Make list of files required
            IDictionary cfgfiles = cfg.ReadSetting("compilers." + name, new Hashtable());
            foreach (DictionaryEntry de in cfgfiles)
            {
                if (de.Key.ToString() != "interface" && de.Key.ToString() != "program")
                {
                    //mxd
                    string include = de.Value.ToString().Replace('\\', '/');
                    if (Files.Contains(include))
                        General.ErrorLogger.Add(ErrorType.Warning, "Include file \"" + de.Value + "\" is double defined in \"" + name + "\" compiler configuration");
                    else
                        Files.Add(include);
                }
            }
        }

        #endregion

        #region ================== Methods

        // This creates the actual compiler interface
        internal Compiler Create()
        {
            return Compiler.Create(this);
        }

        #endregion
    }
}
