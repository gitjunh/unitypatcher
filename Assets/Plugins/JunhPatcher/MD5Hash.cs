using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace JunhPatcher
{
    public sealed class MD5Hash
    {
        #region INSTANCE
        private static MD5Hash mInstance;
        public static MD5Hash instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new MD5Hash();
                }

                return mInstance;
            }
        }
        #endregion INSTANCE



        #region PRIVATE FIELDS
        private MD5 mHasher;
        private readonly string TO_HEXA = "x2";
        #endregion PRIVATE FIELDS



        #region PRIVATE METHODS
        private MD5Hash()
        {
            mHasher = MD5.Create();
        }
        #endregion PRIVATE METHODS



        #region PUBLIC METHODS
        public string GetHashCode(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            if (!File.Exists(path)) return string.Empty;

            var sb = new StringBuilder();

            try
            {
                using (var fs = File.OpenRead(path)) 
                {
                    foreach (var b in mHasher.ComputeHash(fs))
                    {
                        sb.Append(b.ToString(TO_HEXA).ToLower());
                    }
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }

            return sb.ToString().ToUpper();
        }

        public string GetHashCodeInMemory(ref byte[] bData)
        {
            if (bData == null) return string.Empty;

            var sb = new StringBuilder();

            try
            {
                var arrHashData = mHasher.ComputeHash(bData);
                foreach(var data in arrHashData)
                {
                    sb.Append(data.ToString(TO_HEXA).ToLower());
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }

            return sb.ToString().ToUpper();
        }
        #endregion PUBLIC METHODS
    }
}