using System.Collections.Generic;
using LitJson;
using UnityEngine;

namespace JunhPatcher
{
    public sealed class JsonArray : JsonValueBase
    {
        #region PRIVATE FIELDS
        private List<JsonValueBase> mValues;
        #endregion PRIVATE FIELDS

        #region PUBLIC METHODS
        public JsonArray() : base(JsonValueType.Array)
        {
            mValues = new List<JsonValueBase>();
        }

        public void Parse(ref JsonReader reader)
        {
            if(reader == null)
            {
                Debug.LogError("Invalid json reader.");
                return;
            }

            while(true)
            {
                var valueBase = JsonUnity.ParseValueFromReader(ref reader);
                if (valueBase == null) break;
                else mValues.Add(valueBase);
            }
        }

        public string GetValueWithIndex(int index)
        {
            if(index < byte.MinValue || mValues.Count <= index)
            {
                Debug.LogErrorFormat("Invalid index : {0}", index);
                return string.Empty;
            }

            JsonValue realValue = (JsonValue)mValues[index];
            return realValue.GetValue();
        }

        public JsonObject GetObjectWithIndex(int index)
        {
            if (index < byte.MinValue || mValues.Count <= index)
            {
                Debug.LogErrorFormat("Invalid index : {0}", index);
                return null;
            }

            JsonObject realObject = (JsonObject)mValues[index];
            return realObject;
        }

        public JsonArray GetArrayWithIndex(int index)
        {
            if (index < byte.MinValue || mValues.Count <= index)
            {
                Debug.LogErrorFormat("Invalid index : {0}", index);
                return null;
            }

            JsonArray realArray = (JsonArray)mValues[index];
            return realArray;
        }

        public override void ToJSONWriter(ref JsonWriter writer)
        {
            if (writer == null)
            {
                Debug.LogError("Invalid writer.");
                return;
            }

            writer.WriteArrayStart();

            foreach(var json in mValues)
            {
                json.ToJSONWriter(ref writer);
            }

            writer.WriteArrayEnd();
        }

        public int GetCount()
        {
            return mValues.Count;
        }
        #endregion PUBLIC METHODS
    }
}