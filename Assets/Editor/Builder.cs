using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using LitJson;

namespace JunhPatcher
{
    public sealed class Builder
    {
        #region PRIVATE FIELDS
        private Dictionary<string, HashInfo> mHashes;
        private PatchList mPatchList;
        private List<string> mFails;
        private string mOutPath;

        private BuildTarget mBuildtarget;
        private BuildAssetBundleOptions mBuildOptions;
        #endregion PRIVATE FIELDS



        #region PUBLIC METHODS
        public Builder()
        {
            mHashes = new Dictionary<string, HashInfo>();
            mPatchList = new PatchList();
            mFails = new List<string>();

            mBuildtarget = GetBuildTarget();
            mBuildOptions = BuildAssetBundleOptions.AppendHashToAssetBundleName;
        }

        public void MakeOutputPath(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                Debug.LogError("Invalid path.");
                return;
            }

#if UNITY_ANDROID
            mOutPath = Path.Combine(path, "android");
#elif UNITY_IPHONE
            mOutPath = Path.Combine(path, "ios");
#else 
            mOutPath = Path.Combine(path, "android");
#endif //UNITY_ANDROID...

            mOutPath = mOutPath.Replace(Path.DirectorySeparatorChar, '/');

            if(!Directory.Exists(mOutPath))
            {
                Directory.CreateDirectory(mOutPath);
            }
        }

        public void LoadHashes()
        {
            mHashes.Clear();

            var temp = string.Format("{0}/hashes.hsh", mOutPath);
            if (!File.Exists(temp)) return;

            var data = File.ReadAllText(temp);
            if(string.IsNullOrEmpty(data))
            {
                Debug.LogError("Invalid data.");
                return;
            }

            var json = new JsonUnity(data);
            if(json == null)
            {
                Debug.LogError("Failed to create a json.");
                return;
            }

            temp = json.GetError();
            if(!string.IsNullOrEmpty(temp))
            {
                Debug.LogError(temp);
                return;
            }

            var key = "Hashes";
            var array = json.GetArrayWithKey(key);
            if(array == null)
            {
                Debug.LogError("Failed to get the Hashes.");
                return;
            }

            var subKey = string.Empty;
            var keyString = "key";
            var subKeyString = "subKey";
            var fileKeyString = "Files";
            var hashKeyString = "Hash";
            var hashstring = string.Empty;

            JsonObject obj = null;
            JsonObject subObj = null;
            JsonArray sub = null;
            FileHash file = null;
            HashInfo hash = null;
            Dictionary<string, FileHash> files = new Dictionary<string, FileHash>();

            for (var i = default(int); i < array.GetCount(); ++i)
            {
                files.Clear();

                obj = array.GetObjectWithIndex(i);
                key = obj.GetValueWithKey(keyString);
                sub = obj.GetArrayWithKey(fileKeyString);

                for (var j = default(int); j < sub.GetCount(); ++j)
                {
                    subObj = sub.GetObjectWithIndex(j);
                    subKey = subObj.GetValueWithKey(subKeyString);
                    hashstring = subObj.GetValueWithKey(hashKeyString);
                    file = new FileHash(subKey, hashstring);

                    files.Add(subKey, file);
                    Debug.Log(subKey);
                }

                hash = new HashInfo(HashInfo.NOT_CHANGED, key, files);

                mHashes.Add(key, hash);
            }

            Debug.Log("Load hashes complete.");
        }

        public void CheckHashes(ref string src, ref string dest)
        {
            var path = src.Remove(default(int), Application.dataPath.Length);
            path = path.TrimStart(Path.DirectorySeparatorChar);

            var root = src.TrimEnd(path.ToCharArray());
            root = root.TrimEnd(Path.DirectorySeparatorChar);

            RecursionHash(ref root, ref path, ref mOutPath);
        }

        public void SaveHashes()
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter jw = new JsonWriter(sb);

            jw.PrettyPrint = true;
            jw.IndentValue = 2;
            jw.WriteObjectStart();
            jw.WritePropertyName("Hashes");
            jw.WriteArrayStart();

            foreach(KeyValuePair<string, HashInfo> pair in mHashes)
            {
                if (pair.Value.files.Count <= default(int)) continue;

                jw.WriteObjectStart();

                jw.WritePropertyName("Key");
                jw.Write(pair.Key);

                jw.WritePropertyName("Files");
                jw.WriteArrayStart();

                foreach(KeyValuePair<string, FileHash> file in pair.Value.files)
                {
                    jw.WriteObjectStart();

                    jw.WritePropertyName("subKey");
                    jw.Write(file.Key);
                    jw.WritePropertyName("Hash");
                    jw.Write(file.Value.hash);

                    jw.WriteObjectEnd();
                }

                jw.WriteArrayEnd();

                jw.WriteObjectEnd();

                Debug.Log(string.Format("{0} saved.", pair.Key));
            }

            jw.WriteArrayEnd();
            jw.WriteObjectEnd();

            var path = string.Format("{0}/hashes.hsh", mOutPath);

            if(File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllText(path, sb.ToString());

            Debug.Log("save hashes complete.");
        }

        public void LoadPatchList()
        {
            if(mPatchList != null)
            {
                mPatchList.ClearDatas();
            }

            var path = Path.Combine(mOutPath, "PatchList");
            mPatchList = ReadPatchList(Path.Combine(path, "patchlist.pat"));

            if(mPatchList == null)
            {
                mPatchList = new PatchList();
            }
        }

        public void MakeHashBundles()
        {
            var key = string.Empty;
            HashInfo info = null;

            foreach(KeyValuePair<string, HashInfo> pair in mHashes)
            {
                info = pair.Value;
                if (info.state.Equals(FileState.NotChanged)) continue;
                if (info.files.Count <= default(int)) continue;

                key = pair.Key;

                if(key.Contains("/CachedMap/") || key.Contains("/RegenData/") || key.Contains("/TriggerDatas/")|| key.Contains("/WayPointData/"))
                {
                    HashETCBuild(pair.Key, info);
                }
                else if (key.Contains("/Data/"))
                {
                    HashDataBuild(pair.Key, info);
                }
                else if (key.Contains("/Maps/")) continue;
                else
                {
                    HashETCBuild(pair.Key, info);
                }
            }
        }

        public void MakeHashPatchList()
        {
            var path = Path.Combine(mOutPath, "PatchList");
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            WritePatchList(mPatchList, Path.Combine(path, "patchlist.pat"));
        }
        #endregion PUBLIC METHODS



        #region PRIVATE METHODS
        private BuildTarget GetBuildTarget()
        {
#if UNITY_ANDROID
            return BuildTarget.Android;
#elif UNITY_IPHONE
            return BuildTarget.iOS;
#else 
            return BuildTarget.Android;
#endif //UNITY_ANDROID...
        }

        private void RecursionHash(ref string root, ref string src, ref string parent)
        {
            if(string.IsNullOrEmpty(root))
            {
                Debug.LogError("Invalid root");
                return;
            }

            if(string.IsNullOrEmpty(src))
            {
                Debug.LogError("Invalid source.");
                return;
            }

            if(string.IsNullOrEmpty(parent))
            {
                Debug.LogError("Invalid parent.");
                return;
            }

            var curPath = string.Format("{0}/{1}", root, src);

            if (curPath.Contains(".svn")) return;

            if(src.Equals("Data"))
            {
                RecursionDataHash(ref curPath, ref parent);
            }
            else
            {
                RecursionETCHash(curPath, parent, src);
            }

            RecursionDirectoryHash(curPath, parent);
        }

        private void RecursionDataHash(ref string path, ref string parent)
        {
            var files = GetAssetFiles(ref path);

            if(files == null)
            {
                Debug.LogError("Failed to get files.");
                return;
            }

            if (files.Length <= default(int)) return;

            FileHash fileHash = null;
            Dictionary<string, FileHash> hashes = null;
            HashInfo info = null;
            var hash = string.Empty;
            var key = string.Empty;
            var ext = string.Empty;

            var md5Hash = MD5Hash.instance;
            foreach(var file in files)
            {
                ext = Path.GetExtension(file);

                if (ext.Equals(".unity")) continue;

                key = string.Format("{0}/{1}.asset", parent, Path.GetFileNameWithoutExtension(file));
                hash = md5Hash.GetHashCode(file);

                if(mHashes.ContainsKey(key))
                {
                    info = mHashes[key];
                    fileHash = info.files[file];

                    if(fileHash.hash.Equals(hash))
                    {
                        info.state = FileState.NotChanged;
                    }
                    else
                    {
                        info.state = FileState.Changed;
                        fileHash.hash = hash;
                    }
                }
                else
                {
                    fileHash = new FileHash(file, hash);
                    hashes = new Dictionary<string, FileHash>();
                    hashes.Add(file, fileHash);

                    info = new HashInfo(HashInfo.CHANGED, file, hashes);

                    mHashes.Add(key, info);
                }
            }
        }

        private void RecursionETCHash(string path, string parent, string src)
        {
            var files = GetAssetFiles(ref path);

            if(files == null)
            {
                Debug.LogError("Failed to get files.");
                return;
            }

            if (files.Length <= default(int)) return;

            var asset = GetAssetName(ref parent, ref src);
            var outPath = string.Format("{0}/{1}", parent, asset);

            FileHash fileHash = null;
            Dictionary<string, FileHash> hashes = null;
            HashInfo info = null;

            var hash = string.Empty;
            var ext = string.Empty;
            var isSame = true;

            var md5Hash = MD5Hash.instance;

            if(mHashes.ContainsKey(outPath))
            {
                info = mHashes[outPath];
                hashes = info.files;

                if(!hashes.Count.Equals(files.Length))
                {
                    WriteChangeHash(info, hashes, files);
                }
                else
                {
                    isSame = true;
                    foreach(var file in files)
                    {
                        if(!hashes.ContainsKey(file))
                        {
                            isSame = false;
                            break;
                        }
                    }

                    if(!isSame)
                    {
                        WriteChangeHash(info, hashes, files);
                    }
                    else
                    {
                        foreach(var file in files)
                        {
                            ext = Path.GetExtension(file);
                            if (ext.Equals(".unity")) continue;

                            hash = md5Hash.GetHashCode(file);

                            if (hashes[file].hash.Equals(hash)) continue;

                            hashes[file].hash = hash;

                            info.state = FileState.Changed;
                        }
                    }
                }
            }
            else
            {
                hashes = new Dictionary<string, FileHash>();
                foreach(var file in files)
                {
                    ext = Path.GetExtension(file);
                    if (ext.Equals(".unity")) continue;

                    hash = md5Hash.GetHashCode(file);
                    fileHash = new FileHash(file, hash);

                    hashes.Add(file, fileHash);
                }

                info = new HashInfo(HashInfo.CHANGED, outPath, hashes);

                mHashes.Add(outPath, info);
            }
        }

        private string[] GetAssetFiles(ref string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            var list = new List<string>();

            var files = Directory.GetFiles(path, "8", SearchOption.TopDirectoryOnly);
            var newFile = string.Empty;
            var ext = string.Empty;

            foreach(var file in files)
            {
                ext = Path.GetExtension(file);

                if (ext.Equals(".meta") || ext.Equals(".DS_Store") || ext.Equals(".svn")) continue;
                newFile = file.Replace(Path.DirectorySeparatorChar, '/');
                list.Add(newFile);
            }

            return list.ToArray();
        }

        private string GetAssetName(ref string parentPath, ref string src)
        {
            var path = parentPath.Remove(default(int), mOutPath.Length);
            if(path.StartsWith("/"))
            {
                path = path.Remove(default(int), ".".Length);
            }

            var paths = path.Split('/');

            var sb = new StringBuilder();

            foreach(var dir in paths)
            {
                if (string.IsNullOrEmpty(dir)) continue;
                if (dir.Equals(src)) continue;

                sb.Append(dir);
                sb.Append("_");
            }

            return string.Format("{0}{1}.asset", sb.ToString(), src).ToLower();
        }

        private void WriteChangeHash(HashInfo info, Dictionary<string, FileHash> hashes, string[] files)
        {
            info.state = FileState.Changed;
            hashes.Clear();

            FileHash fileHash = null;
            var hash = string.Empty;
            var md5Hash = MD5Hash.instance;
            foreach(var file in files)
            {
                hash = md5Hash.GetHashCode(file);
                fileHash = new FileHash(file, hash);

                hashes.Add(file, fileHash);
            }
        }

        private void RecursionDirectoryHash(string curPath, string parentPath)
        {
            string[] temps = null;
            var newPath = string.Empty;
            var outPath = string.Empty;
            string[] directories = Directory.GetDirectories(curPath);

            foreach(var directory in directories)
            {
                outPath = directory.Remove(Path.DirectorySeparatorChar, '/');
                temps = outPath.Split('/');

                if (temps == null || temps.Length <= default(int)) continue;

                newPath = temps[temps.Length - 1];
                outPath = string.Format("{0}/{1}", parentPath, newPath);

                if(!Directory.Exists(outPath))
                {
                    Directory.CreateDirectory(outPath);
                }

                RecursionHash(ref curPath, ref newPath, ref outPath);
            }
        }

        private PatchList ReadPatchList(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            if(!File.Exists(path))
            {
                var texts = File.ReadAllText(path);
                return new PatchList(texts);
            }

            return null;
        }

        private void HashETCBuild(string outPath, HashInfo info)
        {
            var files = info.files.Keys.ToList<string>();
            var paths = new string[files.Count];
            for (var i = default(int); i < files.Count; ++i)
            {
                paths[i] = GetPath(files[i]);
            }

            var assets = GetMainAssets(paths);
            var names = GetAssetNames(files.ToArray());
            var crc = default(uint);

            Debug.LogFormat("Try build asset : {0}", outPath);
            BuildPipeline.BuildAssetBundleExplicitAssetNames(assets, names, outPath, out crc, mBuildOptions, mBuildtarget);
            Debug.LogFormat("Name : {0} | CRC : {1} | Path : {2}", names, crc, outPath);

            if(crc == default(uint) || !File.Exists(outPath))
            {
                Debug.LogErrorFormat("Failed to create asset : {0}", info.path);
                return;
            }
            else
            {
                var bytes = File.ReadAllBytes(outPath);
                var comps = CLZF2.Compress(bytes);

                var newPath = string.Format("{0}/{1}.lzf", Path.GetDirectoryName(outPath), Path.GetFileNameWithoutExtension(outPath));
                var fs = new FileStream(newPath, FileMode.Create);

                fs.Write(comps, 0, comps.Length);
                fs.Close();

                var data = MakePatchData(outPath, crc, names);
                var key = string.Format("{0}/{1}", data.Path, data.Name);

                if(mPatchList.datas.ContainsKey(key))
                {
                    mPatchList.datas.Remove(key);
                }

                mPatchList.AddData(data);
            }
        }

        private string GetPath(string path)
        {
            var sub = string.Format("Assets/");
            var index = path.IndexOf(sub);
            return path.Substring(index);
        }

        private Object[] GetMainAssets(string[] files)
        {
            if (files == null) return null;

            var list = new List<Object>();
            Object obj = null;

            foreach(var file in files)
            {
                obj = AssetDatabase.LoadMainAssetAtPath(file);
                list.Add(obj);
            }

            return list.ToArray();
        }

        private string[] GetAssetNames(string[] paths)
        {
            if (paths == null) return null;

            var list = new List<string>();
            var name = string.Empty;

            foreach(var path in paths)
            {
                name = Path.GetFileNameWithoutExtension(path);
                list.Add(name);
            }

            return list.ToArray();
        }

        private PatchData MakePatchData(string path, uint crc, string[] names)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (names == null) return null;
            if (names.Length <= 0) return null;

            var name = Path.GetFileName(path);

            var refPath = path.Remove(0, mOutPath.Length);
            if(refPath.StartsWith("/"))
            {
                refPath = refPath.Remove(0, "/".Length);
            }

            refPath = refPath.Replace(name, string.Empty);

            var file = new FileInfo(path);
            if (file == null) return null;

            var size = file.Length;
            if (size <= 0) return null;

            name = name.Replace(".asset", ".lzf");
            var data = new PatchData(name, 0, crc, size, names, refPath);
            if (data == null) return null;

            return data;
        }

        private void HashDataBuild(string outPath, HashInfo info)
        {
            var files = info.files.Keys.ToList<string>();
            var paths = new string[files.Count];

            for (var i = 0; i < files.Count; ++i)
            {
                paths[i] = GetPath(files[i]);
            }

            PatchData data = null;
            Object[] assets = GetMainAssets(paths);
            var names = new string[1];
            var hash = string.Empty;
            var key = string.Empty;
            byte[] bytes = null;
            byte[] comps = null;
            var newPath = string.Empty;
            FileStream fs = null;
            var md5Hash = MD5Hash.instance;

            for (var i = 0; i < assets.Length; ++i)
            {
                if(!File.Exists(outPath))
                {
                    File.Copy(paths[i], outPath);
                }
                else
                {
                    File.Copy(paths[i], outPath, true);
                }

                hash = md5Hash.GetHashCode(outPath);
                names[0] = hash;

                bytes = File.ReadAllBytes(outPath);
                comps = CLZF2.Compress(bytes);

                newPath = string.Format("{0}/{1}.lzf", Path.GetDirectoryName(outPath), Path.GetFileNameWithoutExtension(outPath));
                fs = new FileStream(newPath, FileMode.Create);
                fs.Write(comps, 0, comps.Length);
                fs.Close();

                data = MakePatchData(outPath, 0, names);
                key = string.Format("{0}{1}", data.Path, data.Name);

                if(mPatchList.datas.ContainsKey(key))
                {
                    mPatchList.datas.Remove(key);
                }

                mPatchList.AddData(data);
            }
        }

        private void WritePatchList(PatchList list, string path)
        {
            if (list == null) return;

            var data = list.GetJsonString();
            File.WriteAllText(path, data);
        }
        #endregion PRIVATE METHODS
    }
}