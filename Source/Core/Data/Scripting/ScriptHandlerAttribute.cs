
using CodeImp.DoomBuilder.Config;
using System;

namespace CodeImp.DoomBuilder.Data.Scripting
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class ScriptHandlerAttribute : Attribute
    {

        private Type type;
        private ScriptType scripttype;

        public Type Type { get { return type; } set { type = value; } }
        public ScriptType ScriptType { get { return scripttype; } }

        // Constructor
        public ScriptHandlerAttribute(ScriptType scripttype)
        {
            // Initialize
            this.scripttype = scripttype;
        }
    }
}
