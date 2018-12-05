using UnityEngine;

namespace JunhPatcher
{
    public sealed class FileHash
    {
        #region PRIVATE FIELDS
        private string mPath;
        private string mHash;
        #endregion PRIVATE FIELDS

        #region PROPERTIES
        public string path { get; set; }
        public string hash { get; set; }
        #endregion PROPERTIES

        #region PUBLIC METHODS
        public FileHash(string path, string hash)
        {
            if(string.IsNullOrEmpty(mPath))
            {
                Debug.LogError("Invalid path.");
                return;
            }

            if(string.IsNullOrEmpty(hash))
            {
                Debug.LogError("Invalid hash");
                return;
            }

            mPath = path;
            mHash = hash;
        }
        #endregion PUBLIC METHODS
    }
}