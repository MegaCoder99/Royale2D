using SFML.Graphics;
using SFML.System;
using Shared;
using Color = SFML.Graphics.Color;

namespace Royale2D
{
    public class Storm
    {
        const string renderTextureName = "world_storm";
        const int stormShrinkTimeInSeconds = 120;
        const int stormWaitTimeInSeconds = 120;
        const int stormShrinkTimeInFrames = stormShrinkTimeInSeconds * 60;
        const int stormWaitTimeInFrames = stormWaitTimeInSeconds * 60;

        public int stormPhase;
        public bool isStormWait = true;
        public bool isBattleBusPhase = true;
        public bool isFastStormTimer;

        public FdPoint currentStormCenter;
        public Fd currentStormRadius;

        public FdPoint nextStormCenter;
        public Fd nextStormRadius;
        public FdPoint nextCenterDir;

        public Fd centerMoveRatePxPerFrame;
        public Fd stormShrinkRatePxPerFrame;

        public int stormTimeInFrames = 30 * 60;
        public World world;

        public RenderTexture circleBuffer => world.textureManager.GetRenderTexture(renderTextureName);
        public float currentStormRadiusFloat => currentStormRadius.floatVal;    // Remember, floats are for rendering only!
        public int stormTimeInSeconds => stormTimeInFrames / 60;

        public Storm(World world)
        {
            this.world = world;

            int pixelWidth = world.map.mainSection.pixelWidth;
            int pixelHeight = world.map.mainSection.pixelHeight;

            currentStormRadius = pixelWidth * Fd.New(0, 75);
            currentStormCenter = new FdPoint(pixelWidth / 2, pixelHeight / 2);

            nextStormRadius = currentStormRadius;
            nextStormCenter = currentStormCenter;

            world.textureManager.AddRenderTexture(renderTextureName, (uint)pixelWidth, (uint)pixelHeight);

            if (Debug.main != null && Debug.main.debugStorm == true)
            {
                isStormWait = false;
                currentStormCenter = Debug.main.quickStartPos.ToFdPoint();
                currentStormRadius = 100;
            }
        }

        public void Update()
        {
            int incRateInFrames = (isFastStormTimer ? 100 : 1);
            if (isBattleBusPhase)
            {
                stormTimeInFrames -= incRateInFrames;
                if (stormTimeInFrames <= 0)
                {
                    isBattleBusPhase = false;
                    SetNextStorm();
                }
            }
            else
            {
                stormTimeInFrames -= incRateInFrames;
                if (!isStormWait)
                {
                    currentStormCenter += nextCenterDir * centerMoveRatePxPerFrame * incRateInFrames;
                    currentStormRadius -= stormShrinkRatePxPerFrame * incRateInFrames;
                }

                if (stormTimeInFrames <= 0)
                {
                    isStormWait = !isStormWait;
                    SetNextStorm();
                }
            }
        }

        // Indoor areas can result in a distorted, elliptical visual representation of the storm due to its coordinate system being "stretched" with respect to the main map section
        public (Fd x, Fd y, Fd xRadius, Fd yRadius) GetIndoorStormEllipse(WorldSection renderSection, GridRect indoorMappingToMainGridRect)
        {
            IntRect indoorMappingRect = indoorMappingToMainGridRect.GetIntRect();

            Fd r = currentStormRadius;
            Fd cx = currentStormCenter.x;
            Fd cy = currentStormCenter.y;
            Fd rx = indoorMappingRect.x1;
            Fd ry = indoorMappingRect.y1;
            Fd rw = indoorMappingRect.w;
            Fd rh = indoorMappingRect.h;
            Fd r2w = renderSection.mapSection.pixelWidth;
            Fd r2h = renderSection.mapSection.pixelHeight;

            Fd retCx = ((cx - rx) * r2w) / rw;
            Fd retCy = ((cy - ry) * r2h) / rh;

            Fd retXRadius = (r * r2w) / rw;
            Fd retYRadius = (r * r2h) / rh;

            return (retCx, retCy, retXRadius, retYRadius);
        }

        public void Render(Drawer drawer, WorldSection renderSection)
        {
            int pixelWidth = renderSection.mapSection.pixelWidth;
            int pixelHeight = renderSection.mapSection.pixelHeight;
            Point getTopLeftCamPos = drawer.pos.AddXY(-Game.HalfScreenW, -Game.HalfScreenH);

            Point stormCenterToDraw = currentStormCenter.ToFloatPoint();
            float stormXRadiusToDraw = currentStormRadiusFloat;
            float stormYRadiusToDraw = currentStormRadiusFloat;

            if (renderSection.mapSection.indoorMappingToMain != null)
            {
                (Fd cx, Fd cy, Fd xRadius, Fd yRadius) = GetIndoorStormEllipse(renderSection, renderSection.mapSection.indoorMappingToMain.Value);
                stormCenterToDraw = new Point(cx.floatVal, cy.floatVal);
                stormXRadiusToDraw = xRadius.floatVal;
                stormYRadiusToDraw = yRadius.floatVal;
            }

            if (renderSection.mapSection.mainSectionChildPos != null)
            {
                stormCenterToDraw.x -= renderSection.mapSection.mainSectionChildPos.Value.j * 8;
                stormCenterToDraw.y -= renderSection.mapSection.mainSectionChildPos.Value.i * 8;
            }

            circleBuffer.Clear(Color.Transparent);

            RenderStates states = new RenderStates(circleBuffer.Texture);
            states.BlendMode = new BlendMode(BlendMode.Factor.One, BlendMode.Factor.One, BlendMode.Equation.Subtract);

            RectangleShape rect = new RectangleShape(new Vector2f(pixelWidth, pixelHeight));
            rect.Position = new Vector2f(0, 0);
            rect.FillColor = Colors.StormColor;
            circleBuffer.Draw(rect, states);

            EllipseDrawable ellipse = new EllipseDrawable(stormCenterToDraw.x, stormCenterToDraw.y, stormXRadiusToDraw, stormYRadiusToDraw, Colors.StormColor, 1000);
            circleBuffer.Draw(ellipse, states);

            float x = MathF.Floor(getTopLeftCamPos.x);
            float y = MathF.Floor(getTopLeftCamPos.y);

            FdPoint randScreenPos = new FdPoint(Helpers.RandomRange((int)x, (int)x + Game.ScreenW), Helpers.RandomRange((int)y, (int)y + Game.ScreenH));
            
            if (IsPosInStorm(randScreenPos, renderSection))
            {
                new Anim(renderSection, randScreenPos, "particle_twilight", new AnimOptions { fadeTime = 1, framesToLive = 60, vel = new FdPoint(0, Fd.New(0, -42)), isParticle = true });
            }

            circleBuffer.Display();

            drawer.DrawTexture(circleBuffer.Texture, 0, 0, ZIndex.UIGlobal);
        }

        public void SetNextStorm()
        {
            if (isStormWait)
            {
                // setCurrentMessage("Twilight moves in " + stormShrinkTime.ToString() + " seconds", 5);
                stormTimeInFrames = stormWaitTimeInFrames;
                currentStormCenter = nextStormCenter;
                currentStormRadius = nextStormRadius;

                if (stormPhase == 0)
                {
                    nextStormRadius = currentStormRadius * Fd.Point5;
                }
                else
                {
                    nextStormRadius = currentStormRadius * Fd.Point5;
                }

                int loop = 0;
                while (true)
                {
                    loop++; if (loop > 10000) { throw new Exception("INFINITE LOOP IN SETNEXTSTORM!"); }

                    //Select a random point in the current circle
                    FdPoint prospectiveNext = GetRandPointInCircle(currentStormCenter, currentStormRadius, nextStormRadius);

                    if (prospectiveNext.x - nextStormRadius < 0 || prospectiveNext.y - nextStormRadius < 0 || prospectiveNext.x + nextStormRadius > 4096 || prospectiveNext.y + nextStormRadius > 4096)
                    {
                        continue;
                    }

                    nextStormCenter = prospectiveNext;
                    break;
                }

                nextCenterDir = currentStormCenter.DirToNormalized(nextStormCenter);
                stormShrinkRatePxPerFrame = (currentStormRadius - nextStormRadius) / stormShrinkTimeInFrames;
                centerMoveRatePxPerFrame = currentStormCenter.DistanceTo(nextStormCenter) / stormShrinkTimeInFrames;

                stormPhase++;
            }
            else
            {
                foreach (Character character in world.characters)
                {
                    character.charMusic.SetWarningMusic();
                }
                world.hud.SetAlert1("Warning: Twilight now moving!");
                stormTimeInFrames = stormShrinkTimeInFrames;
            }
        }

        public static FdPoint GetRandPointInCircle(FdPoint center, Fd radius, Fd insideCircleRadius)
        {
            radius -= insideCircleRadius;
            int randRadius = NetcodeSafeRng.RandomRange(0, radius.intVal);
            int randAngle = NetcodeSafeRng.RandomRange(0, 360);
            return center + new FdPoint(randRadius * NetcodeSafeMath.CosD(randAngle), randRadius * NetcodeSafeMath.SinD(randAngle));
        }

        public bool IsPosInStorm(FdPoint pos, WorldSection section)
        {
            if (section.name == "main")
            {
                return pos.DistanceTo(currentStormCenter) > currentStormRadius;
            }

            FdPoint mainSectionPos = section.GetMainSectionPos(pos);
            return mainSectionPos.DistanceTo(currentStormCenter) > currentStormRadius;
        }

        public float GetStormDamage()
        {
            if (stormPhase < 4) return 0.25f;
            else if (stormPhase == 4) return 0.5f;
            else if (stormPhase == 5) return 0.75f;
            else return 1;
        }

        public string GetStormDisplayTime()
        {
            int stormTimeMinutesPart = stormTimeInSeconds / 60;
            int stormTimeSecondsPart = stormTimeInSeconds - (stormTimeMinutesPart * 60);
            string minutesStr = stormTimeMinutesPart.ToString();
            string secondsStr = stormTimeSecondsPart.ToString();
            if (secondsStr.Length == 1) secondsStr = "0" + secondsStr;
            return minutesStr + ":" + secondsStr;
        }
    }
}
