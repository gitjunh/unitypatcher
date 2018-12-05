using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace JunhPatcher
{
    public sealed class PatchData
    {
        #region PRIVATE FIELDS
        private string mName;
        private string mURL;
        private int mVersion;
        private uint mCRC;
        private long mFileSize;
        private List<string> mNames;
        private string mPath;
        #endregion PRIVATE FIELDS



        #region PROPERTIES
        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        public string URL
        {
            get { return mURL; }
            set { mURL = value; }
        }

        public int Version
        {
            get { return mVersion; }
            set { mVersion = value; }
        }

        public uint CRC
        {
            get { return mCRC; }
            set { mCRC = value; }
        }

        public long FileSize
        {
            get { return mFileSize; }
            set { mFileSize = value; }
        }

        public List<string> Names
        {
            get { return mNames; }
            set { mNames = value; }
        }

        public string Path
        {
            get { return mPath; }
            set { mPath = value; }
        }
        #endregion PROPERTIES



        #region PUBLIC METHODS
        public PatchData(string name, int version, uint crc, long fileSize, string[] dataNames, string path)
        {
            if(string.IsNullOrEmpty(name))
            {
                Debug.LogError("Invalid data name.");
                return;
            }

            if(fileSize <= default(int))
            {
                Debug.LogError("Invalid file size.");
                return;
            }

            if(dataNames == null)
            {
                Debug.LogError("Invalid datas.");
                return;
            }

            if(dataNames.Length <= default(int))
            {
                Debug.LogError("Invalid data count.");
                return;
            }

            mName = name;
            mURL = string.Empty;
            mVersion = version;
            mCRC = crc;
            mFileSize = fileSize;
            mNames = new List<string>();
            mPath = path;

            foreach(var dataName in dataNames)
            {
                mNames.Add(dataName);
            }
        }

        public PatchData(JsonObject obj)
        {
            if(obj == null)
            {
                Debug.LogError("Invalid object.");
                return;
            }

            var array = obj.GetArrayWithKey("Names");
            if (array == null) return;

            mNames = new List<string>();
            for (var i = default(int); i < array.GetCount(); ++i)
            {
                mNames.Add(array.GetValueWithIndex(i));
            }

            mName = obj.GetValueWithKey("Name");

            var version = -1;
            if (int.TryParse(obj.GetValueWithKey("Version"), out version)) mVersion = version;

            var crc = default(uint);
            if (uint.TryParse(obj.GetValueWithKey("CRC"), out crc)) mCRC = crc;

            var fileSize = default(long);
            if (long.TryParse(obj.GetValueWithKey("Size"), out fileSize)) mFileSize = fileSize;

            Path = obj.GetValueWithKey("Path");
        }

        public void WriteData(JsonWriter writer)
        {
            if(writer == null)
            {
                Debug.LogError("Invalid writer.");
                return;
            }

            writer.WriteObjectStart();

            writer.WritePropertyName("Name");
            writer.Write(Name);

            writer.WritePropertyName("Version");
            writer.Write(Version);

            writer.WritePropertyName("CRC");
            writer.Write(CRC);

            writer.WritePropertyName("Size");
            writer.Write(FileSize);

            writer.WritePropertyName("Path");
            writer.Write(Path);

            writer.WritePropertyName("Names");
            writer.WriteArrayStart();
            foreach(var dataName in Names)
            {
                writer.Write(dataName);
            }
            writer.WriteArrayEnd();

            writer.WriteObjectEnd();
        }
        #endregion PUBLIC METHODS
    }
}