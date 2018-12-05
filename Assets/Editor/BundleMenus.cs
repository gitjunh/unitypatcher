using UnityEditor;
using UnityEngine;
using System.IO;

namespace JunhPatcher
{
    public sealed class BundleMenus
    {
        #region MENU METHODS
        [MenuItem("Tools/Build/AssetBundle/BuildAll")]
        private static void MakeAllBundles()
        {
            var src = Path.Combine(Application.dataPath, "ResourcesForPackagingAssets");
            var dest = Path.Combine(Application.dataPath, "../AssetBundles");

            MakeBundles(ref src, ref dest);
        }
        #endregion MENU METHODS

        #region PRIVATE METHODS
        private static void MakeBundles(ref string src, ref string dest)
        {
            if (string.IsNullOrEmpty(src))
            {
                EditorUtility.DisplayDialog("Error", "Invalid source path.\nStop Build.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(dest))
            {
                EditorUtility.DisplayDialog("Error", "Invalid destination path.\nStop Build.", "OK");
                return;
            }

            if (!Directory.Exists(src))
            {
                Directory.CreateDirectory(src);
            }

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            var builder = new Builder();

            builder.MakeOutputPath(dest);
            builder.LoadHashes();
            builder.CheckHashes(ref src, ref dest);
            builder.SaveHashes();
            builder.LoadPatchList();
            builder.MakeHashBundles();
            builder.MakeHashPatchList();
        }
        #endregion PRIVE METHODS
    }
}
