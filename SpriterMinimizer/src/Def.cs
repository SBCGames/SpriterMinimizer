using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpriterMinimizer {

    public enum eAttribType {none, str, int8, uint8, int16, uint16, int32, uint32,
        fixed1_7, fixed8_8, fixed16_16,
        boolean,
        curveType, objInfoType, timelineObjType};

    class Defs {
        public Dictionary<string, Item> items = new Dictionary<string, Item>();
        public Def rootDef;
    }

    [JsonConverter(typeof(ItemConverter))]
    class Item {
        public string minName;
        public int binaryCode;
        public eAttribType type;
    }

    class Def {
        // parent def
        [JsonIgnore]
        public Def parent = null;

        public string name;
        [JsonProperty("minName")]
        public Item item;

        public Dictionary<string, Item> attributes = new Dictionary<string, Item>();
        public Dictionary<string, Def> childElements = new Dictionary<string, Def>();

        // ----------------------------------------------------------
        public bool ShouldSerializechildElements() {
            return childElements.Count > 0;
        }

        // ----------------------------------------------------------
        public bool ShouldSerializeattributes() {
            return attributes.Count > 0;
        }
    }

    public class ItemConverter : JsonConverter {
        // ----------------------------------------------------------
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            Item item = (Item)value;
            writer.WriteValue(item.minName);
        }

        // ----------------------------------------------------------
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            Item item = new Item();
            item.minName = (string)reader.Value;

            return item;
        }

        // ----------------------------------------------------------
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Item);
        }
    }
}
