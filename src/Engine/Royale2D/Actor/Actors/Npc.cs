namespace Royale2D
{
    public class Npc : Actor
    {
        public Npc(WorldSection section, FdPoint pos, string spriteName, Dialog dialog) :
            base(section, pos, spriteName)
        {
            AddComponent(new DialogSourceComponent(this, dialog));
            AddComponent(new ColliderComponent(this));
            var poofComponent = AddComponent(new PoofComponent(this));
            AddComponent(new BunnyComponent(this, poofComponent, "npc_dark_world"));
        }
    }
}
