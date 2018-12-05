using LitJson;

namespace JunhPatcher
{
    public enum JsonValueType : byte
    {
        None = byte.MinValue,
        Boolean,
        Double,
        Int,
        Long,
        Null,
        String,
        Array,
        Object,
    }

    public class JsonValueBase
    {
        #region PRIVATE FIELDS
        private JsonValueType mValueType;
        #endregion PRIVATE FIELDS

        #region PROPERTIES
        public JsonValueType ValueType
        {
            get { return mValueType; }
            protected set { mValueType = value; }
        }
        #endregion PROPERTIES

        #region PUBLIC METHODS
        public JsonValueBase(JsonValueType valueType)
        {
            ValueType = valueType;
        }

        public virtual void ToJSONWriter(ref JsonWriter writer) { }
        #endregion PUBLIC METHODS
    }
}