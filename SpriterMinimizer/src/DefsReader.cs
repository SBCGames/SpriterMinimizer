using System;
using System.Xml;

namespace SpriterMinimizer {
    class DefsReader {
        // ----------------------------------------------------------
        public Def ReadDefs(string aDefsFile) {
            // top element
            var rootDef = new Def();

            // load defs
            var doc = new XmlDocument();
            doc.Load(aDefsFile);

            // proces elements recursively
            ReadElement(doc.DocumentElement, rootDef);

            // return top def
            return rootDef;
        }

        // ----------------------------------------------------------
        private void ReadElement(XmlElement aElement, Def aDef) {
            aDef.name = aElement.Attributes["oldName"].Value;
            aDef.newName = aElement.HasAttribute("newName") ? aElement.Attributes["newName"].Value : aDef.name;

            var children = aElement.ChildNodes;

            foreach (XmlNode node in children) {
                if (node.NodeType == XmlNodeType.Element) {
                    XmlElement element = node as XmlElement;

                    if (element.Name == "element") {
                        // new def and set parent
                        var def = new Def();
                        def.parent = aDef;

                        ReadElement(element, def);

                        // add this element to current table
                        aDef.childElements.Add(def.name, def);

                    } else if (element.Name == "attribute") {
                        // add attribute to current table
                        var name = element.Attributes["oldName"].Value;
                        var newName = element.Attributes["newName"].Value;

                        aDef.attributes.Add(name, newName);

                    } else {
                        Console.WriteLine("ERROR: unknown element '" + element.Name + "'");
                    }
                }
            }
        }
    }
}
