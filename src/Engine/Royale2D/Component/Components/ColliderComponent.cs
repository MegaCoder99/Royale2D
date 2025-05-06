using Shared;

namespace Royale2D
{
    public class ColliderComponent : Component
    {
        public List<Collider> globalColliders = [];
        public FdPoint queuedMoveAmount;
        public ColliderMoveStrategy moveStrategy;
        public List<GridCoords> gridCoords = [];    // Only used for ColliderGrid cached removals perf
        public bool enableStairClimbing = true;
        public bool useWallColliderForStairs = true;    // For some actors like boomerang, we don't use wall collider because when it reverses dir, its wall collider flag is disabled
        public bool isOnUpperStairs;
        public bool isOnLowerStairs;
        public bool disableWallCollider;
        public string[] tileClumpTagsToIgnore = [];

        public bool isOnStairs => isOnUpperStairs || isOnLowerStairs;
        public ProjMoveStrategy? projMoveStrategy => moveStrategy as ProjMoveStrategy;
        public CharMoveStrategy? charMoveStrategy => moveStrategy as CharMoveStrategy;

        public ColliderComponent(Actor actor) : this(actor, false, false)
        {
        }

        public ColliderComponent(Actor actor, bool createZCollider = false, bool isDamager = false, bool isDamagable = false, bool isWallCollider = true) : base(actor)
        {
            globalColliders.AddRange(actor.spriteInstance.sprite.hitboxes.Select(h => h.ToCollider()));
            if (globalColliders.Count > 0)
            {
                var firstCollider = globalColliders.First();
                if (isWallCollider)
                {
                    // By convention, the first collider is made the wall collider
                    firstCollider.isWallCollider = isWallCollider;
                }
                if (createZCollider)
                {
                    globalColliders.Add(new Collider(firstCollider.shape));
                }
            }
            
            // By convention, if the sprite representing a projectile has 2 global colliders, the second is made the damager/damagable
            // This is for use cases where the wall collision box must be a different size than the attack box
            // REFACTOR may need to clean this up later, it's very implicit (yet saves a ton of manual editor config)
            if (globalColliders.Count > 1)
            {
                globalColliders[1].isDamager = isDamager;
                globalColliders[1].isDamagable = isDamagable;
            }
            else if (globalColliders.Count > 0)
            {
                globalColliders[0].isDamager = isDamager;
                globalColliders[0].isDamagable = isDamagable;
            }

            moveStrategy = new NormalMoveStrategy(this);
        }

        public ColliderComponent(Actor actor, List<Collider> colliders) : base(actor)
        {
            globalColliders.AddRange(colliders);
            moveStrategy = new NormalMoveStrategy(this);
        }

        public override void Update()
        {
            base.Update();
            isOnUpperStairs = false;
            isOnLowerStairs = false;
            if (enableStairClimbing)
            {
                for (int i = 0; i < actor.section.layers.Count; i++)
                {
                    if (IsInTileWithTag([TileTag.StairUpper], actor.section.layers[i], useWallColliderForStairs))
                    {
                        actor.layerIndex = i + 1;
                        isOnUpperStairs = true;
                        break;
                    }
                    if (IsInTileWithTag([TileTag.StairLower], actor.section.layers[i], useWallColliderForStairs))
                    {
                        actor.layerIndex = i;
                        isOnLowerStairs = true;
                        break;
                    }
                }
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            moveStrategy.ApplyMove(queuedMoveAmount);
            queuedMoveAmount = FdPoint.Zero;
        }

        // QueueMove is called manually, the applymove functions are called in PostUpdate automatically
        public void QueueMove(FdPoint moveAmount)
        {
            queuedMoveAmount += moveAmount;
        }

        public override void Render(Drawer drawer)
        {
            if (Debug.showHitboxes)
            {
                foreach (Collider collider in GetAllColliders())
                {
                    var colliderInstance = new ActorColliderInstance(collider, this);
                    drawer.DrawPolygon(colliderInstance.GetWorldShape().GetPoints(), Colors.DebugBlue, true, ZIndex.UIGlobal);
                }
            }
        }

        public void ChangeMoveStrategy(ColliderMoveStrategy moveStrategy)
        {
            this.moveStrategy = moveStrategy;
            if (moveStrategy is ProjMoveStrategy)
            {
                tileClumpTagsToIgnore = [TileClumpTags.ProjFlyOver];
            }
        }

        public void SortAndFilterActorCollisions(List<ActorCollision> actorCollisions)
        {
            actorCollisions.StableSort((ac1, ac2) =>
            {
                return ac1.CompareToSort(ac2);
            });
            for (int i = 0; i < actorCollisions.Count; i++)
            {
                for (int j = i + 1; j < actorCollisions.Count; j++)
                {
                    actorCollisions[i].CompareToFilter(actorCollisions[j]);
                }
            }
            actorCollisions.RemoveAll(ac => ac.filtered);
        }

        #region core collision functions
        public List<Collider> GetAllColliders()
        {
            var colliders = new List<Collider>(globalColliders);
            colliders.AddRange(actor.spriteInstance.GetFrameColliders(actor.GetChildFrameTagsToHide()));
            return colliders;
        }

        public List<Collider> GetWallColliders()
        {
            if (disableWallCollider) return [];
            return GetAllColliders().Where(c => c.isWallCollider).ToList();
        }

        // Main means using the main collider only (usually only one of these)
        public List<ActorCollision> GetMainActorCollisions(FdPoint moveAmount)
        {
            return GetActorCollisions(moveAmount, GetWallColliders());
        }

        public List<ActorCollision> GetAllActorCollisions(FdPoint moveAmount)
        {
            return GetActorCollisions(moveAmount, GetAllColliders());
        }

        // Main means using the main collider only (usually only one of these)
        public List<TileCollision> GetMainTileCollisions(FdPoint moveAmount)
        {
            return GetTileCollisions(moveAmount, GetWallColliders());
        }

        public List<ActorCollision> GetActorCollisions(FdPoint moveAmount, List<Collider> collidersToUse)
        {
            var collisions = new List<ActorCollision>();
            if (disabled) return collisions;

            foreach (Collider collider in collidersToUse)
            {
                var mine = new ActorColliderInstance(collider, this, moveAmount);
                IntRect? myWorldRect = mine.GetWorldShape() as IntRect;
                if (myWorldRect == null) break;

                List<ColliderComponent> gridCCs = actor.section.colliderGrid.GetColliderComponents(myWorldRect, actor.section);
                foreach (ColliderComponent otherCC in gridCCs)
                {
                    if (otherCC == this) continue;
                    foreach (Collider otherCollider in otherCC.GetAllColliders())
                    {
                        var other = new ActorColliderInstance(otherCollider, otherCC);
                        ActorCollision? collision = mine.CheckActorCollision(other);
                        if (collision != null)
                        {
                            collisions.Add(collision);
                        }
                    }
                }
            }

            return collisions.OrderBy(wc => wc.sqeuDistance).ToList();
        }

        public List<TileCollision> GetTileCollisions(FdPoint moveAmount, List<Collider> collidersToUse)
        {
            var collisions = new List<TileCollision>();
            if (disabled) return collisions;

            foreach (Collider collider in collidersToUse)
            {
                var mine = new ActorColliderInstance(collider, this, moveAmount);
                IntRect? myWorldRect = mine.GetWorldShape() as IntRect;
                if (myWorldRect == null) break;

                // Tile check
                List<GridCoords> tileGridCoords = Helpers.GetOverlappingGridCoords(actor.section.firstTileGrid, myWorldRect, 8);
                foreach (GridCoords gridCoord in tileGridCoords)
                {
                    TileInstance? tileInstance = actor.layer.GetTileInstance(gridCoord.i, gridCoord.j);
                    if (tileInstance == null) continue;

                    Collider? tileCollider = tileInstance.Value.collider;
                    if (tileCollider != null)
                    {
                        var other = new TileColliderInstance(tileCollider, tileInstance.Value);
                        TileCollision? collision = mine.CheckTileCollision(other);
                        if (collision != null)
                        {
                            collisions.Add(collision.Value);
                        }
                    }
                }
            }

            return collisions.OrderBy(wc => wc.sqeuDistance).ToList();
        }

        public IntRect GetColliderBoundingRect()
        {
            List<Collider> colliders = GetAllColliders();
            int x1 = int.MaxValue;
            int y1 = int.MaxValue;
            int x2 = int.MinValue;
            int y2 = int.MinValue;
            foreach (Collider collider in colliders)
            {
                IntShape actorWorldShape = collider.GetActorWorldShape(actor);

                foreach (IntPoint point in actorWorldShape.GetPoints())
                {
                    if (point.x < x1) x1 = point.x;
                    if (point.y < y1) y1 = point.y;
                    if (point.x > x2) x2 = point.x;
                    if (point.y > y2) y2 = point.y;
                }
            }

            return new IntRect(x1, y1, x2, y2);
        }

        #endregion

        #region in tile with tag checks
        // PERF places are calling this repeatedly, optimize?
        public bool IsInTileWithTag(params string[] tagsList)
        {
            return IsInTileWithTag(tagsList, actor.layer);
        }

        public bool IsInTileWithTagAnyLayer(params string[] tagsList)
        {
            foreach (WorldSectionLayer layer in actor.section.layers)
            {
                if (IsInTileWithTag(tagsList, layer))
                {
                    return true;
                }
            }
            return false;
        }

        public List<TileInstance> GetTilesTouching(string colliderTags = "")
        {
            var tileInstances = new List<TileInstance>();

            List<Collider> colliders = GetAllColliders().Where(c => colliderTags == "" ? true : c.tags != "" && c.tags.Contains(colliderTags)).ToList();
            foreach (Collider collider in colliders)
            {
                IntRect? actorRect = collider.GetActorWorldShape(actor) as IntRect;
                if (actorRect == null) continue;

                // PERF this makes me realize, we can simplify collision check with tiles with just overlapping check
                List<GridCoords> gridCoords = Helpers.GetOverlappingGridCoords(actor.layer.tileGrid, actorRect, 8);
                foreach (GridCoords gridCoord in gridCoords)
                {
                    int i = gridCoord.i;
                    int j = gridCoord.j;
                    IntRect tileRect = new IntRect(j * 8, i * 8, (j + 1) * 8, (i + 1) * 8);
                    int area = MySpatial.GetIntersectArea(actorRect, tileRect);
                    //if (area > 0 && actorRect.area / area <= 2)
                    {
                        TileInstance? tileInstance = actor.layer.GetTileInstance(i, j);
                        if (tileInstance != null) tileInstances.Add(tileInstance.Value);
                    }
                }
            }

            return tileInstances;
        }

        private bool IsInTileWithTag(string[] tagsList, WorldSectionLayer layer, bool useWallCollider = true)
        {
            Collider? collider = useWallCollider ? GetWallColliders().FirstOrDefault() : GetAllColliders().FirstOrDefault();
            if (collider == null) return false;

            IntRect? actorRect = collider.GetActorWorldShape(actor) as IntRect;
            if (actorRect == null) return false;

            foreach (string tag in tagsList)
            {
                int totalArea = 0;
                List<GridCoords> gridCoords = Helpers.GetOverlappingGridCoords(layer.tileGrid, actorRect, 8);

                foreach (GridCoords gridCoord in gridCoords)
                {
                    int i = gridCoord.i;
                    int j = gridCoord.j;
                    TileData tileData = layer.tileGrid[i, j];
                    if (tileData.HasTag(tag))
                    {
                        IntRect tileRect = new IntRect(j * 8, i * 8, (j + 1) * 8, (i + 1) * 8);
                        int area = MySpatial.GetIntersectArea(actorRect, tileRect);
                        totalArea += area;
                    }
                }

                if (totalArea > 0 && actorRect.area / totalArea <= 2) return true;
            }

            return false;
        }
        #endregion
    }
}
