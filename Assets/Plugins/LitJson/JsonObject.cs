using System.Collections.Generic;
using LitJson;
using UnityEngine;


namespace JunhPatcher
{
    public sealed class JsonObject : JsonValueBase
    {
        #region PRIVATE FIELDS
        private Dictionary<string, JsonValueBase> mValues;
        #endregion PRIVATE FIELDS

        #region PUBLIC METHODS
        public JsonObject() : base(JsonValueType.Object)
        {
            mValues = new Dictionary<string, JsonValueBase>();
        }

        public void Parse(ref JsonReader reader)
        {
            if (reader == null) 
            {
                Debug.LogError("reader is invalid.");
                return;
            }

            while(reader.Read())
            {
                if (reader.Token == JsonToken.ObjectEnd) break;
                else if(reader.Token == JsonToken.PropertyName)
                {
                    var key = reader.Value.ToString();
                    //JsonValueBase = Json
                }
            }
        }

        public string GetValueWithKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Invalid key.");
                return string.Empty;
            }

            JsonValueBase valueBase = null;
            var result = mValues.TryGetValue(key, out valueBase);
            if (!result) return null;

            if (valueBase.ValueType == JsonValueType.Object || valueBase.ValueType == JsonValueType.Array) return null;

            var realValue = (JsonValue)valueBase;
            return realValue.GetValue();
        }

        public JsonObject GetObjectWithKey(ref string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Invalid key.");
                return null;
            }

            JsonValueBase valueBase = null;
            var result = mValues.TryGetValue(key, out valueBase);
            if (!result) return null;

            if (valueBase.ValueType != JsonValueType.Object) return null;

            return (JsonObject)valueBase;
        }

        public JsonArray GetArrayWithKey(string key)
        {
            JsonValueBase valueBase = null;
            var result = mValues.TryGetValue(key, out valueBase);
            if (!result) return null;

            if (valueBase.ValueType != JsonValueType.Array) return null;

            return (JsonArray)valueBase;
        }

        public override void ToJSONWriter(ref JsonWriter writer)
        {
            if(writer == null)
            {
                Debug.LogError("Invalid writer.");
                return;
            }

            writer.WriteObjectStart();

            foreach(KeyValuePair<string, JsonValueBase> pair in mValues)
            {
                writer.WritePropertyName(pair.Key);
                pair.Value.ToJSONWriter(ref writer);
            }

            writer.WriteObjectEnd();
        }
        #endregion PUBLIC METHODS
    }
}