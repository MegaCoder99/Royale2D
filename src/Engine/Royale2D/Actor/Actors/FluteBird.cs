namespace Royale2D
{
    public class FluteBird : Actor
    {
        public ZComponent zComponent;
        public VelComponent velComponent;
        public ColliderComponent colliderComponent;

        public Character? carriedChar;
        public FdPoint startPos;

        public bool isDropOff;
        public bool shouldDrop;
        bool showChar;

        // REFACTOR remove
        public FluteBirdDevData devData;

        private FluteBird(Character creator, FdPoint pos, bool isDropOff, FluteBirdDevData devData) : base(creator, pos, "char_swag_duck")
        {
            this.devData = devData;
            this.isDropOff = isDropOff;
            velComponent = AddComponent(new VelComponent(this, new FdPoint(devData.GetInitXVel(), 0)));
            zComponent = AddComponent(new ZComponent(this, startZ: (devData.startZ / 2) - 16, zVel: devData.GetInitZVel(), zAcc: devData.zAcc));
            //Collider collider = new Collider(IntRect.CreateWHCentered(0, 0, 8, 8));
            colliderComponent = AddComponent(new ColliderComponent(this, createZCollider: true), disabledOnCreate: true);
            AddComponent(new ShadowComponent(this, ShadowType.Small));
            alpha = 0;
            startPos = pos;
            zLayerOffset = ZIndex.LayerOffsetEverythingAbove;
            showChar = isDropOff;
        }

        public static FluteBird New(Character creator, bool isDropOff)
        {
            FluteBirdDevData devData = Helpers.LoadDevData<FluteBirdDevData>("flute_bird");
            FdPoint createPos = creator.pos.AddXY(-devData.startX, 0);
            return new FluteBird(creator, createPos, isDropOff, devData);
        }

        public override void Update()
        {
            base.Update();

            if (!isDropOff)
            {
                colliderComponent.disabled = zComponent.z > 4;
            }
            else if (pos.x - startPos.x >= devData.startX)
            {
                showChar = false;
                shouldDrop = true;
            }

            PlaySound("keese", true);
            if (velComponent.distTravelled.x.intVal > Math.Abs(devData.startX) * 2)
            {
                DestroySelf();
            }
        }

        public override void RenderUpdate()
        {
            base.RenderUpdate();
            if (pos.x - startPos.x < devData.startAlphaZ)
            {
                alpha += (devData.alphaSpeed / 60f);
            }
            if (pos.x - startPos.x > (devData.startX * 2) - devData.startAlphaZ)
            {
                alpha -= (devData.alphaSpeed / 60f);
            }
        }

        public override List<string> GetDrawboxTagsToHide()
        {
            if (!showChar) return ["l"];
            return [];
        }

        public void PickupChar(Character character)
        {
            carriedChar = character;
            showChar = true;
        }
    }
}
