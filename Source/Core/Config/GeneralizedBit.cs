using System;

namespace CodeImp.DoomBuilder.Config
{
    /// <summary>
    /// Option value in generalized types.
    /// </summary>
    public class GeneralizedBit : INumberedTitle, IComparable<GeneralizedBit>
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Properties
        #endregion

        #region ================== Properties

        public int Index { get; }
        public string Title { get; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal GeneralizedBit(int index, string title)
        {
            // Initialize
            this.Index = index;
            this.Title = title;

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        #endregion

        #region ================== Methods

        // This presents the item as string
        public override string ToString()
        {
            return Title;
        }

        // This compares against another
        public int CompareTo(GeneralizedBit other)
        {
            if (this.Index < other.Index) return -1;
            else if (this.Index > other.Index) return 1;
            else return 0;
        }

        #endregion
    }
}
