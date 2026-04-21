using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Types;
using System;

namespace CodeImp.DoomBuilder.TagExplorer
{
    internal sealed class NodeInfo
    {
        public int Index { get; }
        public int Tag { get; }
        public int PolyobjectNumber { get; }
        public int Action { get; }
        public string DefaultName { get; }
        public NodeInfoType Type { get; }
        public string Comment { get { return GetComment(); } set { SetComment(value); } }

        //constructor
        public NodeInfo(Thing t)
        {
            Type = NodeInfoType.THING;
            Index = t.Index;
            Action = t.Action;
            Tag = t.Tag;
            PolyobjectNumber = (t.Type > 9299 && t.Type < 9304) ? t.AngleDoom : int.MinValue;
            ThingTypeInfo tti = General.Map.Data.GetThingInfoEx(t.Type);
            DefaultName = tti != null ? tti.Title : "Thing";
        }

        public NodeInfo(Sector s, int tagindex)
        {
            Type = NodeInfoType.SECTOR;
            Index = s.Index;
            Action = s.Effect;
            Tag = s.Tags[tagindex];

            if (General.Map.Config.SectorEffects.ContainsKey(Action))
                DefaultName = General.Map.Config.SectorEffects[Action].Title;
            else if (Action > 0)
                DefaultName = General.Map.Config.GetGeneralizedSectorEffectName(Action);
            else
                DefaultName = "Sector";
        }

        public NodeInfo(Linedef l, int tagindex)
        {
            Type = NodeInfoType.LINEDEF;
            Index = l.Index;
            Action = l.Action;
            Tag = l.Tags[tagindex];
            PolyobjectNumber = (l.Action > 0 && l.Action < 9) ? l.Args[0] : int.MinValue;

            if (General.Map.Config.LinedefActions.ContainsKey(Action))
                DefaultName = General.Map.Config.LinedefActions[Action].Title;
            else if (Action > 0 && GameConfiguration.IsGeneralized(Action))
                DefaultName = "Generalized (" + General.Map.Config.GetGeneralizedActionCategory(Action) + ")";
            else
                DefaultName = "Linedef";
        }

        // Copy constructor
        public NodeInfo(NodeInfo other, int neweffect)
        {
            this.Type = other.Type;
            this.Index = other.Index;
            this.Action = neweffect;
            this.Tag = other.Tag;
            this.PolyobjectNumber = other.PolyobjectNumber;

            switch (Type)
            {
                case NodeInfoType.LINEDEF:
                    if (General.Map.Config.LinedefActions.ContainsKey(Action))
                        DefaultName = General.Map.Config.LinedefActions[Action].Title;
                    else if (Action > 0 && GameConfiguration.IsGeneralized(Action))
                        DefaultName = "Generalized (" + General.Map.Config.GetGeneralizedActionCategory(Action) + ")";
                    else
                        DefaultName = "Linedef";
                    break;

                case NodeInfoType.SECTOR:
                    if (General.Map.Config.SectorEffects.ContainsKey(Action))
                        DefaultName = General.Map.Config.SectorEffects[Action].Title;
                    else if (Action > 0)
                        DefaultName = General.Map.Config.GetGeneralizedSectorEffectName(Action);
                    else
                        DefaultName = "Sector";
                    break;

                default: throw new NotImplementedException("Not implemented...");
            }
        }

        //methods
        private UniFields GetFields()
        {
            if (Type == NodeInfoType.THING)
            {
                Thing t = General.Map.Map.GetThingByIndex(Index);
                return t == null ? null : t.Fields;
            }

            if (Type == NodeInfoType.SECTOR)
            {
                Sector s = General.Map.Map.GetSectorByIndex(Index);
                return s == null ? null : s.Fields;
            }

            Linedef l = General.Map.Map.GetLinedefByIndex(Index);
            return l == null ? null : l.Fields;
        }

        //comment
        private void SetComment(string comment)
        {
            UniFields fields = GetFields();

            if (comment.Length == 0)
            {
                if (fields.ContainsKey("comment"))
                {
                    General.Map.UndoRedo.CreateUndo("Remove comment");
                    fields.BeforeFieldsChange();
                    fields.Remove("comment");
                }
                return;
            }

            //create undo stuff
            General.Map.UndoRedo.CreateUndo("Set comment");
            fields.BeforeFieldsChange();

            if (!fields.ContainsKey("comment"))
                fields.Add("comment", new UniValue((int)UniversalType.String, comment));
            else
                fields["comment"].Value = comment;
        }

        private string GetComment()
        {
            UniFields fields = GetFields();
            if (fields == null) return "";
            if (!fields.ContainsKey("comment")) return "";
            return fields["comment"].Value.ToString();
        }

        //naming
        public string GetName(ref string comment, string sortMode)
        {
            if (Type == NodeInfoType.THING)
            {
                Thing t = General.Map.Map.GetThingByIndex(Index);
                if (t == null) return "<invalid thing>";
                return GetThingName(t, ref comment, sortMode);
            }

            if (Type == NodeInfoType.SECTOR)
            {
                Sector s = General.Map.Map.GetSectorByIndex(Index);
                if (s == null) return "<invalid sector>";
                return GetSectorName(s, ref comment, sortMode);
            }

            Linedef l = General.Map.Map.GetLinedefByIndex(Index);
            if (l == null) return "<invalid linedef>";
            return GetLinedefName(l, ref comment, sortMode);
        }

        private string GetThingName(Thing t, ref string comment, string sortmode)
        {
            comment = (TagExplorer.UDMF && t.Fields.ContainsKey("comment")) ? t.Fields["comment"].Value.ToString() : string.Empty;
            return CombineName(comment, sortmode);
        }

        private string GetSectorName(Sector s, ref string comment, string sortmode)
        {
            comment = (TagExplorer.UDMF && s.Fields.ContainsKey("comment")) ? s.Fields["comment"].Value.ToString() : string.Empty;
            return CombineName(comment, sortmode);
        }

        private string GetLinedefName(Linedef l, ref string comment, string sortmode)
        {
            if (PolyobjectNumber != int.MinValue) return CombineName(string.Empty, sortmode);
            comment = (TagExplorer.UDMF && l.Fields.ContainsKey("comment")) ? l.Fields["comment"].Value.ToString() : string.Empty;
            return CombineName(comment, sortmode);
        }

        private string CombineName(string comment, string sortmode)
        {
            string name = !string.IsNullOrEmpty(comment) ? comment : DefaultName;

            switch (sortmode)
            {
                case SortMode.SORT_BY_ACTION: //action name is already shown as category name, so we'll show tag here
                    return (Tag > 0 ? "Tag " + Tag + ": " : "") + name + ", Index " + Index;

                case SortMode.SORT_BY_INDEX:
                    return Index + ": " + name + (Tag > 0 ? ", Tag " + Tag : "") + (Action > 0 ? ", Action " + Action : "");

                case SortMode.SORT_BY_TAG: //tag is already shown as category name, so we'll show action here
                    return (Action > 0 ? "Action " + Action + ": " : "") + name + ", Index " + Index;

                case SortMode.SORT_BY_POLYOBJ_NUMBER:
                    return "PO " + PolyobjectNumber + ": " + DefaultName + ", Index " + Index;

                default:
                    return name;
            }
        }
    }

    internal enum NodeInfoType
    {
        THING,
        SECTOR,
        LINEDEF
    }
}
