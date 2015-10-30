using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;

namespace SpriterMinimizer {
    class XmlMinimizer {

        private XmlDocument _reader;
        private XmlWriter _writer;

        private HashSet<string> _warnings = new HashSet<string>();

        // ----------------------------------------------------------
        public void Minimize(Options aOptions, Def aDef) {
            // reader
            _reader = new XmlDocument();
            _reader.Load(aOptions.inFile);

            // writter with write settings
            var writterSettings = new XmlWriterSettings();
            if (aOptions.prettyPrint) {
                writterSettings.Indent = true;
                writterSettings.IndentChars = "  ";
            }
            _writer = XmlWriter.Create(aOptions.outFile, writterSettings);

            _writer.WriteStartDocument();

            ProcessElement(_reader.DocumentElement, aDef);

            _writer.WriteEndDocument();
            _writer.Close();

            // print warnings
            foreach(string warning in _warnings) {
                Console.WriteLine(warning);
            }
        }

        // ----------------------------------------------------------
        private void ProcessElement(XmlElement aElement, Def aDef) {
            bool isElementDef = aDef != null && (aElement.Name == aDef.name);

            // write element
            var newName = isElementDef ? aDef.item.minName : aElement.Name;
            _writer.WriteStartElement(newName);

            // special case for very first element - add attribute to say this file is minimized
            if (aElement.Name == "spriter_data") {
                _writer.WriteAttributeString("min", "true");
            }

            // write all attributes
            foreach(XmlAttribute attribute in aElement.Attributes) {
                if (aDef != null && aDef.attributes.ContainsKey(attribute.Name)) {
                    _writer.WriteAttributeString(aDef.attributes[attribute.Name].minName, attribute.Value);
                } else {
                    PrintMissingAttributeDef(attribute.Name, aElement.Name);
                    _writer.WriteAttributeString(attribute.Name, attribute.Value);
                }
            }

            // next elements
            foreach(XmlNode node in aElement.ChildNodes) {
                if (node.NodeType == XmlNodeType.Element) {
                    XmlElement element = node as XmlElement;

                    if (aDef != null && aDef.childElements.ContainsKey(element.Name)) {
                        ProcessElement(element, aDef.childElements[element.Name]);
                    } else {
                        PrintMissingElementDef(element.Name, aDef);
                        ProcessElement(element, null);
                    }
                }
            }

            // end element
            _writer.WriteEndElement();
        }

        // ----------------------------------------------------------
        private void PrintMissingElementDef(string aName, Def aDef) {
            StringBuilder stringBuilder = new StringBuilder();

            while(aDef != null) {
                stringBuilder.Insert(0, aDef.name + "->");
                aDef = aDef.parent;
            }

            stringBuilder.Append(aName);
            stringBuilder.Insert(0, "WARNING: missing new name for element ");

            _warnings.Add(stringBuilder.ToString());
        }

        // ----------------------------------------------------------
        private void PrintMissingAttributeDef(string aAttributeName, string aElementName) {
            _warnings.Add("WARNING: missing new name for attribute " + aAttributeName + " in element " + aElementName);
        }
    }
}
