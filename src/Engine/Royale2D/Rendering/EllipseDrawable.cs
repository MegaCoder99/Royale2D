using SFML.Graphics;
using SFML.System;
using Color = SFML.Graphics.Color;

namespace Royale2D
{
    public class EllipseDrawable : Drawable
    {
        private VertexArray vertices;
        private float centerX;
        private float centerY;

        public EllipseDrawable(float x, float y, float radiusX, float radiusY, Color color, uint pointCount = 30)
        {
            this.centerX = x;
            this.centerY = y;

            // Initialize the vertex array
            vertices = new VertexArray(PrimitiveType.TriangleFan, pointCount + 2);

            // Set the center point (needed for TriangleFan)
            vertices[0] = new Vertex(new Vector2f(x, y), color);

            // Calculate the points around the ellipse
            for (uint i = 1; i <= pointCount + 1; i++)
            {
                float angle = i * 2.0f * (float)Math.PI / pointCount;
                float pointX = centerX + radiusX * (float)Math.Cos(angle);
                float pointY = centerY + radiusY * (float)Math.Sin(angle);
                vertices[i] = new Vertex(new Vector2f(pointX, pointY), color);
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            // Use the target to draw the vertices
            target.Draw(vertices, states);
        }
    }
}
