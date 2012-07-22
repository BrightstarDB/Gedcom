using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;

/*
 * The original code for the parser was created by Aaron Skonnard and can
 * be found online at http://msdn.microsoft.com/en-us/magazine/cc188730.aspx */

namespace BrightstarDB.Gedcom
{
    public interface IGedcomNode
    {
        string Name { get; }
        string Value { get; }
        int LineNumber { get; }
        XmlNodeType NodeType { get; }
    }

    public class GedcomAttribute : IGedcomNode
    {
        private string name;
        private string val;
        private int lineNumber;

        public GedcomAttribute(string name, string val, int lineNumber)
        {
            this.name = name;
            this.val = val;
            this.lineNumber = lineNumber;
        }
        public XmlNodeType NodeType
        {
            get { return XmlNodeType.Attribute; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Value
        {
            get { return val; }
        }

        public int LineNumber
        {
            get { return lineNumber; }
        }
    }

    public class GedcomElement : IGedcomNode
    {
        private string name;
        private int attributeCursor = -1;
        private bool onAttributeText = false;
        private int lineNumber;

        private ArrayList attributes = new ArrayList();

        public GedcomElement(string name, int lineNumber)
        {
            this.name = name;
            this.lineNumber = lineNumber;
        }

        public string Name
        {
            get
            {
                if (IsText)
                    return "#text";
                else if (IsAttribute)
                    return GetAttributeNode(attributeCursor).Name;
                else
                    return name;
            }
        }

        public string Value
        {
            get
            {
                if (IsText || IsAttribute)
                    return GetAttributeNode(attributeCursor).Value;
                else
                    return null;
            }
        }

        public XmlNodeType NodeType
        {
            get
            {
                if (IsText)
                    return XmlNodeType.Text;
                else if (IsAttribute)
                    return XmlNodeType.Attribute;
                else
                    return XmlNodeType.Element;
            }
        }

        public bool IsText
        {
            get { return IsAttribute && onAttributeText; }
        }

        public bool IsAttribute
        {
            get
            {
                if (attributes.Count > 0 && attributeCursor >= 0)
                    return true;
                else
                    return false;
            }
        }

        public void AddAttribute(GedcomAttribute att)
        {
            attributes.Add(att);
        }

        public GedcomAttribute GetAttributeNode(int i)
        {
            return attributes[i] as GedcomAttribute;
        }
        public string GetAttribute(int i)
        {
            GedcomAttribute a = attributes[i] as GedcomAttribute;
            return a.Value;
        }
        public string GetAttribute(string name)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                if (GetAttributeNode(i).Name.Equals(name))
                    return GetAttribute(i);
            }
            return null;
        }
        public string this[int i]
        {
            get { return GetAttribute(i); }
        }

        public bool MoveToAttribute(int i)
        {
            onAttributeText = false;
            if (attributes.Count > 0 && i >= 0 && i < attributes.Count)
            {
                attributeCursor = i;
                return true;
            }
            return false;
        }
        public bool MoveToAttribute(string name)
        {
            onAttributeText = false;
            for (int i = 0; i < attributes.Count; i++)
            {
                if (GetAttributeNode(i).Name.Equals(name))
                    attributeCursor = i;
            }
            return false;
        }
        public bool MoveToFirstAttribute()
        {
            onAttributeText = false;
            if (attributes.Count == 0)
                return false;
            attributeCursor = 0;
            return true;
        }
        public bool MoveToNextAttribute()
        {
            onAttributeText = false;
            if (attributes.Count == 0 || (attributes.Count - 1 == attributeCursor))
                return false;
            attributeCursor++;
            return true;
        }
        public void MoveToAttributeText()
        {
            onAttributeText = true;
        }
        public int AttributeCount
        {
            get { return attributes.Count; }
        }
        public bool MoveToElement()
        {
            onAttributeText = false;
            attributeCursor = -1;
            return true;
        }
        public int LineNumber
        {
            get { return lineNumber; }
        }
    }
    public class GedcomEndElement : IGedcomNode
    {
        private int lineNumber;
        public GedcomEndElement(int lineNumber)
        {
            this.lineNumber = lineNumber;
        }
        public string Name
        {
            get { return "#endelement"; }
        }
        public string Value
        {
            get { return null; }
        }

        public XmlNodeType NodeType
        {
            get { return XmlNodeType.EndElement; }
        }
        public int LineNumber
        {
            get { return lineNumber; }
        }
    }
    public class GedcomText : IGedcomNode
    {
        private string val;
        private int lineNumber;

        public GedcomText(string val, int lineNumber)
        {
            this.val = val;
            this.lineNumber = lineNumber;
        }

        public string Name
        {
            get { return "#text"; }
        }

        public XmlNodeType NodeType
        {
            get { return XmlNodeType.Text; }
        }

        public string Value
        {
            get { return val; }
        }

        public int LineNumber
        {
            get { return lineNumber; }
        }
    }

    public class GedcomLine
    {
        private int cursor = 0;
        private int lineNumber;
        private ArrayList nodes = new ArrayList();

        public GedcomLine(int lineNumber)
        {
            this.lineNumber = lineNumber;
        }
        public void Add(IGedcomNode node)
        {
            nodes.Add(node);
        }
        public IGedcomNode Current
        {
            get
            {
                if (nodes.Count == 0)
                    return null;
                return nodes[cursor] as IGedcomNode;
            }
        }
        public bool MoveNext()
        {
            if (nodes.Count == 0)
                return false;
            if (cursor == nodes.Count - 1)
                return false;
            cursor++;
            return true;
        }
        public bool EOF
        {
            get
            {
                if (nodes.Count == 0)
                    return true;
                else
                    return cursor == nodes.Count - 1;
            }
        }
        public int LineNumber
        {
            get { return lineNumber; }
        }
    }


    public class GedcomReader : XmlReader
    {
        private string gedcomFileName;
        private StreamReader fileReader;
        private ReadState readState;
        private Stack nodeStack = new Stack();
        private string currentLineText;
        private GedcomLine currentLineNodes;
        private NameTable nameTable = new NameTable();

        public GedcomReader(string gedcomFileName)
        {
            this.gedcomFileName = gedcomFileName;
            fileReader = new StreamReader(gedcomFileName);
            readState = ReadState.Initial;
        }

        public override void Close()
        {
            fileReader.Close();
            readState = ReadState.Closed;
        }

        public override string GetAttribute(int i)
        {
            if (GetCurrentNode().NodeType == XmlNodeType.Element)
            {
                GedcomElement e = GetCurrentNode() as GedcomElement;
                return e.GetAttribute(i);
            }
            else return null;
        }

        public override string GetAttribute(string name, string ns)
        {
            if (ns.Equals(""))
                return GetAttribute(name);
            else
                return null;
        }

        public override string GetAttribute(string name)
        {
            if (GetCurrentNode().NodeType == XmlNodeType.Element)
            {
                GedcomElement e = GetCurrentNode() as GedcomElement;
                return e.GetAttribute(name);
            }
            return null;
        }

        public override string LookupNamespace(string prefix)
        {
            return String.Empty;
        }

        public override void MoveToAttribute(int i)
        {
            if (GetCurrentNode().NodeType == XmlNodeType.Element)
            {
                GedcomElement e = GetCurrentNode() as GedcomElement;
                e.MoveToAttribute(i);
            }
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            if (ns.Equals(""))
                return MoveToAttribute(name);
            else
                return false;
        }

        public override bool MoveToAttribute(string name)
        {
            if (GetCurrentNode().NodeType == XmlNodeType.Element)
            {
                GedcomElement e = GetCurrentNode() as GedcomElement;
                return e.MoveToAttribute(name);
            }
            return false;
        }

        public override bool MoveToElement()
        {
            if (GetCurrentNode().NodeType == XmlNodeType.Element)
            {
                GedcomElement e = GetCurrentNode() as GedcomElement;
                return e.MoveToElement();
            }
            else
                return false;
        }

        public override bool MoveToFirstAttribute()
        {
            if (GetCurrentNode().NodeType == XmlNodeType.Element)
            {
                GedcomElement e = GetCurrentNode() as GedcomElement;
                return e.MoveToFirstAttribute();
            }
            else
                return false;
        }

        public override bool MoveToNextAttribute()
        {
            if (GetCurrentNode().NodeType == XmlNodeType.Element)
            {
                GedcomElement e = GetCurrentNode() as GedcomElement;
                return e.MoveToNextAttribute();
            }
            else
                return false;
        }

        public override string this[string name, string namespaceURI]
        {
            get { return GetAttribute(name, namespaceURI); }
        }

        public override string this[string name]
        {
            get { return GetAttribute(name); }
        }

        public override string this[int i]
        {
            get { return GetAttribute(i); }
        }

        private string GetRemainingValue(string[] parts, int index)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = index; i < parts.Length; i++)
            {
                sb.Append(parts[i]);
                if (i != parts.Length - 1)
                    sb.Append(' ');
            }
            return sb.ToString();
        }

        private GedcomLine ParseGedcomLine(string lineText)
        {
            string xref_id = "", tag = "", pointer = "", linevalue = "";
            string[] lineParts = lineText.Split(' ');
            int nextPart = 0;

            if (lineParts.Length < 2)
                throw new Exception("invalid GEDCOM line");

            // first part is always the line number
            int lineNumber = int.Parse(lineParts[nextPart++]);
            if (lineParts[nextPart].StartsWith("@"))
            {
                if (lineParts.Length < 3)
                    throw new Exception("invalid GEDCOM line");
                // this is an xref_id, next part is the tag
                xref_id = lineParts[nextPart++].Replace("@", "");
                tag = lineParts[nextPart++];
            }
            else
                // no xref_id, first part is tag
                tag = lineParts[nextPart++];

            if (lineParts.Length > nextPart)
            {
                if (lineParts[nextPart].StartsWith("@"))
                    pointer = lineParts[nextPart++].Replace("@", "");
            }
            if (lineParts.Length > nextPart)
                linevalue = GetRemainingValue(lineParts, nextPart);

            GedcomLine line = new GedcomLine(lineNumber);
            GedcomElement e = new GedcomElement(tag, lineNumber);
            if (xref_id != "")
                e.AddAttribute(new GedcomAttribute("id", xref_id, lineNumber));
            if (pointer != "")
                e.AddAttribute(new GedcomAttribute("idref", pointer, lineNumber));
            line.Add(e);
            if (linevalue != "")
            {
                e.AddAttribute(new GedcomAttribute("value", linevalue, lineNumber));
                //GedcomText t = new GedcomText(linevalue, lineNumber);
                //line.Add(t);
            }
            return line;
        }

        public override bool Read()
        {
            try
            {
                switch (readState)
                {
                    case ReadState.Initial:
                        {
                            nodeStack.Push(new GedcomElement("GEDCOM", -1));
                            readState = ReadState.Interactive;
                            return true;
                        }
                    case ReadState.Interactive:
                        {
                            if (GetCurrentNode() is GedcomElement)
                            {
                                GedcomElement ge = GetCurrentNode() as GedcomElement;
                                if (ge != null && ge.IsAttribute || ge.IsText)
                                    ge.MoveToElement();
                            }

                            if (GetCurrentNode().NodeType == XmlNodeType.EndElement)
                            {
                                // pop until you hit an element node
                                while (((IGedcomNode)nodeStack.Pop()).NodeType != XmlNodeType.Element) ;

                                int curLineNumber = GetCurrentLineNumber();
                                if (currentLineNodes.LineNumber <= curLineNumber)
                                    // need to close previous element
                                    nodeStack.Push(new GedcomEndElement(curLineNumber));
                                else
                                    // just push new elements, they're children
                                    nodeStack.Push(currentLineNodes.Current);
                            }
                            else if (currentLineNodes != null && !currentLineNodes.EOF)
                            {
                                currentLineNodes.MoveNext();
                                nodeStack.Push(currentLineNodes.Current);
                            }
                            else
                            {
                                // need to parse a new line
                                currentLineText = fileReader.ReadLine();
                                // detect EOF
                                if (currentLineText == null)
                                {
                                    readState = ReadState.EndOfFile;
                                    return false;
                                }
                                // parse text into logical nodes
                                currentLineNodes = ParseGedcomLine(currentLineText);
                                // see if we need to insert end element
                                int curLineNumber = GetCurrentLineNumber();
                                if (currentLineNodes.LineNumber <= curLineNumber)
                                    // need to close previous element
                                    nodeStack.Push(new GedcomEndElement(curLineNumber));
                                else
                                    // just push new elements, they're children
                                    nodeStack.Push(currentLineNodes.Current);
                            }
                            return true;
                        }
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                readState = ReadState.Error;
                throw e;
            }
        }

        public override bool ReadAttributeValue()
        {
            if (GetCurrentNode().NodeType == XmlNodeType.Attribute)
            {
                GedcomElement e = GetCurrentNode() as GedcomElement;
                e.MoveToAttributeText();
                return true;
            }
            return false;
        }

        public override void ResolveEntity()
        {
            // do nothing
        }

        public override int AttributeCount
        {
            get
            {
                if (GetCurrentNode().NodeType == XmlNodeType.Element)
                {
                    GedcomElement e = GetCurrentNode() as GedcomElement;
                    return e.AttributeCount;
                }
                return 0;
            }
        }

        public override string BaseURI
        {
            get { return String.Empty; }
        }

        public override int Depth
        {
            get
            {
                return nodeStack.Count - 1;
            }
        }

        public override bool EOF
        {
            get { return readState == ReadState.EndOfFile; }
        }

        public override bool HasValue
        {
            get
            {
                if (readState != ReadState.Interactive)
                    return false;
                switch (GetCurrentNodeType())
                {
                    case XmlNodeType.Attribute:
                    case XmlNodeType.Text:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public override bool IsDefault
        {
            get { return false; }
        }

        public override bool IsEmptyElement
        {
            get { return false; }
        }

        public override string LocalName
        {
            get
            {
                if (readState != ReadState.Interactive)
                    return String.Empty;
                IGedcomNode node = nodeStack.Peek() as IGedcomNode;
                return node.Name;
            }
        }

        public override string Name
        {
            get
            {
                if (readState != ReadState.Interactive)
                    return String.Empty;
                IGedcomNode node = nodeStack.Peek() as IGedcomNode;
                return node.Name;
            }
        }

        public override string NamespaceURI
        {
            get { return String.Empty; }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return nameTable;
            }
        }

        private IGedcomNode GetCurrentNode()
        {
            return nodeStack.Peek() as IGedcomNode;
        }

        private XmlNodeType GetCurrentNodeType()
        {
            IGedcomNode cur = GetCurrentNode();
            if (cur == null)
                return XmlNodeType.None;
            else
                return cur.NodeType;
        }
        private int GetCurrentLineNumber()
        {
            IGedcomNode cur = GetCurrentNode();
            if (cur == null)
                return 0;
            else
                return cur.LineNumber;
        }

        public override XmlNodeType NodeType
        {
            get
            {
                if (readState != ReadState.Interactive)
                    return XmlNodeType.None;
                return GetCurrentNodeType();
            }
        }

        public override string Prefix
        {
            get { return String.Empty; }
        }

        public override char QuoteChar
        {
            get { return '"'; }
        }

        public override ReadState ReadState
        {
            get { return readState; }
        }

        public override string Value
        {
            get
            {
                IGedcomNode node = nodeStack.Peek() as IGedcomNode;
                return node.Value;
            }
        }

        public override string XmlLang
        {
            get { return String.Empty; }
        }

        public override XmlSpace XmlSpace
        {
            get { return XmlSpace.None; }
        }
    }
}


