using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpriterMinimizer {

    class DefsReader {
        // ----------------------------------------------------------
        public Defs ReadDefs(Options aOptions) {
            // top element
            var defs = new Defs();
            defs.rootDef = new Def();

            // load defs
            var doc = new XmlDocument();
            doc.Load(aOptions.defsFile);

            // read items table
            ReadItems(doc.GetElementsByTagName("items").Item(0) as XmlElement, defs.items);
            // proces elements recursively
            var spriterElement = doc.GetElementsByTagName("spriter").Item(0) as XmlElement;
            ReadSpriterDefs(spriterElement.GetElementsByTagName("element").Item(0) as XmlElement, defs.rootDef, defs.items);
            
            // output json file with defs
            var jsonDefsString = JsonConvert.SerializeObject(defs.rootDef, Newtonsoft.Json.Formatting.Indented);
            var jsonDefsFileName = Path.GetFileNameWithoutExtension(aOptions.defsFile) + ".json";
            using (StreamWriter sw = new StreamWriter(jsonDefsFileName)) {
                sw.Write(jsonDefsString);
            }

            // return top def
            return defs;
        }

        // ----------------------------------------------------------
        private void ReadItems(XmlElement aElement, Dictionary<string, Item> aItems) {
            // get all items
            var itemDefs = aElement.GetElementsByTagName("item");
            foreach(XmlNode node in itemDefs) {
                var element = node as XmlElement;

                var name = element.Attributes["name"].Value;

                var item = new Item();
                item.minName = element.Attributes["minName"].Value;
                item.binaryCode = int.Parse(element.Attributes["bin"].Value);
                item.type = (eAttribType) Enum.Parse(typeof(eAttribType), element.Attributes["type"].Value, true);

                aItems.Add(name, item);
            }
        }

        // ----------------------------------------------------------
        private void ReadSpriterDefs(XmlElement aElement, Def aDef, Dictionary<string, Item> aItems) {
            aDef.name = aElement.Attributes["name"].Value;

            if (!aElement.HasAttribute("item")) {
                Console.WriteLine("Missing item attribute for element " + aDef.name);
                return;
            }
            if (!aItems.ContainsKey(aElement.Attributes["item"].Value)) {
                Console.WriteLine("Missing item definition for item " + aElement.Attributes["item"].Value);
                return;
            }
            aDef.item = aItems[aElement.Attributes["item"].Value];

            var children = aElement.ChildNodes;

            foreach (XmlNode node in children) {
                if (node.NodeType == XmlNodeType.Element) {
                    XmlElement element = node as XmlElement;

                    if (element.Name == "element") {
                        // new def and set parent
                        var def = new Def();
                        def.parent = aDef;

                        ReadSpriterDefs(element, def, aItems);

                        // add this element to current table
                        aDef.childElements.Add(def.name, def);

                    } else if (element.Name == "attribute") {
                        // add attribute to current table
                        var name = element.Attributes["name"].Value;
                        if (!element.HasAttribute("item")) {
                            Console.WriteLine("Missing item attribute for attribute " + name);
                            return;
                        }
                        if (!aItems.ContainsKey(element.Attributes["item"].Value)) {
                            Console.WriteLine("Missing item definition for item " + element.Attributes["item"].Value);
                            return;
                        }
                        aDef.attributes.Add(name, aItems[element.Attributes["item"].Value]) ;

                    } else {
                        Console.WriteLine("ERROR: unknown element '" + element.Name + "'");
                    }
                }
            }
        }
    }
}
