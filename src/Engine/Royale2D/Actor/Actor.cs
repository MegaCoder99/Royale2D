namespace Royale2D
{
    public partial class Actor
    {
        public FdPoint pos;    // If collision is on, usually don't change directly, use IncPos or ChangePos instead
        public int layerIndex;
        public bool isDestroyed;
        public List<Component> components = new List<Component>();
        public FdPoint lastPos; // Do not use except in one place that uses it
        public FdPoint deltaPos;
        public WorldSection section;

        public WorldSectionLayer layer => section.layers[layerIndex];
        public World world => section.world;
        public Match match => world.match;

        public Actor(WorldSection section, FdPoint pos, string spriteName = "empty", bool addToSection = true)
        {
            this.pos = pos;
            this.section = section;
            baseSpriteName = spriteName;
            layerIndex = section.mapSection.startLayer;
            if (Assets.sprites.ContainsKey(spriteName) == false)
            {
                spriteInstance = new SpriteInstance("empty");
            }
            else
            {
                spriteInstance = new SpriteInstance(spriteName);
            }
            if (addToSection)
            {
                section.AddActor(this);
            }
        }

        public Actor(Actor creator, FdPoint pos, string spriteName, bool addToSection = true) : this(creator.section, pos, spriteName, addToSection)
        {
            layerIndex = creator.layerIndex;
        }

        public virtual void PreUpdate()
        {
        }

        public virtual void Update()
        {
            spriteInstance.Update();
            foreach (Component component in Component.SortComponentsForUpdate(components))
            {
                if (!component.disabled)
                {
                    component.Update();
                }
            }
        }

        public void PostUpdate()
        {
            foreach (Component component in Component.SortComponentsForUpdate(components))
            {
                if (!component.disabled)
                {
                    component.PostUpdate();
                }
            }
            RefreshSprite();
        }

        public virtual void RenderUpdate()
        {
        }

        public virtual void OnActorCollision(ActorCollision collision)
        {
            foreach (Component component in components.ToList())
            {
                component.OnActorCollision(collision);
            }
        }

        public virtual void OnTileCollision(TileCollision collision)
        {
            foreach (Component component in components.ToList())
            {
                component.OnTileCollision(collision);
            }
        }

        public virtual void OnDamageDealt()
        {
            foreach (Component component in components.ToList())
            {
                component.OnDamageDealt();
            }
        }

        public virtual void OnDamageReceived(Damager damager, Character? attacker, ActorColliderInstance? attackerAci)
        {
        }

        public virtual void OnLand()
        {
            foreach (Component component in components.ToList())
            {
                component.OnLand();
            }
        }

        public virtual void OnPositionChanged(FdPoint oldPos, FdPoint newPos)
        {
        }

        public virtual void OnShieldBlock()
        {
        }

        public T? GetComponent<T>(bool includeDisabled = false)
        {
            foreach (Component component in components.ToList())
            {
                if (!includeDisabled && component.disabled) continue;
                if (component is T t) return t;
            }

            return default;
        }

        public T AddComponent<T>(T component, bool disabledOnCreate = false) where T : Component
        {
            if (components.Any(c => c.GetType() == component.GetType()))
            {
                throw new Exception("Component type already exists");
            }
            if (disabledOnCreate)
            {
                component.disabled = true;
            }
            components.Add(component);
            return component;
        }

        public void RemoveComponent<T>()
        {
            foreach (Component component in components.ToList())
            {
                if (component is T)
                {
                    components.Remove(component);
                    // section.RemoveComponent(component);
                }
            }
        }

        // Unlike the above if the component already exists, it will be replaced with the new one
        // Use AddComponent in times where double adding is a mistake, to catch errors better
        // Caveat: If using this, be sure to update the component variable to its return value, if you have one
        // IMPROVE how can we avoid the caveat mentioned above?
        public T ResetComponent<T>(T component) where T : Component
        {
            RemoveComponent<T>();
            return AddComponent<T>(component);
        }

        public void EnableComponent<T>() where T : Component
        {
            if (GetComponent<T>(true) is T component)
            {
                component.disabled = false;
            }
        }

        public void DisableComponent<T>() where T : Component
        {
            if (GetComponent<T>(true) is T component)
            {
                component.disabled = true;
            }
        }

        public void DestroySelf(string fadeSprite, string fadeSound = "")
        {
            DestroySelf(() =>
            {
                if (fadeSprite != "")
                {
                    new Anim(this, GetRenderPos(), fadeSprite, new AnimOptions { direction = GetComponent<DirectionComponent>()?.direction, soundName = fadeSound });
                }
            });
        }

        public void DestroySelf(Action? destroyAction = null)
        {
            // This could get called multiple times in a frame, i.e. collision callback loops, so guard against that
            if (isDestroyed)
            {
                //Debugger.Break();
                return;
            }
            isDestroyed = true;

            destroyAction?.Invoke();

            section.RemoveActor(this);

            foreach (Component component in components.ToList())
            {
                component.OnDestroy();
            }
        }

        // Set dontPlayIfExists=true and call every frame to essentially loop the sound
        public void PlaySound(string soundName, bool dontPlayIfExists = false)
        {
            world.soundManager.AddSound(soundName, this, dontPlayIfExists);
        }

        public void IncPos(FdPoint amount)
        {
            if (GetComponent<ColliderComponent>() is ColliderComponent cc)
            {
                cc.QueueMove(amount);
            }
            else
            {
                pos += amount;
            }
        }

        public void ChangePos(FdPoint newPos)
        {
            IncPos(newPos - pos);
        }

        public bool MoveToPos(FdPoint destPoint, Fd speed)
        {
            FdPoint amount = pos.DirTo(destPoint).Normalized() * speed;
            pos += amount;
            if (pos.DistanceTo(destPoint) < speed)
            {
                return true;
            }
            return false;
        }
    }
}
