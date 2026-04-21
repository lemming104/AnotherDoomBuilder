#region ================== Namespaces

using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

#endregion

namespace CodeImp.DoomBuilder.Data.Scripting
{
    public sealed class ScriptResource
    {
        #region ================== Variables

        private string resourcedisplayname;
        private DataReader resource;
        private string resourcepath;

        // Special cases...
        private string parentresourcelocation;

        #endregion

        #region ================== Properties

        public string Filename { get; } // Path to text file inside of Resource
        public string FilePathName { get; private set; } // Resource location and file path inside resource combined
        public int LumpIndex { get; } = -1; // Text lump index if Resource is wad, -1 otherwise
        internal DataReader Resource { get { return GetResource(); } }
        public HashSet<string> Entries { get; } // Actors/models/sounds etc.
        public ScriptType ScriptType { get; }
        public bool IsReadOnly { get; }

        #endregion

        #region ================== Constructor

        public ScriptResource(TextResourceData source, ScriptType type)
        {
            resource = source.Source;
            resourcepath = resource.Location.location;
            resourcedisplayname = resource.Location.GetDisplayName();
            Filename = source.Filename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            FilePathName = Path.Combine(resourcepath, Filename);
            Entries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            LumpIndex = source.LumpIndex;
            ScriptType = type;
            IsReadOnly = resource.IsReadOnly;

            // Embedded resources require additional tender loving care...
            if (resource is WADReader)
            {
                WADReader wr = (WADReader)resource;
                if (wr.ParentResource is PK3Reader)
                    parentresourcelocation = wr.ParentResource.Location.location;
            }
        }

        #endregion

        #region ================== Methods

        internal bool ContainsText(FindReplaceOptions options)
        {
            // Get text
            DataReader res = GetResource();
            if (res == null) return false;
            MemoryStream stream = res.LoadFile(Filename, LumpIndex);
            if (stream != null)
            {
                // Add word boundary delimiter?
                string findtext = options.WholeWord ? "\\b" + options.FindText + "\\b" : options.FindText;
                RegexOptions ro = options.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                Regex regex = new Regex(findtext, ro);

                using (StreamReader reader = new StreamReader(stream, ScriptEditorControl.Encoding))
                {
                    while (!reader.EndOfStream)
                    {
                        if (regex.IsMatch(reader.ReadLine())) return true;
                    }
                }
            }

            // No dice...
            return false;
        }

        // Finds text occurencies in the resource. Whole word / ignode case only.
        internal List<FindUsagesResult> FindUsages(FindReplaceOptions options)
        {
            var result = new List<FindUsagesResult>();

            // Get text
            DataReader res = GetResource();
            if (res == null) return result;
            MemoryStream stream = res.LoadFile(Filename, LumpIndex);
            if (stream != null)
            {
                // Add word boundary delimiter
                string findtext = options.WholeWord ? "\\b" + options.FindText + "\\b" : options.FindText;
                Regex regex = new Regex(findtext, options.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);

                using (StreamReader reader = new StreamReader(stream, ScriptEditorControl.Encoding))
                {
                    int lineindex = 0;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        foreach (Match match in regex.Matches(line))
                            result.Add(new FindUsagesResult(this, match, line, lineindex));

                        lineindex++;
                    }
                }
            }

            return result;
        }

        private DataReader GetResource()
        {
            if (resource == null || resource.IsDisposed)
            {
                resource = null;

                // Try to re-aquire resource
                if (!string.IsNullOrEmpty(parentresourcelocation))
                {
                    // Special case: WAD resource inside of PK3 resource.
                    // Resource resourcepath will be different after reloading resources, because it's randomly generated.
                    // So resolve using displayname and parent resource location...
                    foreach (DataReader reader in General.Map.Data.Containers)
                    {
                        // Found parent
                        if (reader.Location.location == parentresourcelocation && reader is PK3Reader)
                        {
                            PK3Reader pr = (PK3Reader)reader;
                            foreach (WADReader wr in pr.Wads)
                            {
                                if (wr.Location.GetDisplayName() == resourcedisplayname)
                                {
                                    // Found it
                                    resource = reader;

                                    // Some paths need updating...
                                    resourcepath = resource.Location.location;
                                    FilePathName = Path.Combine(resourcepath, Filename);

                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (DataReader reader in General.Map.Data.Containers)
                    {
                        if (reader.Location.location == resourcepath)
                        {
                            // Found it
                            resource = reader;
                            break;
                        }
                    }
                }
            }

            return resource;
        }

        // Used as tab and script navigator item title
        public override string ToString()
        {
            return (LumpIndex != -1 ? LumpIndex + ":" : "") + Path.GetFileName(Filename);
        }

        #endregion
    }
}
