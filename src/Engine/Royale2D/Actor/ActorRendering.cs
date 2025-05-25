namespace Royale2D
{
    public partial class Actor
    {
        private float _alpha = 1;
        public float alpha
        {
            get => _alpha;
            set => _alpha = Math.Clamp(value, 0, 1);
        }
        public int zLayerOffset = ZIndex.LayerOffsetActor;
        public string baseSpriteName;
        public SpriteInstance spriteInstance;
        public int xDir = 1;    // Don't change manually if using direction component, use DirectionComponent.Change()
        public int yDir = 1;    // Don't change manually if using direction component, use DirectionComponent.Change()
        public bool visible = true;    // Meant for quick and dirty cases on small random actors. Don't overuse since it doesn't handle complexity and multiple sources well
        public IntPoint spriteOffset;

        public Sprite sprite => spriteInstance.sprite;
        public string spriteName => spriteInstance.spriteName;
        public int frameIndex => spriteInstance.frameIndex;
        public void ChangeFrameIndex(int newFrameIndex) => spriteInstance.ChangeFrameIndex(newFrameIndex);
        public int currentFrameTime => spriteInstance.currentFrameTime;
        public int totalFrameTime => spriteInstance.totalFrameTime;
        public Fd frameSpeed
        {
            get => spriteInstance.frameSpeed;
            set => spriteInstance.frameSpeed = value;
        }
        public int loopCount => spriteInstance.loopCount;
        public int elapsedFrames => spriteInstance.elapsedFrames;
        public bool IsAnimOver() => spriteInstance.IsAnimOver();

        public virtual void Render(Drawer drawer)
        {
            Point renderPos = GetRenderFloatPos();

            spriteInstance.Render(
                drawer,
                renderPos.x,
                renderPos.y,
                GetRenderZIndex(),
                xDir,
                yDir,
                xScale: 1,
                yScale: 1,
                alpha,
                spriteOffset,
                GetDrawboxTagsToHide(),
                GetOverrideTexture(),
                GetShaderInstance()
            );

            foreach (Component component in Component.SortComponentsForRender(components))
            {
                if (!component.disabled)
                {
                    component.Render(drawer);
                }
            }
        }

        // This is the definitive Point at which the actor will be rendered, and should be used in most actor/component rendering code
        // Actor.pos can "lie" to you; it's an abstraction that doesn't always represent where the actor is rendered/shown on screen.
        // This is the case for ZComponent, where the character's pos remains at the ground, yet they are rendered higher up
        public FdPoint GetRenderPos()
        {
            FdPoint renderOffset = FdPoint.Zero;
            foreach (Component component in Component.SortComponentsForRender(components))
            {
                if (!component.disabled)
                {
                    renderOffset += component.GetRenderOffset();
                }
            }
            return pos + renderOffset;
        }

        public Point GetRenderFloatPos()
        {
            return GetRenderPos().ToFloatPoint();
        }

        // This is in WORLD coordinates, NOT local
        public FdPoint GetFirstPOI(string? withTag = null, bool useRenderOffset = false)
        {
            FdPoint basePos = useRenderOffset ? GetRenderPos() : pos;
            POI? poi = spriteInstance.GetCurrentFrame().POIs.FirstOrDefault(poi => withTag == null ? true : poi.tags == withTag);
            if (poi == null) return basePos;
            return basePos.AddXY(poi.x * xDir, poi.y);
        }

        // This is in WORLD coordinates, NOT local
        public FdPoint? GetFirstPOIOrNull(string? withTag = null, bool useRenderOffset = false)
        {
            FdPoint basePos = useRenderOffset ? GetRenderPos() : pos;
            POI? poi = spriteInstance.GetCurrentFrame().POIs.FirstOrDefault(poi => withTag == null ? true : poi.tags == withTag);
            if (poi == null) return null;
            return basePos.AddXY(poi.x * xDir, poi.y);
        }

        public virtual ShaderInstance? GetShaderInstance()
        {
            return null;
        }

        public virtual List<string> GetDrawboxTagsToHide()
        {
            return [];
        }

        public virtual string GetOverrideTexture()
        {
            return "";
        }

        public ZIndex GetRenderZIndex(int childZIndexOffset = 0, int? overrideYPos = null)
        {
            // Child components should be considered part of the parent for z-index rendering
            if (GetComponent<ChildComponent>() is ChildComponent childComponent)
            {
                /*
                // This abstraction system is used to "hook" into parent sprite's child frame matching a certain tag for getting z-index
                // For example it's mainly used to determine z-index of child liftable actor as it's being lifted
                if (childComponent.childSpriteTag != "")
                {
                    Frame? matchingChildFrame = childComponent.parent.spriteInstance.GetCurrentFrame().childFrames.FirstOrDefault(f => f.tags.Contains(childComponent.childSpriteTag));
                    childZIndexOffset = matchingChildFrame?.zIndex ?? childZIndexOffset;
                }
                */

                return childComponent.parent.GetRenderZIndex(childZIndexOffset);
            }

            // If on bottom of stairs, render as if on one layer above so sword swing animations and whatnot don't clip under the floor
            int layerIndexToUse = layerIndex;
            if (GetComponent<ColliderComponent>() is ColliderComponent cc && cc.isOnLowerStairs)
            {
                layerIndexToUse++;
            }

            return new ZIndex(layerIndexToUse, zLayerOffset, overrideYPos ?? pos.y.intVal, childZIndexOffset);
        }

        public virtual bool IsVisible()
        {
            foreach (Component component in components.ToList())
            {
                if (!component.IsVisible())
                {
                    return false;
                }
            }
            return visible;
        }

        // Called automatically in PostUpdate, should be called manually if you need the new spriteInstance ready before then
        public void RefreshSprite()
        {
            string spriteNameToUse = GetSpriteNameToUse();
            if (spriteNameToUse != spriteInstance.spriteName)
            {
                if (Assets.sprites.ContainsKey(spriteNameToUse))
                {
                    spriteInstance = new SpriteInstance(spriteNameToUse);
                }
                else
                {
                    spriteInstance = new SpriteInstance("empty");
                }
            }
        }

        public virtual string GetSpriteNameToUse()
        {
            string baseSpriteNameToUse = baseSpriteName;

            Character? chr = this as Character;
            if (chr != null)
            {
                // Ledge jump state is unique in that it puts the player in a "freeze-frame" of the last charstate's exact sprite used
                if (chr.charState is LedgeJumpState ljs)
                {
                    return ljs.baseSpriteName;
                }

                baseSpriteNameToUse = chr.charState.baseSpriteName;
            }
            if (GetComponent<BunnyComponent>() is BunnyComponent bc && bc.bunnyTime > 0)
            {
                baseSpriteNameToUse = bc.baseSpriteName;
            }
            if (GetComponent<DirectionComponent>() is DirectionComponent dc)
            {
                string dcSpriteName = dc.GetSpriteName(baseSpriteNameToUse);
                if (Assets.sprites.ContainsKey(dcSpriteName))
                {
                    baseSpriteNameToUse = dcSpriteName;
                }
            }
            if (chr != null && !chr.lastMoveData.moveAmount.IsZero())
            {
                if (Assets.sprites.ContainsKey(baseSpriteNameToUse + "_move"))
                {
                    baseSpriteNameToUse += "_move";
                }
            }

            return baseSpriteNameToUse;
        }
    }
}
