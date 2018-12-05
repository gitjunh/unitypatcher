using System.Collections.Generic;
using UnityEngine;

namespace JunhPatcher
{
    public enum FileState : byte
    {
        NotChanged = byte.MinValue,
        Changed,
    }

    public sealed class HashInfo
    {
        #region PRIVATE FIELDS
        private FileState mState;
        private string mPath;
        private Dictionary<string, FileHash> mFiles;
        #endregion PRIVATE FIELDS

        #region STATIC FIELDS
        public static string NOT_CHANGED = "0";
        public static string CHANGED = "";
        #endregion STATIC FIELDS

        #region PROPERTIES
        public FileState state { get; set; }
        public string path { get; set; }
        public Dictionary<string, FileHash> files { get; set; }
        #endregion PROPERTIES

        #region PUBLIC METHODS
        public HashInfo(string state, string path, Dictionary<string, FileHash> files)
        {
            if(string.IsNullOrEmpty(state))
            {
                Debug.LogError("Invalid state.");
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Invalid path.");
                return;
            }

            if(files == null)
            {
                Debug.LogError("Invalid files");
                return;
            }

            mState = state.Equals(NOT_CHANGED) ? FileState.NotChanged : FileState.Changed;
            mPath = path;
            mFiles = files;
        }
        #endregion PUBLIC METHODS
    }
}