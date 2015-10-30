using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Globalization;

namespace SpriterMinimizer {

    class Xml2Bin {

        private bool _bigOffset = true;
        private int _offsetSize = 4;

        // ----------------------------------------------------------
        public void Convert(Options aOptions, Def aRootDef) {
            if (aOptions.smallOffset) {
                _bigOffset = false;
                _offsetSize = 2;
            }
            
            // reader
            XmlDocument reader = new XmlDocument();
            reader.Load(aOptions.inFile);

            using (FileStream fs = new FileStream(aOptions.outFile, FileMode.Create)) {
                using (BinaryWriter writer = new BinaryWriter(fs)) {
                    // write 0 for bigOffset and 1 for smallOffset
                    writer.Write((byte)(_bigOffset ? 0 : 1));
                    // write all elements recursively
                    ProcessElement(1, reader.DocumentElement, aRootDef, writer);
                }
            }
        }

        // ----------------------------------------------------------
        private void ProcessElement(int aPosition, XmlElement aElement, Def aDef, BinaryWriter aWriter) {
            bool isElementDef = aDef != null && (aElement.Name == aDef.name);

            // save initial position
            int initialPosition = aPosition;

            // write element
            byte elementID = (byte) (aDef.item.binaryCode & 0xFF);
            aWriter.Write(elementID);

            // get subelements and count them
            var subElements = aElement.ChildNodes;
            byte subElementsCount = (byte)CountSubElements(subElements);
            aWriter.Write(subElementsCount);
            int subElsPosition = (int) aWriter.BaseStream.Position;
            int subElsPositionCurrent = subElsPosition;
            aWriter.Seek(subElementsCount * _offsetSize, SeekOrigin.Current);

            // write number of attributes
            aWriter.Write((byte)aElement.Attributes.Count);
            // write all attributes
            foreach (XmlAttribute attribute in aElement.Attributes) {
                if (aDef != null && aDef.attributes.ContainsKey(attribute.Name)) {
                    var attribItem = aDef.attributes[attribute.Name];
                    // write attrib ID
                    aWriter.Write((byte) attribItem.binaryCode);
                    // write attrib value
                    WriteAttribute(attribute.Value, attribItem.type, aWriter);
                } else {
                    Console.WriteLine("Attribute '" + attribute.Name + "' item definition is missing");
                }
            }
            
            // write sub elements
            foreach (XmlNode node in aElement.ChildNodes) {
                if (node.NodeType == XmlNodeType.Element) {
                    XmlElement element = node as XmlElement;

                    int currentPosition = (int) aWriter.BaseStream.Position;
                    int offset = (currentPosition - subElsPosition);

                    if (!_bigOffset && offset >= 65536) {
                        Console.WriteLine("Offset to element is bigger than 65536");
                    }

                    aWriter.Seek(subElsPositionCurrent, SeekOrigin.Begin);
                    if (_bigOffset) {
                        aWriter.Write(offset);
                    } else {
                        aWriter.Write((ushort) offset);
                    }
                    subElsPositionCurrent += _offsetSize;

                    aWriter.Seek(currentPosition, SeekOrigin.Begin);

                    if (aDef != null && aDef.childElements.ContainsKey(element.Name)) {
                        ProcessElement(currentPosition, element, aDef.childElements[element.Name], aWriter);
                    } else {
                        Console.WriteLine("Child elements for " + aElement.Name + " do not contain " + element.Name);
                    }
                }
            }
        }

        // ----------------------------------------------------------
        private int CountSubElements(XmlNodeList aNodes) {
            var count = 0;

            foreach (XmlNode node in aNodes) {
                if (node.NodeType == XmlNodeType.Element) {
                    ++count;
                }
            }

            return count;
        }

        // ----------------------------------------------------------
        private void WriteAttribute(string aValue, eAttribType aType, BinaryWriter aWriter) {
            switch (aType) {
                case eAttribType.none:
                    break;

                case eAttribType.str:
                    {
                        byte[] chars = Encoding.Default.GetBytes(aValue);
                        if (chars.Length > 255) {
                            Console.WriteLine("String '" + aValue + "' is longer than 255 chars");
                        }
                        // write number of chars
                        aWriter.Write((byte)chars.Length);
                        // write chars
                        aWriter.Write(chars);
                    }
                    break;

                case eAttribType.int8:
                    {
                        int value = int.Parse(aValue);
                        if (value > sbyte.MaxValue || value < sbyte.MinValue) {
                            Console.WriteLine("Value " + value + " is out of int8 range");
                        }
                        aWriter.Write((sbyte)value);
                    }
                    break;

                case eAttribType.uint8:
                    {
                        int value = int.Parse(aValue);
                        if (value > byte.MaxValue || value < byte.MinValue) {
                            Console.WriteLine("Value " + value + " is out of uint8 range");
                        }
                        aWriter.Write((byte)value);
                    }
                    break;

                case eAttribType.int16:
                    {
                        int value = int.Parse(aValue);
                        if (value > short.MaxValue || value < short.MinValue) {
                            Console.WriteLine("Value " + value + " is out of short range");
                        }
                        aWriter.Write((short)value);
                    }
                    break;

                case eAttribType.uint16:
                    {
                        int value = int.Parse(aValue);
                        if (value > ushort.MaxValue || value < ushort.MinValue) {
                            Console.WriteLine("Value " + value + " is out of ushort range");
                        }
                        aWriter.Write((ushort)value);
                    }
                    break;

                case eAttribType.int32:
                    {
                        int value = int.Parse(aValue);
                        aWriter.Write(value);
                    }
                    break;

                case eAttribType.uint32:
                    {
                        int value = int.Parse(aValue);
                        if (value < uint.MinValue) {
                            Console.WriteLine("Value " + value + " is out of uint range");
                        }
                        aWriter.Write((uint)value);
                    }
                    break;

                case eAttribType.fixed1_7:
                    {
                        int value = (int) (float.Parse(aValue, CultureInfo.InvariantCulture) * 128 + 0.5f);
                        if (value > byte.MaxValue || value < byte.MinValue) {
                            Console.WriteLine("Value " + value + " is out of 8bit range needed for fixed1_7");
                        }
                        aWriter.Write((byte)value);
                    }
                    break;

                case eAttribType.fixed8_8:
                    {
                        int value = (int) (float.Parse(aValue, CultureInfo.InvariantCulture) * 256 + 0.5f);
                        if (value > short.MaxValue || value < short.MinValue) {
                            Console.WriteLine("Value " + value + " is out of 16bit range needed for fixed8_8");
                        }
                        aWriter.Write((short)value);
                    }
                    break;

                case eAttribType.fixed16_16:
                    {
                        int value = (int) (float.Parse(aValue, CultureInfo.InvariantCulture) * 65536 + 0.5f);
                        if (value > int.MaxValue || value < int.MinValue) {
                            Console.WriteLine("Value " + value + " is out of 32bit range needed for fixed16_16");
                        }
                        aWriter.Write(value);
                    }
                    break;

                case eAttribType.boolean:
                    {
                        var value = aValue.ToLower() == "true" ? 1 : 0;
                        aWriter.Write((byte) value);
                    }
                    break;

                case eAttribType.curveType:
                    if (aValue == "linear") {
                        aWriter.Write((byte)0);
                    } else if (aValue == "instant") {
                        aWriter.Write((byte)1);
                    } else if (aValue == "quadratic") {
                        aWriter.Write((byte)2);
                    } else if (aValue == "cubic") {
                        aWriter.Write((byte)3);
                    } else {
                        Console.WriteLine("Unknown curve type: " + aValue);
                    }
                    break;

                case eAttribType.objInfoType:
                    if (aValue != "bone" && aValue != "sprite") {
                        Console.WriteLine("Unknown obj_info type type: " + aValue);
                    }
                    aWriter.Write((byte)(aValue == "bone" ? 1 : 0));
                    break;

                case eAttribType.timelineObjType:
                    if (aValue != "bone" && aValue != "sprite") {
                        Console.WriteLine("Unknown timeline object type: " + aValue);
                    }
                    aWriter.Write((byte)(aValue == "bone" ? 1 : 0));
                    break;
            }
        }
    }
}
