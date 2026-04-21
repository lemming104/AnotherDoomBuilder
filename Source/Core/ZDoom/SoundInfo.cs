#region ================== Namespaces

using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.ZDoom
{
    public class SoundInfo
    {
        #region ================== Enums

        public enum RolloffType
        {
            NONE,
            INVALID,
            CUSTOM,
            LINEAR,
            LOG
        }

        public enum SoundInfoType
        {
            SOUND,
            GROUP_RANDOM,
        }

        #endregion

        #region ================== Variables


        #endregion

        #region ================== Properties

        public string Name { get; }
        public List<SoundInfo> Children { get; }
        public SoundInfoType Type { get; internal set; }

        // Sound settings
        public string LumpName;
        public float Volume;
        public float Attenuation;
        public int MinimumDistance;
        public int MaximumDistance;
        public RolloffType Rolloff;
        public float RolloffFactor;

        #endregion

        #region ================== Constructor

        public SoundInfo(string name)
        {
            this.Name = name;
            Children = new List<SoundInfo>();
            Type = SoundInfoType.SOUND;

            // Set non-existent settings
            Volume = float.MinValue;
            Attenuation = float.MinValue;
            MinimumDistance = int.MinValue;
            MaximumDistance = int.MinValue;
            Rolloff = RolloffType.INVALID;
            RolloffFactor = float.MinValue;
        }

        // Default props constructor
        internal SoundInfo()
        {
            this.Name = "#GLOBAL_PROPERTIES#";
            Children = new List<SoundInfo>();
            Type = SoundInfoType.SOUND;

            // Set non-existent settings
            Volume = 1.0f;
            Attenuation = 1.0f;
            MinimumDistance = 200;
            MaximumDistance = 1200;
            Rolloff = RolloffType.NONE;
            RolloffFactor = 1.0f; // Is this the default value?
        }

        #endregion
    }
}
