using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Types;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
    public abstract class BaseActionTextures
    {

        private List<int> tags;

        public BaseActionTextures()
        {
            tags = FindTags();
        }

        // Determine if the sidedef's sector tag is for an inspected action and the sidedef will require a texture.
        public bool RequiresTexture(Sidedef side, int tag)
        {
            return tags.Contains(tag) &&
                side.Other != null &&
                side.Other.Sector != side.Sector &&
                !side.Other.Sector.Tags.Contains(tag) &&
                HasAdjustedSector(side);
        }

        // Gather all sector tags for the linedef actions being inspected.
        private List<int> FindTags()
        {
            List<int> actions = FindActions();
            List<int> tags = new List<int>();
            int tag;

            foreach (Linedef ld in DoomBuilder.General.Map.Map.Linedefs)
            {
                if (ld.Action > 0 && actions.Contains(ld.Action))
                {
                    if (DoomBuilder.General.Map.HEXEN || DoomBuilder.General.Map.UDMF)
                        tag = FindArgumentsSectorTag(ld.Action, ld.Args);
                    else
                        tag = ld.Tag;

                    if (tag > 0)
                        tags.Add(tag);
                }
            }

            if (DoomBuilder.General.Map.HEXEN || DoomBuilder.General.Map.UDMF)
            {
                foreach (Thing t in DoomBuilder.General.Map.Map.Things)
                {
                    if (t.Action > 0 && actions.Contains(t.Action))
                    {
                        tag = FindArgumentsSectorTag(t.Action, t.Args);

                        if (tag > 0)
                            tags.Add(tag);
                    }
                }
            }

            return tags;
        }

        // Get the sector tag from linedef/things action arguments.
        private int FindArgumentsSectorTag(int action, int[] args)
        {
            if (DoomBuilder.General.Map.Config.GetLinedefActionInfo(action).Args[0].Type == (int)UniversalType.SectorTag)
                return args[0];

            return 0;
        }

        // Gather the linedef specials from the configuration that this will inspect.
        private List<int> FindActions()
        {
            List<int> actions = new List<int>();

            foreach (LinedefActionInfo info in DoomBuilder.General.Map.Config.LinedefActions.Values)
                if (InspectsAction(info))
                    actions.Add(info.Index);

            return actions;
        }

        // Determine if tagged sectors for the given action will have its textures analyzed.
        protected abstract bool InspectsAction(LinedefActionInfo info);

        // Determine whether an upper or lower texture is required after the sector tag is activated.
        protected abstract bool HasAdjustedSector(Sidedef side);
    }
}
