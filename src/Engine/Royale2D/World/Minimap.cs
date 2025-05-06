using SFML.Graphics;
using SFML.System;
using Color = SFML.Graphics.Color;

namespace Royale2D
{
    public class Minimap
    {
        Storm storm;

        int minimapWidth;
        int minimapHeight;

        float startX;
        float startY;

        // All these "actual" variables represent the actual representation of map pixels and not the padding
        int actualMinimapWidth;
        int actualMinimapHeight;
        float actualMinimapRatio;

        string renderTextureName;

        /*
        public float msFlashTime = 0;
        public int msFlashIndex = 0;
        public float msBlinkTime = 0;
        public bool msBlink = false;
        */

        TextureManager textureManager => storm.world.textureManager;
        RenderTexture renderTexture => textureManager.GetRenderTexture(renderTextureName);

        /*
        - mapWidth: the width of the entire map in pixels (# tile cols * 8)
        - mapHeight: the height of the entire map in pixels (# tile rows * 8)
        - minimapWidth: the width of the entire minimap in pixels
        - minimapHeight: the height of the entire minimap in pixels
        - startX / startY / endX / endY: 
            - The map image itself may have some padding around its edges that depict cliffs/clouds that are not representative of actual map pixels.
            - That's what these set of coordinates are for. They represent the rectanglar area inside the minimap that are the actual representation of map pixels.
            - To simplify an already complex system, we enforce a requirement that these 4 coords must create a rect whose width/height matches ratio of map itself
        */
        public Minimap(int mapWidth, int mapHeight, int minimapWidth, int minimapHeight, int startX, int startY, int endX, int endY, Storm storm, string renderTextureName)
        {
            this.minimapWidth = minimapWidth;
            this.minimapHeight = minimapHeight;

            this.startX = startX;
            this.startY = startY;

            this.actualMinimapWidth = endX - startX;
            this.actualMinimapHeight = endY - startY;

            this.actualMinimapRatio = actualMinimapWidth / (float)mapWidth;

            if (!Helpers.AreClose(actualMinimapWidth / (float)actualMinimapHeight, mapWidth / (float)mapHeight, 0.1f))
            {
                throw new Exception("start/end x/y do not match minimap ratio.");
            }

            this.storm = storm;
            this.renderTextureName = renderTextureName;
            textureManager.AddRenderTexture(renderTextureName, (uint)minimapWidth, (uint)minimapHeight);

        }

        public void Update()
        {
            /*
            msBlinkTime += Game.spfConst;
            if (msBlinkTime > 0.5)
            {
                msBlinkTime = 0;
                msBlink = !msBlink;
            }
            */
        }

        public void Render(Drawer drawer, int x, int y, Action? additionalRendering = null)
        {
            renderTexture.Clear(Color.Transparent);

            RenderStates states = new RenderStates(renderTexture.Texture);
            states.BlendMode = new BlendMode(BlendMode.Factor.One, BlendMode.Factor.One, BlendMode.Equation.Subtract);

            // Initial storm rect
            RectangleShape rect = new RectangleShape(new Vector2f(minimapWidth, minimapHeight));
            rect.FillColor = Colors.StormColor;
            renderTexture.Draw(rect, states);

            // The current safe zone. It is a circle "scooped out" of the rectangle above
            CircleShape circle1 = new CircleShape(MathF.Ceiling(storm.currentStormRadiusFloat * actualMinimapRatio));
            circle1.FillColor = Colors.StormColor;
            circle1.Origin = new Vector2f(circle1.Radius, circle1.Radius);
            Point minimapStormCenter = GetMinimapPos(storm.currentStormCenter.ToFloatPoint());
            circle1.Position = new Vector2f(minimapStormCenter.x, minimapStormCenter.y);
            renderTexture.Draw(circle1, states);

            // Red outline of the current safe zone
            circle1.OutlineThickness = 1;
            circle1.OutlineColor = Color.Red;
            circle1.FillColor = Color.Transparent;
            renderTexture.Draw(circle1);

            // A black circle outline representing the next safe zone
            CircleShape circle3 = new CircleShape(MathF.Ceiling(storm.nextStormRadius.floatVal * actualMinimapRatio));
            circle3.OutlineThickness = 1;
            circle3.OutlineColor = Color.Black;
            circle3.FillColor = Color.Transparent;
            circle3.Origin = new Vector2f(circle3.Radius, circle3.Radius);
            Point nextStormCenter = GetMinimapPos(storm.nextStormCenter.ToFloatPoint());
            circle3.Position = new Vector2f(nextStormCenter.x, nextStormCenter.y);
            renderTexture.Draw(circle3);

            additionalRendering?.Invoke();

            renderTexture.Display();

            drawer.DrawTexture(renderTexture.Texture, x, y);
        }

        public void DrawSpriteOnMinimap(string spriteName, Point mapPos)
        {
            Sprite sprite = Assets.GetSprite(spriteName);
            IntRect spriteRect = sprite.frames[0].rect;
            var sfmlSprite = new SFML.Graphics.Sprite(sprite.frames[0].texture, new SFML.Graphics.IntRect(spriteRect.x1, spriteRect.y1, spriteRect.w, spriteRect.h));
            var minimapPos = GetMinimapPos(mapPos);
            sfmlSprite.Origin = new Vector2f(spriteRect.w / 2, spriteRect.h / 2);
            sfmlSprite.Position = new Vector2f(minimapPos.x, minimapPos.y);
            renderTexture.Draw(sfmlSprite);
        }

        public Point GetMinimapPos(Point mapPos)
        {
            float x = startX + (mapPos.x * actualMinimapRatio);
            float y = startY + (mapPos.y * actualMinimapRatio);

            return new Point(x, y);
        }
    }
}
