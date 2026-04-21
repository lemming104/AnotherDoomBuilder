
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
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

using CodeImp.DoomBuilder.Data;
using System;
using System.IO;
using System.Text;

#endregion

namespace CodeImp.DoomBuilder.IO
{
    public class Lump : IDisposable
    {
        #region ================== Methods

        // Allowed characters in a map lump name
        internal const string MAP_LUMP_NAME_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_";

        #endregion

        #region ================== Variables

        // Owner

        // Data stream

        // Data info

        // Disposing

        #endregion

        #region ================== Properties

        internal WAD Owner { get; private set; }
        internal string Name { get; private set; }
        internal long LongName { get; private set; }
        internal byte[] FixedName { get; private set; }
        internal int Offset { get; }
        internal int Length { get; }
        internal ClippedStream Stream { get; }
        internal bool IsDisposed { get; private set; }


        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal Lump(Stream data, WAD owner, byte[] fixedname, int offset, int length)
        {
            // Initialize
            this.Stream = new ClippedStream(data, offset, length);
            this.Owner = owner;
            this.FixedName = fixedname;
            this.Offset = offset;
            this.Length = length;

            // Make name
            this.Name = MakeNormalName(fixedname, WAD.ENCODING).ToUpperInvariant();
            this.FixedName = MakeFixedName(Name, WAD.ENCODING);
            this.LongName = MakeLongName(Name, false); //mxd

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Disposer
        public void Dispose()
        {
            // Not already disposed?
            if (!IsDisposed)
            {
                // Clean up
                Stream.Dispose();
                Owner = null;

                // Done
                IsDisposed = true;
            }
        }

        #endregion

        #region ================== Methods

        // This returns the long value for a 8 byte texture name
        /*public static unsafe long MakeLongName(string name)
		{
			long value = 0;
			byte[] namebytes = Encoding.ASCII.GetBytes(name.Trim().ToUpper());
			uint bytes = (uint)namebytes.Length;
			if(bytes > 8) bytes = 8;

			fixed(void* bp = namebytes)
			{
				General.CopyMemory(&value, bp, bytes);
			}

			return value;
		}*/

        //mxd. This returns (hopefully) unique hash value for a texture name of any length
        public static long MakeLongName(string name)
        {
            return MakeLongName(name, General.Map != null && General.Map.Config != null && General.Map.Config.UseLongTextureNames);
        }

        //mxd. This returns (hopefully) unique hash value for a texture name of any length
        public static long MakeLongName(string name, bool uselongnames)
        {
            // biwa. is using ToUpper a good idea? Will result in clashes with same names with different cases
            name = name.ToUpper();
            if (!uselongnames && name.Length > DataManager.CLASIC_IMAGE_NAME_LENGTH)
            {
                name = name.Substring(0, DataManager.CLASIC_IMAGE_NAME_LENGTH);
            }
            return MurmurHash2.Hash(name);
        }

        // This makes the normal name from fixed name
        public static string MakeNormalName(byte[] fixedname, Encoding encoding)
        {
            int length = 0;

            // Figure out the length of the lump name
            while ((length < fixedname.Length) && (fixedname[length] != 0)) length++;

            // Make normal name
            return encoding.GetString(fixedname, 0, length).Trim().ToUpper();
        }

        // This makes the fixed name from normal name
        public static byte[] MakeFixedName(string name, Encoding encoding)
        {
            // Make uppercase name and count bytes
            string uppername = name.Trim().ToUpper();
            int bytes = encoding.GetByteCount(uppername);
            if (bytes < 8) bytes = 8;

            // Make 8 bytes, all zeros
            byte[] fixedname = new byte[bytes];

            // Write the name in bytes
            encoding.GetBytes(uppername, 0, uppername.Length, fixedname, 0);

            // Return result
            return fixedname;
        }

        // This copies lump data to another lump
        internal void CopyTo(Lump lump)
        {
            // Create a reader
            BinaryReader reader = new BinaryReader(Stream);

            // Copy bytes over
            Stream.Seek(0, SeekOrigin.Begin);
            lump.Stream.Write(reader.ReadBytes((int)Stream.Length), 0, (int)Stream.Length);
        }

        // String representation
        public override string ToString()
        {
            return Name;
        }

        // This renames the lump
        internal void Rename(string newname)
        {
            // Make name
            this.FixedName = MakeFixedName(newname, WAD.ENCODING);
            this.Name = MakeNormalName(this.FixedName, WAD.ENCODING).ToUpperInvariant();
            this.LongName = MakeLongName(newname);

            // Write changes
            Owner.WriteHeaders();
        }

        // [ZZ] this function is thread safe.
        //      it produces a MemoryStream with copied contents of Stream.
        public Stream GetSafeStream()
        {
            if (Stream == null || Stream.BaseStream == null)
                return null;

            // create new stream. do NOT return the WAD stream. This causes problems with multithreading, and other readers create a MemoryStream.
            byte[] data;
            lock (Stream.BaseStream)
            {
                Stream.Position = 0;
                data = Stream.ReadAllBytes();
            }

            MemoryStream ms = new MemoryStream(data);
            ms.Position = 0;
            return ms;
        }

        #endregion
    }
}
