using System;

//mxd. Attribute to store git short hash string
namespace CodeImp.DoomBuilder
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public class AssemblyHashAttribute : Attribute
    {
        public String CommitHash { get; }

        public AssemblyHashAttribute(String commithash)
        {
            this.CommitHash = commithash;
        }
    }
}
