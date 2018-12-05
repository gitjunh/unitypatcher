using UnityEngine;
using LitJson;

namespace JunhPatcher
{
    public sealed class JsonUnity
    {
        #region PRIVATE FIELDS
        private JsonObject mBaseObject;
        private string mJsongString;
        private string mReasonOfInvalid;
        #endregion PRIVATE FIELDS



        #region PUBLIC METHODS    
        public JsonUnity(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                Debug.LogError("Invalid json string.");
                return;
            }

            mJsongString = jsonString;
            Parse();
        }

        public string GetValueWithKey(string key)
        {
            if(string.IsNullOrEmpty(key))
            {
                Debug.LogError("Invalid key.");
                return string.Empty;
            }

            if(mBaseObject == null)
            {
                Debug.LogError("Failed to parse an object.");
                return string.Empty;
            }

            return mBaseObject.GetValueWithKey(key);
        }

        public JsonObject GetObjectWithKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Invalid key.");
                return null;
            }

            if (mBaseObject == null)
            {
                Debug.LogError("Failed to parse an object.");
                return null;
            }

            return mBaseObject.GetObjectWithKey(ref key);
        }

        public JsonArray GetArrayWithKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Invalid key.");
                return null;
            }

            if (mBaseObject == null)
            {
                Debug.LogError("Failed to parse an object.");
                return null;
            }

            return mBaseObject.GetArrayWithKey(key);
        }

        public string GetError()
        {
            return mReasonOfInvalid;
        }

        public JsonObject GetBaseObject()
        {
            return mBaseObject;
        }
        #endregion PUBLIC METHODS

        #region PRIVATE METHODS
        private void Parse()
        {
            mReasonOfInvalid = string.Empty;
            var reader = new JsonReader(mJsongString);

            try
            {
                reader.Read();
                if (reader.Token == JsonToken.ObjectStart)
                {
                    mBaseObject = new JsonObject();
                    mBaseObject.Parse(ref reader);
                }
            }
            catch (JsonException e)
            {
                mBaseObject = null;
                mReasonOfInvalid = e.ToString();
            }
        }
        #endregion PRIVATE METHODS

        #region STATIC METHODS
        public static JsonValueBase ParseValueFromReader(ref JsonReader reader)
        {
            if(reader == null)
            {
                Debug.LogError("Invalid json reader.");
                return null;
            }

            reader.Read();

            JsonValueBase valueBase = null;

            switch(reader.Token)
            {
                case JsonToken.ObjectStart:
                    {
                        JsonObject obj = new JsonObject();
                        obj.Parse(ref reader);
                        valueBase = obj;
                    }
                    break;

                case JsonToken.ArrayStart:
                    {
                        JsonArray array = new JsonArray();
                        array.Parse(ref reader);
                        valueBase = array;
                    }
                    break;

                case JsonToken.Boolean:
                    {
                        JsonValue value = new JsonValue(JsonValueType.Boolean);
                        value.SetValue(reader.Value.ToString());
                        valueBase = value;
                    }
                    break;

                case JsonToken.Double:
                    {
                        JsonValue value = new JsonValue(JsonValueType.Double);
                        value.SetValue(reader.Value.ToString());
                        valueBase = value;
                    }
                    break;

                case JsonToken.Int:
                    {
                        JsonValue value = new JsonValue(JsonValueType.Int);
                        value.SetValue(reader.Value.ToString());
                        valueBase = value;
                    }
                    break;

                case JsonToken.Long:
                    {
                        JsonValue value = new JsonValue(JsonValueType.Long);
                        value.SetValue(reader.Value.ToString());
                        valueBase = value;
                    }
                    break;

                case JsonToken.None:
                    {
                        JsonValue value = new JsonValue(JsonValueType.None);
                        value.SetValue(string.Empty);
                        valueBase = value;
                    }
                    break;

                case JsonToken.Null:
                    {
                        JsonValue value = new JsonValue(JsonValueType.Null);
                        value.SetValue(string.Empty);
                        valueBase = value;
                    }
                    break;

                case JsonToken.String:
                    {
                        JsonValue value = new JsonValue(JsonValueType.String);
                        value.SetValue(reader.Value.ToString());
                        valueBase = value;
                    }
                    break;

                case JsonToken.ArrayEnd:
                    {
                        valueBase = null;
                    }
                    break;
            }

            return valueBase;
        }
        #endregion STATIC METHODS
    }
}