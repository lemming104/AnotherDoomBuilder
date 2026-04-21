#region ================== Namespaces

using CodeImp.DoomBuilder.Config;
using System;

#endregion

namespace CodeImp.DoomBuilder.Data.Scripting
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class ScriptHandlerAttribute : Attribute
    {
        #region ================== Variables

        #endregion

        #region ================== Properties

        public Type Type { get; set; }
        public ScriptType ScriptType { get; }

        #endregion

        #region ================== Constructor / Destructor

        // Constructor
        public ScriptHandlerAttribute(ScriptType scripttype)
        {
            // Initialize
            this.ScriptType = scripttype;
        }

        #endregion
    }
}
