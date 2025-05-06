using SFML.Graphics;
using SFML.System;
using Color = SFML.Graphics.Color;

namespace Royale2D
{
    public class LineDrawable : Drawable
    {
        public Vertex[] vertices = new Vertex[4];
        public float thickness = 1;
        public Color color = Color.Black;

        public LineDrawable(Vector2f point1, Vector2f point2, Color color, float thickness)
        {
            this.color = color;
            this.thickness = thickness;

            Vector2f direction = point2 - point1;
            Vector2f unitDirection = direction / MathF.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            Vector2f unitPerpendicular = new Vector2f(-unitDirection.Y, unitDirection.X);

            Vector2f offset = (thickness / 2.0f) * unitPerpendicular;

            vertices[0].Position = point1 + offset;
            vertices[1].Position = point2 + offset;
            vertices[2].Position = point2 - offset;
            vertices[3].Position = point1 - offset;

            for (int i = 0; i < 4; ++i)
                vertices[i].Color = color;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(vertices, PrimitiveType.Quads);
        }
    }
}
