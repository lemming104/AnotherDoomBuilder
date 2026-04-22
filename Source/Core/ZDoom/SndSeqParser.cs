
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.ZDoom
{
    internal sealed class SndSeqParser : ZDTextParser
    {

        private readonly List<string> sequences;
        private readonly List<string> sequencegroups;
        private readonly HashSet<string> seqencenames;

        internal override ScriptType ScriptType { get { return ScriptType.SNDSEQ; } }

        public SndSeqParser()
        {
            specialtokens = "";
            sequences = new List<string>();
            sequencegroups = new List<string>();
            seqencenames = new HashSet<string>();
        }

        public override bool Parse(TextResourceData data, bool clearerrors)
        {
            //mxd. Already parsed?
            if (!base.AddTextResource(data))
            {
                if (clearerrors) ClearError();
                return true;
            }

            // Cannot process?
            if (!base.Parse(data, clearerrors)) return false;

            char[] dots = { ':' };
            char[] brace = { '[' };

            // Continue until at the end of the stream
            while (SkipWhitespace(true))
            {
                string token = ReadToken();

                if (!string.IsNullOrEmpty(token))
                {
                    // Sound sequence definition
                    if (token.StartsWith(":"))
                    {
                        string val = token.TrimStart(dots);
                        if (!string.IsNullOrEmpty(val) && !seqencenames.Contains(val.ToUpper()))
                        {
                            sequences.Add(val);
                            seqencenames.Add(val.ToUpper());
                        }
                    }
                    // Group definition
                    else if (token.StartsWith("["))
                    {
                        string val = token.TrimStart(brace);
                        if (!string.IsNullOrEmpty(val) && !seqencenames.Contains(val.ToUpper()))
                        {
                            sequencegroups.Add(val);
                            seqencenames.Add(val.ToUpper());
                        }
                    }
                }
            }

            return true;
        }

        internal string[] GetSoundSequences()
        {
            List<string> result = new List<string>(sequencegroups.Count + sequences.Count);

            // Add to the collection
            sequencegroups.Sort();
            result.AddRange(sequencegroups);

            sequences.Sort();
            result.AddRange(sequences);

            // Return the collection
            return result.ToArray();
        }
    }
}