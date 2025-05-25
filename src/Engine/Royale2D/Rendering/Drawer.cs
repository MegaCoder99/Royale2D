using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Color = SFML.Graphics.Color;
using View = SFML.Graphics.View;

namespace Royale2D
{
    public struct DrawableWrapper
    {
        public Drawable drawable;
        public Texture? batchTexture;   // If set indicates to batch draw the drawable with others with same texture for perf
        public ShaderInstance? shaderInstance;
        public ZIndex zIndex;

        public DrawableWrapper(Drawable drawable, ZIndex zIndex, Texture? batchTexture, ShaderInstance? shaderInstance)
        {
            this.batchTexture = batchTexture;
            this.zIndex = zIndex;
            this.drawable = drawable;
            this.shaderInstance = shaderInstance;
        }
    }

    public enum UIQuality
    {
        Low,
        Medium,
        High
    };

    // PERF consider optimizing with pixijs's "re-render computation"
    public class Drawer
    {
        public View view;
        public List<DrawableWrapper> drawableWrappers = [];

        // Quality and scale should only ever be set for hud/menu drawing
        public UIQuality? uiQuality;
        public int scale = 1;
        
        // pos and GetScreenRect should only ever be called for world drawing
        public Point pos
        {
            get => new Point(view.Center.X, view.Center.Y);
            set => view.Center = new Vector2f(value.x, value.y);
        }
        public Rect GetScreenRect() => Rect.CreateWH(pos.x - Game.HalfScreenW, pos.y - Game.HalfScreenH, Game.ScreenW, Game.ScreenH);

        public Drawer(UIQuality? uiQuality = null)
        {
            this.uiQuality = uiQuality;
            if (uiQuality == UIQuality.Medium) scale = 2;
            if (uiQuality == UIQuality.High) scale = 4;
            view = CreateView();
        }

        public View CreateView()
        {
            Vector2f viewCenter = new Vector2f(Game.HalfScreenW * scale, Game.HalfScreenH * scale);
            var viewSize = new Vector2f(Game.ScreenW * scale, Game.ScreenH * scale);
            var view = new View(viewCenter, viewSize);
            view.Viewport = GetViewport();
            return view;
        }

        public FloatRect GetViewport()
        {
            float w = VideoMode.DesktopMode.Width;
            float h = VideoMode.DesktopMode.Height;

            if (!Options.main.fullScreen)
            {
                w = Game.windowW;
                h = Game.windowH;
            }

            float heightMultiple = h / Game.ScreenH;

            // if (Options.main.integerFullscreen) heightMultiple = MathF.Floor(desktopHeight / screenH);

            float extraWidthPercent = (w - Game.ScreenW * heightMultiple) / w;
            float extraHeightPercent = (h - Game.ScreenH * heightMultiple) / h;

            return new FloatRect(extraWidthPercent / 2f, extraHeightPercent / 2f, 1f - extraWidthPercent, 1f - extraHeightPercent);
        }

        public void RefreshViewport()
        {
            view.Viewport = GetViewport();
        }

        public void PreRender()
        {
            drawableWrappers.Clear();
        }

        public void PostRender()
        {
            Game.window.SetView(view);

            // This allows for stable sort to preserve order their draw() calls were called in of equal elements
            // REFACTOR could use StableSort() extension instead
            drawableWrappers = drawableWrappers.OrderBy(dw => dw.zIndex, new ZIndexComparer()).ToList();

            foreach (DrawableWrapper drawableWrapper in drawableWrappers)
            {
                if (drawableWrapper.shaderInstance != null)
                {
                    Game.window.Draw(drawableWrapper.drawable, new RenderStates(drawableWrapper.shaderInstance.GetShader()));
                }
                else
                {
                    Game.window.Draw(drawableWrapper.drawable);
                }
            }
        }

        private void DrawInternal(Drawable drawable, ZIndex zIndex = default, Texture? batchTexture = null, ShaderInstance? shaderInstance = null)
        {
            drawableWrappers.Add(new DrawableWrapper(drawable, zIndex, batchTexture, shaderInstance));
        }

        private void OffsetXY(ref float x, ref float y)
        {
            //x -= pos.x - Game.HalfScreenW;
            //y -= pos.y - Game.HalfScreenH;
            //x = MathF.Floor(x);
            //y = MathF.Floor(y);

            x *= scale;
            y *= scale;
        }

        public void DrawTexture(Texture texture, float x, float y, ZIndex zIndex = default, int? overrideScale = null, float alpha = 1)
        {
            OffsetXY(ref x, ref y);

            // PERF is creating a Sprite (a class) every frame performant? If this is an issue, a lot of other places are too (like lists, etc)
            var sprite = new SFML.Graphics.Sprite(texture);
            sprite.Position = new Vector2f(x, y);
            sprite.Scale = new Vector2f(overrideScale ?? scale, overrideScale ?? scale);
            sprite.Color = new Color(255, 255, 255, (byte)(alpha * 255));

            DrawInternal(sprite, zIndex, batchTexture: texture);
        }

        public void DrawTexture(string textureName, float x, float y, ZIndex zIndex = default, bool hasMediumQuality = false)
        {
            int? overrideScale = null;
            if (hasMediumQuality)
            {
                textureName += "_mq";
                if (uiQuality == UIQuality.Medium) overrideScale = 1;
                if (uiQuality == UIQuality.High) overrideScale = 2;
            }
            
            DrawTexture(Assets.textures[textureName], x, y, zIndex, overrideScale);
        }

        public void DrawTexture(string textureName, float x, float y, int sourceX, int sourceY, int sourceW, int sourceH, ZIndex zIndex = default)
        {
            OffsetXY(ref x, ref y);

            Texture texture = Assets.textures[textureName];
            var sprite = new SFML.Graphics.Sprite(texture);
            sprite.Position = new Vector2f(x, y);
            sprite.TextureRect = new SFML.Graphics.IntRect(sourceX, sourceY, sourceW, sourceH);
            sprite.Scale = new Vector2f(scale, scale);

            DrawInternal(sprite, zIndex, batchTexture: texture);
        }

        public void DrawTexture(Texture texture, float x, float y, IntRect sourceRect, ZIndex zIndex,
            float cx = 0, float cy = 0, float xScale = 1, float yScale = 1, float angle = 0, float alpha = 1, ShaderInstance? shaderInstance = null)
        {
            OffsetXY(ref x, ref y);
            xScale *= scale;
            yScale *= scale;

            var sprite = new SFML.Graphics.Sprite(texture);
            sprite.Position = new Vector2f(x, y);
            sprite.TextureRect = new SFML.Graphics.IntRect((int)sourceRect.x1, (int)sourceRect.y1, (int)sourceRect.w, (int)sourceRect.h);
            sprite.Origin = new Vector2f(cx, cy);
            sprite.Scale = new Vector2f(xScale, yScale);
            sprite.Rotation = angle;
            sprite.Color = new Color(255, 255, 255, (byte)(alpha * 255));

            DrawInternal(sprite, zIndex, shaderInstance: shaderInstance);
        }

        public void DrawLine(float x1, float y1, float x2, float y2, Color color, float thickness, ZIndex zIndex = default)
        {
            OffsetXY(ref x1, ref y1);
            OffsetXY(ref x2, ref y2);
            thickness *= scale;

            LineDrawable line = new LineDrawable(new Vector2f(x1, y1), new Vector2f(x2, y2), color, thickness);
            DrawInternal(line, zIndex: zIndex);
        }

        public void DrawCircle(float x, float y, float radius, bool filled, Color color, float thickness = 0, Color? outlineColor = null, uint? pointCount = null)
        {
            OffsetXY(ref x, ref y);
            radius *= scale;
            thickness *= scale;

            CircleShape circle = new CircleShape(radius);
            circle.Position = new Vector2f(x, y);
            circle.Origin = new Vector2f(radius, radius);
            if (filled)
            {
                circle.FillColor = color;
                if (outlineColor != null)
                {
                    circle.OutlineColor = (Color)outlineColor;
                    circle.OutlineThickness = thickness;
                }
            }
            else
            {
                circle.FillColor = Color.Transparent;
                circle.OutlineColor = color;
                circle.OutlineThickness = thickness;
            }
            if (pointCount != null)
            {
                circle.SetPointCount(pointCount.Value);
            }

            DrawInternal(circle);
        }

        public void DrawPixel(float x, float y, Color color, ZIndex zIndex = default)
        {
            OffsetXY(ref x, ref y);

            RectangleShape rect = new RectangleShape(new Vector2f(scale, scale));
            rect.Position = new Vector2f(x, y);
            rect.FillColor = color;

            DrawInternal(rect, zIndex);
        }

        public void DrawRect(float x1, float y1, float x2, float y2, bool filled, Color color, float thickness, Color? outlineColor = null, ZIndex zIndex = default)
        {
            OffsetXY(ref x1, ref y1);
            OffsetXY(ref x2, ref y2);
            thickness *= scale;

            RectangleShape rect = new RectangleShape(new Vector2f(x2 - x1, y2 - y1));
            rect.Position = new Vector2f(x1, y1);
            if (filled)
            {
                rect.FillColor = color;
                if (outlineColor != null)
                {
                    rect.OutlineColor = (Color)outlineColor;
                    rect.OutlineThickness = thickness;
                }
            }
            else
            {
                rect.FillColor = Color.Transparent;
                rect.OutlineColor = outlineColor ?? color;
                rect.OutlineThickness = thickness;
            }

            DrawInternal(rect, zIndex);
        }

        public void DrawRectWH(float x1, float y1, float w, float h, bool filled, Color color, int thickness, Color? outlineColor = null, ZIndex zIndex = default)
        {
            DrawRect(x1, y1, x1 + w, y1 + h, filled, color, thickness, outlineColor, zIndex);
        }

        public void DrawPolygon(List<IntPoint> points, Color color, bool fill, ZIndex zIndex = default)
        {
            DrawPolygon(points.Select(p => p.ToFloatPoint()).ToList(), color, fill, zIndex);
        }

        public void DrawPolygon(List<Point> points, Color color, bool fill, ZIndex zIndex = default)
        {
            ConvexShape shape = new ConvexShape((uint)points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                float x = points[i].x;
                float y = points[i].y;
                OffsetXY(ref x, ref y);
                shape.SetPoint((uint)i, new Vector2f(x, y));
            }
            if (fill)
            {
                shape.FillColor = color;
            }
            else
            {
                shape.OutlineColor = color;
                shape.OutlineThickness = 1;
            }

            DrawInternal(shape, zIndex);
        }

        public float DrawText(
            string text,
            float x,
            float y,
            AlignX alignX = AlignX.Left,
            AlignY alignY = AlignY.Top,
            FontType fontType = FontType.Normal,
            ZIndex zIndex = default,
            int letterSpacing = 0,
            bool batch = true)
        {
            OffsetXY(ref x, ref y);

            BitmapFont bitmapFont = BitmapFont.FromFontType(fontType);
            int fontScale = 1;
            if (scale > 1)
            {
                fontScale = scale;
            }

            int maxTextW = 0;

            int charH = bitmapFont.charHeight;
            int textW = 0;
            int textH = 0;
            int paddingX = bitmapFont.paddingX + letterSpacing;
            int paddingY = 2;

            var sprites = new List<SFML.Graphics.Sprite>();
            string[] lines = text.Split('\n');
            foreach (string line in lines)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    Dictionary<char, FontCharData> charMap = bitmapFont.charMap;
                    char c = line[i];
                    if (!charMap.ContainsKey(c)) c = '?';

                    int charW = charMap[c].width;
                    int position = charMap[c].index;
                    int xPos = ((position % bitmapFont.columnCount) * bitmapFont.cellSize);
                    int yPos = (position / bitmapFont.columnCount) * bitmapFont.cellSize;

                    var sprite = new SFML.Graphics.Sprite(bitmapFont.texture);
                    sprite.TextureRect = new SFML.Graphics.IntRect(xPos, yPos, charW, charH);
                    sprite.Position = new Vector2f(x + textW, y + textH);
                    sprites.Add(sprite);

                    textW += (charW + paddingX) * fontScale;
                }

                foreach (SFML.Graphics.Sprite sprite in sprites)
                {
                    var offset = new Point();

                    if (alignX == AlignX.Center) offset.x -= textW / 2;
                    if (alignX == AlignX.Right) offset.x -= textW;
                    if (alignY == AlignY.Middle) offset.y -= charH / 2;
                    if (alignY == AlignY.Bottom) offset.y -= charH;

                    sprite.Position += new Vector2f(offset.x, offset.y * fontScale);
                    sprite.Scale = new Vector2f(fontScale, fontScale);

                    DrawInternal(sprite, zIndex, batchTexture: batch ? bitmapFont.texture : null);
                }

                sprites.Clear();
                if (textW > maxTextW) maxTextW = textW;
                textW = 0;
                textH += (charH + paddingY) * fontScale;
            }

            return maxTextW / Game.windowScale;
        }
    }

    // Abstraction layer around text scale/quality and swapping of fonts
    public enum FontType
    {
        Small,
        Normal,
        NumberHUD,
        NumberItemBox,
        NumberWorld
    }
}
