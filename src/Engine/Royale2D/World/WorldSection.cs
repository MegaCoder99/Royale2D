namespace Royale2D
{
    public class WorldSection
    {
        public List<WorldSectionLayer> layers = [];
        public List<Actor> actors = [];
        public ColliderGrid colliderGrid;
        public List<CameraShakeComponent> cameraShakeComponents = [];
        public List<FxLayer> fxLayers = [];
        public World world;
        public string name;

        private List<Actor> _particleEffects;
        public List<Actor> particleEffects
        {
            get
            {
                if (_particleEffects == null) _particleEffects = new List<Actor>();
                return _particleEffects;
            }
        }

        public TileData[,] firstTileGrid => layers[0].tileGrid;

        public MapSection mapSection;

        public WorldSection(World world, MapSection mapSection)
        {
            this.world = world;
            this.name = mapSection.name;
            this.mapSection = mapSection;

            for (int i = 0; i < mapSection.layers.Count; i++)
            {
                layers.Add(new WorldSectionLayer(i, this));
            }
            
            colliderGrid = new ColliderGrid(mapSection);

            // Behind the hoods this will populate actors list in the individual actor constructors in this method
            // (kinda weird, maybe think about more explicit adding of actors to section instead of implicitly in constructors)
            mapSection.CreateActors(this);

            // REFACTOR do not key off map section name, should be something more "internal" and "hard-coded"
            if (mapSection.IsWoods())
            {
                fxLayers.Add(new WoodsFogFxLayer());
            }
            else if (mapSection.name == "mountain")
            {
                fxLayers.Add(new MountainFxLayer());
            }
        }

        public void Update()
        {
            foreach (WorldSectionLayer layer in layers)
            {
                layer.Update();
            }

            foreach (FxLayer fxLayer in fxLayers)
            {
                fxLayer.Update();
            }

            // Game logic
            foreach (Actor actor in actors.Concat(particleEffects).ToList())
            {
                FdPoint lastPos = actor.pos;
                actor.PreUpdate();
                actor.Update();
                actor.RenderUpdate();
                actor.PostUpdate();
                actor.deltaPos = actor.pos - lastPos;
                actor.OnPositionChanged(lastPos, actor.pos);
            }

            // FYI we only update the actor grid after its Update() step has run for simplicity.
            // Limitations: any changes in hitboxes (pos, sprite, etc) in actor.Update() will not be reflected until next frame, or in a subsequent actor on the same frame
            // ALWAYS keep this limitation in mind. If these limitations present problems in future, come up with different solution.
            cameraShakeComponents.Clear();
            foreach (Actor actor in actors.ToList())
            {
                if (actor is Anim) continue;    // for perf
                if (actor.GetComponent<ColliderComponent>() is ColliderComponent cc)
                {
                    colliderGrid.UpdateInGrid(cc, this);
                }
                if (actor.GetComponent<CameraShakeComponent>() is CameraShakeComponent csc)
                {
                    cameraShakeComponents.Add(csc);
                }
            }
        }

        public void Render(Drawer drawer)
        {
            foreach (FxLayer fxLayer in fxLayers)
            {
                if (!fxLayer.isForeground) fxLayer.Render(drawer);
            }

            foreach (WorldSectionLayer layer in layers)
            {
                layer.Render(drawer);
            }

            foreach (Actor actor in actors.Concat(particleEffects))
            {
                if (actor.IsVisible())
                {
                    actor.Render(drawer);
                }
            }

            world.storm.Render(drawer, this);

            foreach (FxLayer fxLayer in fxLayers)
            {
                if (fxLayer.isForeground) fxLayer.Render(drawer);
            }

            Debug.main?.RenderToWorld(drawer, colliderGrid);
        }

        public void AddActor(Actor actor)
        {
            // Should never add a destroyed actor, this could happen if transitioning sections
            if (actor.isDestroyed) return;

            actor.section = this;
            actors.Add(actor);
        }

        public void RemoveActor(Actor actor)
        {
            if (actor.GetComponent<ColliderComponent>() is ColliderComponent cc)
            {
                colliderGrid.RemoveFromGrid(cc);
            }

            if (actor.GetComponent<CameraShakeComponent>() is CameraShakeComponent csc)
            {
                cameraShakeComponents.Remove(csc);
            }

            actors.Remove(actor);
        }

        // Can also think of this as "GetOverworldPos(Point insideStructurePos)"
        public FdPoint GetMainSectionPos(FdPoint nonMainSectionPos)
        {
            if (mapSection.indoorMappingToMain == null)
            {
                return nonMainSectionPos;
            }
            
            IntRect overworldRect = mapSection.indoorMappingToMain.Value.GetIntRect();

            // IMPROVE these are slightly off, 1.003
            Fd xScale = Fd.New(overworldRect.w) / mapSection.pixelWidth;
            Fd yScale = Fd.New(overworldRect.h) / mapSection.pixelHeight;

            Fd adjustedX = overworldRect.x1 + (nonMainSectionPos.x * xScale);
            Fd adjustedY = overworldRect.y1 + (nonMainSectionPos.y * yScale);

            return new FdPoint(adjustedX, adjustedY);
        }

        public Actor? CreateRandomPickup(FdPoint pos, bool bounceUp)
        {
            int rand = NetcodeSafeRng.RandomRange(0, 400);
            if (rand < 30)
            {
                return Collectables.CreateGreenRupee(this, pos, bounceUp);
            }
            else if (rand < 50)
            {
                int amount = 1;
                int amountDecider = NetcodeSafeRng.RandomRange(1, 10);
                if (amountDecider < 6) amount = 1;
                else if (amountDecider < 9) amount = 4;
                else if (amountDecider <= 10) amount = 8;
                return Collectables.CreateArrow(this, pos, bounceUp, amount);
            }
            else if (rand < 70)
            {
                return Collectables.CreateHeart(this, pos, bounceUp);
            }
            else if (rand < 90)
            {
                return Collectables.CreateSmallMagic(this, pos, bounceUp);
            }
            else if (rand < 97)
            {
                return Collectables.CreateBlueRupee(this, pos, bounceUp);
            }
            else if (rand < 98)
            {
                return Collectables.CreateRedRupee(this, pos, bounceUp);
            }
            else if (rand < 99)
            {
                return Collectables.CreateLargeMagic(this, pos, bounceUp);
            }
            else if (rand < 100)
            {
                return new Fairy(this, pos, true);
            }
            else
            {
                return null;
            }
        }
    }
}
