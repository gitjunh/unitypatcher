using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using LitJson;

namespace JunhPatcher
{
    public sealed class PatchList
    {
        #region PRIVATE FIELDS
        private Dictionary<string, PatchData> mDatas;
        #endregion PRIVATE FIELDS



        #region PROPERTIES
        public Dictionary<string, PatchData> datas
        {
            get { return mDatas; }
            set { mDatas = value; }
        }
        #endregion PROPERTIES



        #region PUBLIC METHODS
        public PatchList()
        {
            mDatas = new Dictionary<string, PatchData>();
        }

        public Dictionary<string, PatchData> GetUpdateList(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData)) return null;

            var json = new JsonUnity(jsonData);
            if (json == null) return null;

            if (!string.IsNullOrEmpty(json.GetError())) return null;

            var newDatas = GetPatchDatas(json);
            if (newDatas == null)
            {
                Debug.LogError("Failed to get the PatchList");
                return null;
            }

            var path = Path.Combine(Application.temporaryCachePath, "PatchList.pat");
            if (!File.Exists(path))
            {
                return newDatas;
            }
            else
            {
                var text = File.ReadAllText(path);
                if (string.IsNullOrEmpty(text))
                {
                    return newDatas;
                }
                else
                {
                    return GetCompareDatas(newDatas, text);
                }
            }
        }

        public void UpdateData(PatchData data)
        {
            if (mDatas == null) return;

            if (mDatas.ContainsKey(data.Name))
            {
                mDatas[data.Name].CRC = data.CRC;
                mDatas[data.Name].Names = data.Names;
            }
            else
            {
                mDatas.Add(data.Name, data);
            }
        }

        public void WriteLocalPatchList()
        {
            var path = Path.Combine(Application.temporaryCachePath, "PatchList.pat");

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Invalid path.");
                return;
            }

            var sb = new StringBuilder();
            var jw = new JsonWriter(sb);

            jw.PrettyPrint = true;
            jw.IndentValue = 2;

            jw.WriteObjectStart();

            jw.WritePropertyName("Datas");
            jw.WriteArrayStart();

            foreach (KeyValuePair<string, PatchData> pair in mDatas)
            {
                pair.Value.WriteData(jw);
            }

            jw.WriteArrayEnd();
            jw.WriteObjectEnd();

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllText(path, sb.ToString());
        }
        #endregion PUBLIC METHODS



        #region PRIVATE METHODS
        private Dictionary<string, PatchData> GetPatchDatas(JsonUnity json)
        {
            var datas = new Dictionary<string, PatchData>();
            var array = json.GetArrayWithKey("Datas");

            JsonObject obj = null;
            PatchData data = null;

            for (var i = default(int); i < array.GetCount(); ++i)
            {
                obj = array.GetObjectWithIndex(i);
                data = new PatchData(obj);
                datas.Add(string.Format("{0}{1}", data.Path, data.Name), data);
            }

            return datas;
        }

        private Dictionary<string, PatchData> GetCompareDatas(Dictionary<string, PatchData> newDatas, string text)
        {
            var json = new JsonUnity(text);
            if (json == null) return newDatas;

            var error = json.GetError();
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
                return newDatas;
            }

            var array = json.GetArrayWithKey("Datas");
            if (array == null) return newDatas;

            JsonObject obj = null;
            PatchData info = null;

            mDatas = new Dictionary<string, PatchData>();
            for (var i = default(int); i < array.GetCount(); ++i)
            {
                obj = array.GetObjectWithIndex(i);
                info = new PatchData(obj);
                mDatas.Add(info.Name, info);
            }

            var updateDatas = new Dictionary<string, PatchData>();
            PatchData data = null;
            var path = string.Empty;
            var name = string.Empty;
            foreach (KeyValuePair<string, PatchData> pair in newDatas)
            {
                data = pair.Value;
                name = data.Name.Replace(".lzf", ".asset");
                path = string.Format("{0}/{1}{2}", Application.persistentDataPath, data.Path, name);
                if (File.Exists(path))
                {
                    if (mDatas.ContainsKey(data.Name))
                    {
                        if (data.Path.Contains("data/"))
                        {
                            if (data.Name[default(int)].Equals(mDatas[data.Name].Names[default(int)])) continue;
                        }
                        else
                        {
                            if (data.CRC.Equals(mDatas[data.Name].CRC)) continue;
                        }

                        updateDatas.Add(data.Name, data);
                    }
                    else
                    {
                        updateDatas.Add(data.Name, data);
                    }
                }
                else
                {
                    updateDatas.Add(data.Name, data);
                }
            }

            return updateDatas;
        }
        #endregion PRIVATE METHODS



        #region EDITOR METHODS
        public PatchList(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData)) return;

            var json = new JsonUnity(jsonData);
            if (json == null) return;

            if (!string.IsNullOrEmpty(json.GetError())) return;

            if(mDatas == null)
            {
                mDatas = new Dictionary<string, PatchData>();
            }

            mDatas.Clear();

            var array = json.GetArrayWithKey("Datas");
            if (array == null) return;

            JsonObject obj = null;
            PatchData data = null;

            for (var i = default(int); i < array.GetCount(); ++i)
            {
                obj = array.GetObjectWithIndex(i);
                data = new PatchData(obj);

                mDatas.Add(string.Format("{0}{1}", data.Path, data.Name), data);
            }
        }

        public void AddData(PatchData data)
        {
            if (data == null) return;

            mDatas.Add(string.Format("{0}{1}", data.Path, data.Name), data);
        }

        public void ClearDatas()
        {
            if (mDatas == null) return;
            mDatas.Clear();
        }

        public string GetJsonString()
        {
            var sb = new StringBuilder();
            var jw = new JsonWriter(sb);

            jw.PrettyPrint = true;
            jw.IndentValue = 2;

            WriteJsonData(jw);

            return sb.ToString();
        }

        private void WriteJsonData(JsonWriter writer)
        {
            if (writer == null) return;

            writer.WriteObjectStart();
            writer.WritePropertyName("Datas");
            writer.WriteArrayStart();

            foreach(var data in mDatas.Values)
            {
                data.WriteData(writer);
            }

            writer.WriteArrayEnd();
            writer.WriteObjectEnd();
        }
        #endregion EDITOR METHODS
    }
}