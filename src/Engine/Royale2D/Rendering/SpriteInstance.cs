using SFML.Graphics;
using Shared;

namespace Royale2D
{
    public class SpriteInstance
    {
        public string spriteName;
        public int frameIndex { get; private set; }
        public int currentFrameTime = 0;   // This is in frames, so 3 would be 3 frames elapsed since current frame index
        public int totalFrameTime = 0;     // This is in frames, so 30 would be 30 frames elapsed since start of entire sprite animation
        public Fd frameSpeed = 1;
        public int loopCount = 0;
        public int elapsedFrames;

        public Sprite sprite => Assets.GetSprite(spriteName);

        public SpriteInstance(string spriteName)
        {
            this.spriteName = spriteName;
        }

        public void Update()
        {
            elapsedFrames++;
            currentFrameTime++;
            totalFrameTime++;
            if (frameSpeed > 0 && currentFrameTime >= GetCurrentFrame().duration / frameSpeed)
            {
                bool onceEnd = sprite.wrapMode == WrapMode.Once && frameIndex == sprite.frames.Count - 1;
                if (!onceEnd)
                {
                    currentFrameTime = sprite.loopStartFrame;
                    frameIndex++;
                    if (frameIndex >= sprite.frames.Count)
                    {
                        frameIndex = 0;
                        totalFrameTime = 0;
                        loopCount++;
                    }
                }
            }
        }

        // PERF don't render stuff outside camera
        // REFACTOR drawboxTagsToHide can now hide parent frame too, rename parameter
        public void Render(Drawer drawer, float x, float y, ZIndex zIndex, 
            int xDir = 1, int yDir = 1, float xScale = 1, float yScale = 1, float alpha = 1, IntPoint? spriteOffset = null, 
            List<string>? drawboxTagsToHide = null, string overrideTexture = "", ShaderInstance? shaderInstance = null)
        {
            Point pos = new Point(x, y);
            Frame currentFrame = sprite.frames[frameIndex];
            IntPoint spriteOffsetValue = spriteOffset ?? IntPoint.Zero;

            float frameOffsetX = (currentFrame.offset.x + spriteOffsetValue.x) * xDir;
            float frameOffsetY = currentFrame.offset.y + spriteOffsetValue.y;

            float xDirArg = xDir * xScale;
            float yDirArg = yDir * yScale;

            Point center = GetFloatCenter();

            Texture? texture = currentFrame.texture ?? sprite.texture;
            if (overrideTexture != "")
            {
                texture = Assets.textures[overrideTexture];
            }
            if (texture == null)
            {
                return;
            }

            if (drawboxTagsToHide == null || !drawboxTagsToHide.Contains(currentFrame.tags))
            {
                drawer.DrawTexture(
                    texture: texture,
                    x: pos.x + frameOffsetX,
                    y: pos.y + frameOffsetY,
                    sourceRect: currentFrame.rect,
                    zIndex: zIndex,
                    cx: center.x,
                    cy: center.y,
                    xScale: xDirArg,
                    yScale: yDirArg,
                    angle: 0,
                    alpha: alpha,
                    shaderInstance: shaderInstance);
            }

            foreach (Drawbox drawbox in currentFrame.drawboxes)
            {
                if (drawboxTagsToHide != null && drawboxTagsToHide.Contains(drawbox.tags)) continue;

                ZIndex childZ = zIndex;

                // In principle, this should have been done in editor, but I'm not manually changing 1000's of textboxes at this point!
                if (drawbox.tags.Contains("sword") && drawbox.zIndex == 0)
                {
                    drawbox.zIndex = 1;
                }

                childZ.drawboxOffset += drawbox.zIndex * 10; // We multiply by 10 to give leeway to insert more z-indicies in between, like wadable

                drawer.DrawTexture(
                    texture: drawbox.texture,
                    x: pos.x + ((drawbox.pos.x + spriteOffsetValue.x) * xDir),
                    y: pos.y + ((drawbox.pos.y + spriteOffsetValue.y) * yDir),
                    sourceRect: drawbox.rect,
                    zIndex: childZ,
                    cx: 0,
                    cy: 0,
                    xScale: xDir,
                    yScale: yDir,
                    angle: 0,
                    alpha: alpha,
                    shaderInstance: shaderInstance);
            }
        }

        public void ChangeFrameIndex(int newFrameIndex)
        {
            if (newFrameIndex < 0 || newFrameIndex >= sprite.frames.Count) return;
            frameIndex = newFrameIndex;
        }

        public List<Collider> GetFrameColliders(List<string> drawboxTagsToHide)
        {
            var colliders = new List<Collider>();
            Frame currentFrame = sprite.frames[frameIndex];

            foreach (Hitbox hitbox in currentFrame.hitboxes)
            {
                if (drawboxTagsToHide.Contains(hitbox.tags)) continue;
                colliders.Add(hitbox.ToCollider());
            }

            return colliders;
        }

        public Frame GetCurrentFrame()
        {
            return sprite.frames[frameIndex];
        }

        public bool IsAnimOver()
        {
            return (frameIndex == sprite.frames.Count - 1 && currentFrameTime >= GetCurrentFrame().duration) || loopCount > 0;
        }

        public void Reset()
        {
            frameIndex = 0;
            currentFrameTime = 0;
            totalFrameTime = 0;
            loopCount = 0;
            elapsedFrames = 0;
        }

        public Point GetFloatCenter()
        {
            (float cx, float cy) = Helpers.GetAlignmentOriginFloat(sprite.alignment, GetCurrentFrame().rect.w, GetCurrentFrame().rect.h);
            return new Point(cx, cy);
        }

        public IntPoint GetIntCenter()
        {
            (int cx, int cy) = Helpers.GetAlignmentOriginInt(sprite.alignment, GetCurrentFrame().rect.w, GetCurrentFrame().rect.h);
            return new IntPoint(cx, cy);
        }
    }
}
