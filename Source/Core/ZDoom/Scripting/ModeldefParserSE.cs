
#region ================== Namespaces

using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.GZBuilder.Data;
using System.Collections.Generic;

#endregion

//mxd. Modeldef parser used to create ScriptItems for use in script editor's navigator
//Should be parse model definitions even from invalid MODELDEF and should never fail parsing
namespace CodeImp.DoomBuilder.ZDoom.Scripting
{
    internal sealed class ModeldefParserSE : ZDTextParser
    {
        internal override ScriptType ScriptType { get { return ScriptType.MODELDEF; } }

        internal List<ScriptItem> Models { get; }

        public ModeldefParserSE()
        {
            Models = new List<ScriptItem>();
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

            // Continue until at the end of the stream
            while (SkipWhitespace(true))
            {
                string token = ReadToken();
                if (string.IsNullOrEmpty(token) || token.ToUpperInvariant() != "MODEL") continue;

                SkipWhitespace(true);
                int startpos = (int)datastream.Position;
                string modelname = ReadToken();

                SkipWhitespace(true);
                token = ReadToken(); //this should be "{"

                if (token == "{")
                {
                    ScriptItem i = new ScriptItem(modelname, startpos, false);
                    Models.Add(i);
                }

                while (SkipWhitespace(true))
                {
                    token = ReadToken();
                    if (string.IsNullOrEmpty(token) || token == "}") break;
                }
            }

            // Sort nodes
            Models.Sort(ScriptItem.SortByName);
            return true;
        }
    }
}
