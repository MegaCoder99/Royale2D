using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace Royale2D
{
    public class Gui
    {
        public Div rootDiv;
        public Gui(XmlDocument doc)
        {
            ProcessNode(doc.DocumentElement, null, null, out Node rootDivToInit);
            rootDiv = rootDivToInit as Div;
            rootDiv.SetChildrenWidthHeight(true);
            rootDiv.SetChildrenPos(true);
        }

        public static Gui FromFile(string filePath)
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            return new Gui(doc);
        }

        public static Gui FromString(string xmlString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            return new Gui(doc);
        }

        private void ProcessNode(XmlNode xmlNode, XmlNode? xmlParentNode, Div? parent, out Node node)
        {
            string id = "";
            NodeUnit x = NodeUnit.New("0");
            NodeUnit y = NodeUnit.New("0");
            int width = 0;
            int height = 0;
            AlignX hAlign = AlignX.Left;
            AlignY vAlign = AlignY.Top;
            List<Node> children = new List<Node>();
            string imagePath = "";
            string sprite = "";
            string text = "";
            int spacing = 0;
            bool hidden = false;

            if (xmlNode.Attributes != null)
            {
                foreach (XmlAttribute attr in xmlNode.Attributes)
                {
                    if (attr.Name == "id")
                    {
                        id = attr.Value;
                    }
                    else if (attr.Name == "x")
                    {
                        x = NodeUnit.New(attr.Value);
                    }
                    else if (attr.Name == "y")
                    {
                        y = NodeUnit.New(attr.Value);
                    }
                    else if (attr.Name == "width")
                    {
                        width = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "height")
                    {
                        height = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "halign")
                    {
                        if (attr.Value == "center") hAlign = AlignX.Center;
                        else if (attr.Value == "right") hAlign = AlignX.Right;
                    }
                    else if (attr.Name == "valign")
                    {
                        if (attr.Value == "center") vAlign = AlignY.Middle;
                        else if (attr.Value == "bottom") vAlign = AlignY.Bottom;
                    }
                    else if (attr.Name == "image")
                    {
                        imagePath = attr.Value;
                    }
                    else if (attr.Name == "sprite")
                    {
                        sprite = attr.Value;
                    }
                    else if (attr.Name == "text")
                    {
                        text = attr.Value;
                    }
                    else if (attr.Name == "spacing")
                    {
                        spacing = int.Parse(attr.Value);
                    }
                    else if (attr.Name == "hidden")
                    {
                        hidden = bool.Parse(attr.Value);
                    }
                }
            }

            if (xmlNode.Name == "div")
            {
                node = new Div(id, x, y, width, height, hAlign, vAlign, parent, imagePath, DivType.Absolute, children, spacing);
            }
            else if (xmlNode.Name == "hdiv")
            {
                node = new Div(id, x, y, width, height, hAlign, vAlign, parent, imagePath, DivType.Horizontal, children, spacing);
            }
            else if (xmlNode.Name == "vdiv")
            {
                node = new Div(id, x, y, width, height, hAlign, vAlign, parent, imagePath, DivType.Vertical, children, spacing);
            }
            else if (xmlNode.Name == "image")
            {
                node = new ImageNode(id, x, y, width, height, hAlign, vAlign, parent, imagePath, sprite);
            }
            else if (xmlNode.Name == "smalltext")
            {
                node = new TextNode(id, x, y, width, height, hAlign, vAlign, parent, text, FontType.Small);
            }
            else if (xmlNode.Name == "numbertext")
            {
                node = new TextNode(id, x, y, width, height, hAlign, vAlign, parent, text, FontType.NumberHUD);
            }
            else if (xmlNode.Name == "text")
            {
                node = new TextNode(id, x, y, width, height, hAlign, vAlign, parent, text, FontType.Normal);
            }
            else
            {
                throw new Exception($"Unknown node type: {xmlNode.Name}");
            }

            node.hidden = hidden;

            // Recursive call for each child node
            foreach (XmlNode child in xmlNode.ChildNodes)
            {
                if (node is not Div) throw new Exception("Node has children but isn't a div");
                ProcessNode(child, xmlNode, node as Div, out Node childNode);
                children.Add(childNode);
            }
        }

        public void Render(Drawer drawer)
        {
            foreach (Node child in rootDiv.children)
            {
                child.Render(drawer);
            }
        }

        public Node? GetNodeById(string id)
        {
            return rootDiv.GetNodeById(id);
        }

        public List<Node> GetNodesById(string id)
        {
            var returnNodes = new List<Node>();
            rootDiv.GetNodesById(id, returnNodes);
            return returnNodes;
        }

        public void SetNodeText(string id, string text)
        {
            List<Node> textNodes = GetNodesById(id);
            foreach (Node node in textNodes)
            {
                if (node is TextNode textNode)
                {
                    textNode.text = text;
                }
            }
        }

        public void SetNodeSprite(string id, string spriteName)
        {
            List<Node> textNodes = GetNodesById(id);
            foreach (Node node in textNodes)
            {
                if (node is ImageNode imageNode)
                {
                    imageNode.spriteName = spriteName;
                }
            }
        }
    }

    public class Node
    {
        public string id;
        public NodeUnit x;
        public NodeUnit y;
        public int width;
        public int height;
        public AlignX hAlign;
        public AlignY vAlign;
        public Div? parent;
        public bool hidden;

        // Computed after initial xml parsing/population
        public IntPoint? offset;

        public Node(string id, NodeUnit x, NodeUnit y, int width, int height, AlignX hAlign, AlignY vAlign, Div? parent)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.hAlign = hAlign;
            this.vAlign = vAlign;
            this.parent = parent;
        }

        public Point GetPos()
        {
            if (parent != null)
            {
                var parentPos = parent.GetPos();
                float posX = x.GetValue(parent.width) + offset.Value.x + parentPos.x;
                float posY = y.GetValue(parent.height) + offset.Value.y + parentPos.y;
                return new Point(posX, posY);
            }
            else
            {
                return new Point(0, 0);
            }
        }

        public virtual void Render(Drawer drawer)
        {
        }
    }

    public class Div : Node
    {
        public string imagePath;
        public DivType type;
        public List<Node> children;
        public int spacing;

        public Div(string id, NodeUnit x, NodeUnit y, int width, int height, AlignX hAlign, AlignY vAlign, Div? parent, string imagePath, DivType type, List<Node> children, int spacing)
            : base(id, x, y, width, height, hAlign, vAlign, parent)
        {
            this.imagePath = imagePath;
            this.type = type;
            this.children = children;
            this.spacing = spacing;
        }

        public Node? GetNodeById(string id)
        {
            foreach (var node in children)
            {
                if (node.id == id)
                {
                    return node;
                }
                else if (node is Div divNode)
                {
                    Node? foundNode = divNode.GetNodeById(id);
                    if (foundNode != null)
                    {
                        return foundNode;
                    }
                }
            }
            return null;
        }

        public void GetNodesById(string id, List<Node> returnNodes)
        {
            foreach (var node in children)
            {
                if (node.id == id)
                {
                    returnNodes.Add(node);
                }
                else if (node is Div divNode)
                {
                    divNode.GetNodesById(id, returnNodes);
                }
            }
        }

        public void SetChildrenWidthHeight(bool isRoot)
        {
            int maxWidth = 0;
            int maxHeight = 0;
            for (int i = 0; i < children.Count; i++)
            {
                Node child = children[i];

                if (child is Div childDiv)
                {
                    childDiv.SetChildrenWidthHeight(false);
                }

                if (type == DivType.Horizontal)
                {
                    if (child.height > maxHeight)
                    {
                        maxHeight = child.height;
                    }
                    maxWidth += child.width + spacing;
                }
                else if (type == DivType.Vertical)
                {
                    if (child.width > maxWidth)
                    {
                        maxWidth = child.width;
                    }
                    maxHeight += child.height + spacing;
                }
            }

            if (!isRoot)
            {
                width = maxWidth;
                height = maxHeight;
            }
        }

        public void SetChildrenPos(bool isRoot)
        {
            if (isRoot) offset = IntPoint.Zero;

            int offX = offset.Value.x;
            int offY = offset.Value.y;
            for (int i = 0; i < children.Count; i++)
            {
                Node child = children[i];
                child.offset = new IntPoint(offX, offY);

                if (child is Div childDiv)
                {
                    childDiv.SetChildrenPos(false);
                }

                if (type == DivType.Horizontal)
                {
                    offX += child.width + spacing;
                }
                else if (type == DivType.Vertical)
                {
                    offY += child.height + spacing;
                }
            }
        }

        public override void Render(Drawer drawer)
        {
            if (hidden) return;
            foreach (Node child in children)
            {
                child.Render(drawer);
            }
        }
    }

    public class TextNode : Node
    {
        public string text;
        public FontType fontType;

        public TextNode(string id, NodeUnit x, NodeUnit y, int width, int height, AlignX hAlign, AlignY vAlign, Div? parent, string text, FontType fontType)
            : base(id, x, y, width, height, hAlign, vAlign, parent)
        {
            this.text = text;
            this.fontType = fontType;
            BitmapFont bitmapFont = BitmapFont.FromFontType(fontType);
            this.width = width == 0 ? (text.Length * bitmapFont.maxCharWidth) : width;
            this.height = height == 0 ? bitmapFont.charHeight : height;
        }

        public override void Render(Drawer drawer)
        {
            if (hidden) return;
            Point pos = GetPos();
            drawer.DrawText(text ?? "", pos.x, pos.y, alignX: hAlign, alignY: vAlign, fontType: fontType);
        }
    }

    public class ImageNode : Node
    {
        public string imagePath;
        public string spriteName;

        public ImageNode(string id, NodeUnit x, NodeUnit y, int width, int height, AlignX hAlign, AlignY vAlign, Div? parent, string imagePath, string spriteName)
            : base(id, x, y, width, height, hAlign, vAlign, parent)
        {
            this.imagePath = imagePath;
            this.spriteName = spriteName;
            if (spriteName != "")
            {
                Sprite sprite = Assets.GetSprite(spriteName);
                this.width = width == 0 ? sprite.frames[0].rect.w : width;
                this.height = height == 0 ? sprite.frames[0].rect.h : height;
            }
        }

        public override void Render(Drawer drawer)
        {
            if (hidden) return;

            Sprite sprite = Assets.GetSprite(spriteName);
            Point pos = GetPos();
            int xOff = 0;
            if (hAlign == AlignX.Center)
            {
                xOff = width / 2;
            }
            else if (hAlign == AlignX.Right)
            {
                xOff = width;
            }

            int yOff = 0;
            if (vAlign == AlignY.Middle)
            {
                yOff = height / 2;
            }
            else if (vAlign == AlignY.Bottom)
            {
                yOff = height;
            }

            sprite.Render(drawer, pos.x - xOff, pos.y - yOff, 0);
        }
    }

    #region helper types
    public enum DivType
    {
        Absolute,
        Vertical,
        Horizontal
    }

    public struct NodeUnit
    {
        public int value;
        public bool isPercentage;

        public NodeUnit(int value, bool isPercentage)
        {
            this.value = value;
            this.isPercentage = isPercentage;
        }

        public static NodeUnit New(string value)
        {
            if (value.EndsWith("%"))
            {
                return new NodeUnit(int.Parse(value.Substring(0, value.Length - 1)), true);
            }
            else
            {
                return new NodeUnit(int.Parse(value), false);
            }
        }

        public int GetValue(int parentSize)
        {
            if (isPercentage)
            {
                return (int)((value / 100f) * parentSize);
            }
            return value;
        }
    }
    #endregion
}
