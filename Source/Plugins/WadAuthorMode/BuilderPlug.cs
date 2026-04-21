
#region ================== Copyright (c) 2010 Pascal vd Heiden

/*
 * Copyright (c) 2010 Pascal vd Heiden
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

using CodeImp.DoomBuilder.Plugins;

#endregion

namespace CodeImp.DoomBuilder.WadAuthorMode
{
    //
    // MANDATORY: The plug!
    // This is an important class to the Doom Builder core. Every plugin must
    // have exactly 1 class that inherits from Plug. When the plugin is loaded,
    // this class is instantiated and used to receive events from the core.
    // Make sure the class is public, because only public classes can be seen
    // by the core.
    //

    public class BuilderPlug : Plug
    {
        // Static instance. We can't use a real static class, because BuilderPlug must
        // be instantiated by the core, so we keep a static reference. (this technique
        // should be familiar to object-oriented programmers)
        // Static property to access the BuilderPlug
        public static BuilderPlug Me { get; private set; }

        // This event is called when the plugin is initialized
        public override void OnInitialize()
        {
            base.OnInitialize();

            // Keep a static reference
            Me = this;
        }

        // This is called when the plugin is terminated
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
