using Shared;

namespace Royale2D
{
    public class Camera
    {
        public Point targetPos;
        public WorldSection section;
        public bool enabled;

        public Drawer drawer => Game.worldDrawer;
        public Point pos => drawer.pos;

        public Camera(Point targetPos, WorldSection section)
        {
            this.targetPos = targetPos;
            this.section = section;
        }

        public void Update(Point targetPos, WorldSection section)
        {
            this.targetPos = targetPos;
            this.section = section;

            // Snap to map section bounds
            drawer.pos = new Point(
                MyMath.Clamp(targetPos.x, Game.HalfScreenW, section.mapSection.pixelWidth - Game.HalfScreenW),
                MyMath.Clamp(targetPos.y, Game.HalfScreenH, section.mapSection.pixelHeight - Game.HalfScreenH)
            );

            // Snap out of no scroll areas
            foreach (PixelZone noScrollZone in section.mapSection.noScrollZones)
            {
                Rect noScrollRect = noScrollZone.rect;
                Rect camRect = drawer.GetScreenRect();
                if (camRect.Overlaps(noScrollRect, false))
                {
                    float? overlapX = camRect.GetOverlapX(noScrollRect);
                    float? overlapY = camRect.GetOverlapY(noScrollRect);

                    if (overlapX != null && overlapY != null)
                    {
                        if (Math.Abs(overlapX.Value) < Math.Abs(overlapY.Value))
                        {
                            drawer.pos = drawer.pos.AddXY(overlapX.Value, 0);
                        }
                        else
                        {
                            drawer.pos = drawer.pos.AddXY(0, overlapY.Value);
                        }
                    }
                    else if (overlapX != null)
                    {
                        drawer.pos = drawer.pos.AddXY(overlapX.Value, 0);
                    }
                    else if (overlapY != null)
                    {
                        drawer.pos = drawer.pos.AddXY(0, overlapY.Value);
                    }
                }
            }

            // Add shake amount if any
            Point shakeAmount = GetShakeAmount();
            drawer.pos += shakeAmount;
        }

        public void Render()
        {
            if (enabled)
            {
                section.Render(drawer);
            }
        }

        public Point GetShakeAmount()
        {
            Point totalShakePower = Point.Zero;
            foreach (CameraShakeComponent csc in section.cameraShakeComponents)
            {
                totalShakePower += csc.GetShakePowerAtPos(drawer.pos);
            }

            totalShakePower = new Point(MyMath.Clamp(totalShakePower.x, 0, 1), MyMath.Clamp(totalShakePower.y, 0, 1));

            return CameraShakeComponent.GetCamOffset(section.world, totalShakePower);
        }
    }
}
