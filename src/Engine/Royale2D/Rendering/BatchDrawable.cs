using SFML.Graphics;
using SFML.System;
using Color = SFML.Graphics.Color;

namespace Royale2D
{
    public class BatchDrawable : Transformable, Drawable
    {
        public VertexArray vertices;
        public Texture texture;

        public BatchDrawable(Texture texture)
        {
            vertices = new VertexArray();
            vertices.PrimitiveType = PrimitiveType.Quads;
            this.texture = texture;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            states.Transform *= Transform;
            states.Texture = texture;
            target.Draw(vertices, states);
        }

        public void AddSprite(SFML.Graphics.Sprite sprite)
        {
            float sx = sprite.TextureRect.Left;
            float sy = sprite.TextureRect.Top;
            float sw = sprite.TextureRect.Width;
            float sh = sprite.TextureRect.Height;
            float dx = sprite.Position.X;
            float dy = sprite.Position.Y;
            float scale = sprite.Scale.X;
            Color color = sprite.Color;

            float width = sw * scale;
            float height = sh * scale;

            Vertex vertex1 = new Vertex(new Vector2f(dx, dy), color);
            Vertex vertex2 = new Vertex(new Vector2f(dx, dy + height), color);
            Vertex vertex3 = new Vertex(new Vector2f(dx + width, dy + height), color);
            Vertex vertex4 = new Vertex(new Vector2f(dx + width, dy), color);

            vertex1.TexCoords = new Vector2f(sx, sy);
            vertex2.TexCoords = new Vector2f(sx, sy + sh);
            vertex3.TexCoords = new Vector2f(sx + sw, sy + sh);
            vertex4.TexCoords = new Vector2f(sx + sw, sy);

            vertices.Append(vertex1);
            vertices.Append(vertex2);
            vertices.Append(vertex3);
            vertices.Append(vertex4);
        }
    }
}
