namespace Royale2D
{
    public abstract class Component
    {
        public bool disabled;
        public Actor actor;

        // public int time;     // Uncomment if at least 3 places are definitively using this

        public Component(Actor actor)
        {
            this.actor = actor;
        }

        public virtual void Update()
        {
            // time++;
        }

        public virtual void PostUpdate()
        {
        }

        public virtual void Render(Drawer drawer)
        {
        }

        public virtual void OnActorCollision(ActorCollision collision)
        {
        }

        public virtual void OnTileCollision(TileCollision collision)
        {
        }

        public virtual void OnDamageDealt()
        {
        }

        public virtual void OnLand()
        {
        }

        public virtual void OnDestroy()
        {
        }

        public T? GetComponent<T>()
        {
            return actor.GetComponent<T>();
        }

        internal static IEnumerable<Component> SortComponentsForUpdate(List<Component> components)
        {
            return components.OrderBy(component => componentUpdateOrder.IndexOf(component.GetType()));
        }

        internal static IEnumerable<Component> SortComponentsForRender(List<Component> components)
        {
            return components.OrderBy(component => componentRenderOrder.IndexOf(component.GetType()));
        }

        public virtual FdPoint GetRenderOffset()
        {
            return FdPoint.Zero;
        }

        // If ANY component is not visible, the actor is considered not visible
        public virtual bool IsVisible()
        {
            return true;
        }

        // These two vars below must have all components for netcode safety to avoid unpredictable execution orders

        public static readonly List<Type> componentUpdateOrder = new List<Type>
        {
            typeof(ColliderComponent),
            typeof(DirectionComponent),
            typeof(ShadowComponent),
            typeof(WadeComponent),
            typeof(ZComponent),
            typeof(LiftableComponent),
            typeof(ParentComponent)
        };

        public static readonly List<Type> componentRenderOrder = new List<Type>
        {
            typeof(ColliderComponent),
            typeof(DirectionComponent),
            typeof(ShadowComponent),
            typeof(WadeComponent),
            typeof(ZComponent),
            typeof(LiftableComponent),
            typeof(ParentComponent)
        };
    }
}
