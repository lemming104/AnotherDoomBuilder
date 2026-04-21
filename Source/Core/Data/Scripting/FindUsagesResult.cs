using System.Text.RegularExpressions;

namespace CodeImp.DoomBuilder.Data.Scripting
{
    public class FindUsagesResult
    {
        public ScriptResource Resource { get; }
        public string Line { get; }
        public int LineIndex { get; }
        public int MatchStart { get; }
        public int MatchEnd { get; }

        private FindUsagesResult() { }
        public FindUsagesResult(ScriptResource source, Match match, string line, int lineindex)
        {
            this.Resource = source;
            this.Line = line;
            this.LineIndex = lineindex;
            this.MatchStart = match.Index;
            this.MatchEnd = match.Index + match.Length;
        }
    }
}
