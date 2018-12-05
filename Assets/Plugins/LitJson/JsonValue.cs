using LitJson;
using UnityEngine;

namespace JunhPatcher
{
    public sealed class JsonValue : JsonValueBase
    {
        #region PRIVATE FIELDS
        private string mValue;
        #endregion PRIVATE FIELDS

        #region PUBLIC METHODS
        public JsonValue(JsonValueType valueType) : base(valueType) {}

        public void SetValue(string value)
        {
            mValue = value;
        }

        public string GetValue()
        {
            return mValue;
        }

        public override void ToJSONWriter(ref JsonWriter writer)
        {
            if (writer == null)
            {
                Debug.LogError("Invalid writer.");
                return;
            }

            switch (ValueType)
            {
                case JsonValueType.Boolean:
                    {
                        bool value;
                        if (!bool.TryParse(mValue, out value)) value = false;

                        writer.Write(value);
                    }
                    break;

                case JsonValueType.Double:
                    {
                        double value;
                        if (!double.TryParse(mValue, out value)) value = -1;

                        writer.Write(value);
                    }
                    break;

                case JsonValueType.Int:
                    {
                        int value;
                        if (!int.TryParse(mValue, out value)) value = -1;

                        writer.Write(value);
                    }
                    break;

                case JsonValueType.Long:
                    {
                        long value;
                        if (!long.TryParse(mValue, out value)) value = -1;

                        writer.Write(value);
                    }
                    break;

                case JsonValueType.Null:
                case JsonValueType.String:
                    {
                        writer.Write(mValue);
                    }
                    break;
            }
        }
        #endregion PUBLIC METHODS
    }
}