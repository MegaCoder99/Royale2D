namespace Royale2D
{
    public class ShovelState : CharState
    {
        public ShovelState(Character character) : base(character)
        {
            baseSpriteName = "char_shovel";
            idleOnAnimEnd = true;
        }

        public override void Update()
        {
            base.Update();

            FdPoint? digPOI = character.GetFirstPOIOrNull();
            if (digPOI != null && !once)
            {
                once = true;
                WorldSectionLayer layer = character.section.layers[character.layerIndex];

                int i16 = digPOI.Value.y.intVal / 16;
                int j16 = digPOI.Value.x.intVal / 16;

                int i = i16 * 2;
                int j = j16 * 2;

                List<TileInstance?> tileInstances = [
                    layer.GetTileInstance(i, j),
                    layer.GetTileInstance(i, j + 1),
                    layer.GetTileInstance(i + 1, j),
                    layer.GetTileInstance(i + 1, j + 1)
                ];

                if (tileInstances.All(t => t != null && t.Value.collider == null))
                {
                    /*
                    layer.TransformTile(i, j, 6343);
                    layer.TransformTile(i, j + 1, 6346);
                    layer.TransformTile(i + 1, j, 6359);
                    layer.TransformTile(i + 1, j + 1, 6360);
                    */

                    character.PlaySound("dig");
                    FdPoint collectablePos = new FdPoint((j + 1) * 8, (i + 1) * 8);
                    Actor? pickup = character.section.CreateRandomPickup(collectablePos, true);
                    if (pickup is Collectable)
                    {
                        pickup.AddComponent(new VelComponent(pickup, new FdPoint(Fd.New(0, 75) * character.xDir, 0)));
                    }
                    if (pickup != null)
                    {
                        character.PlaySound("zol");
                    }
                }
                else
                {
                    character.PlaySound("tink");
                    new Anim(character, digPOI.Value, "hammer_hit");
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }
    }
}
