using SFML.Graphics;
using Shared;

namespace Royale2D
{
    public class FontCharData
    {
        public int index;
        public int width;

        public FontCharData(int index, int width)
        {
            this.index = index;
            this.width = width;
        }
    }

    public class BitmapFont
    {
        public string textureName;
        public int cellSize;
        public int columnCount;
        public int maxCharWidth;
        public int charHeight;
        public int paddingX;
        public Dictionary<char, FontCharData> charMap = new Dictionary<char, FontCharData>();
        public Texture texture => Assets.textures[textureName];

        public BitmapFont(string textureName, int cellSize, int columnCount, int maxCharWidth, int charHeight, Dictionary<char, FontCharData> charMap, int paddingX = 0)
        {
            this.textureName = textureName;
            this.cellSize = cellSize;
            this.columnCount = columnCount;
            this.maxCharWidth = maxCharWidth;
            this.charHeight = charHeight;
            this.charMap = charMap;
            this.paddingX = paddingX;
        }

        public static BitmapFont FromFontType(FontType fontType)
        {
            if (fontType == FontType.Small)
            {
                return Assets.bitmapFonts["small"];
            }
            else if (fontType == FontType.NumberHUD)
            {
                return Assets.bitmapFonts["number_hud"];
            }
            else if (fontType == FontType.NumberWorld)
            {
                return Assets.bitmapFonts["number_world"];
            }
            else
            {
                return Assets.bitmapFonts["menu"];
            }
        }
    }
}
