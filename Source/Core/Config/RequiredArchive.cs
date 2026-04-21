using System.Collections.Generic;

namespace CodeImp.DoomBuilder.Config
{
    class RequiredArchiveEntry
    {
        public RequiredArchiveEntry(string reqClass, string reqLump)
        {
            this.Class = reqClass;
            this.Lump = reqLump;
        }

        public string Class { get; }
        public string Lump { get; }
    }

    class RequiredArchive
    {
        private List<RequiredArchiveEntry> entries;

        public RequiredArchive(string id, string filename, bool excludeFromTesting, List<RequiredArchiveEntry> entries)
        {
            this.ID = id;
            this.FileName = filename;
            this.ExcludeFromTesting = excludeFromTesting;
            this.entries = entries;
        }

        public string ID { get; }
        public string FileName { get; }
        public bool ExcludeFromTesting { get; }
        public IReadOnlyCollection<RequiredArchiveEntry> Entries { get { return entries; } }
    }
}
