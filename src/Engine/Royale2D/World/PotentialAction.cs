namespace Royale2D
{
    public abstract class PotentialAction
    {
        public Character character;
        public PotentialAction(Character character)
        {
            this.character = character;
        }

        // Lower = higher priority, null = action not allowed
        protected virtual Fd? GetPriority()
        {
            return 0;
        }

        // Reason this exists is we want there to be actions that are actionable but result in error for user friendliness and those should be very low pri
        public Fd? GetPriorityExternal()
        {
            Fd? priority = GetPriority();
            if (priority == null) return null;
            if (GetErrorText() != "")
            {
                return priority + 100;
            }
            return priority;
        }

        protected virtual void Execute()
        {
        }

        public void ExecuteExternal()
        {
            string errorText = GetErrorText();
            if (errorText == "")
            {
                Execute();
            }
            else if (character.IsMainChar())
            {
                character.world.hud.SetAlert1(errorText);
                Game.PlaySound("error");
            }
        }

        protected Fd? GetTileClumpPriority(TileClumpInstance? nullableTileClumpInstance)
        {
            if (nullableTileClumpInstance == null) return null;
            TileClumpInstance tileClumpInstance = nullableTileClumpInstance.Value;

            FdPoint tileClumpCenterPos = tileClumpInstance.GetCenterPos();
            int pixelWidth = tileClumpInstance.GetPixelWidth();
            int pixelHeight = tileClumpInstance.GetPixelWidth();
            int distFromCenter;
            if (character.dir == Direction.Up || character.dir == Direction.Down)
            {
                distFromCenter = Math.Abs((tileClumpCenterPos.x - character.pos.x).intVal);
                if (distFromCenter > pixelWidth / 2) return null;
                return distFromCenter / (pixelWidth / 2);
            }
            else
            {
                distFromCenter = Math.Abs((tileClumpCenterPos.y - character.pos.y).intVal);
                if (distFromCenter > pixelHeight / 2) return null;
                return distFromCenter / (pixelHeight / 2);
            }
        }

        public string GetDisplayTextExternal()
        {
            if (GetErrorText() == "")
            {
                return GetDisplayText();
            }
            return "";
        }

        protected virtual string GetDisplayText()
        {
            return "";
        }

        // If there's an error text, the display text will be hidden, and the error text will be shown when trying to interact
        protected virtual string GetErrorText()
        {
            return "";
        }
    }

    public class LiftTileClumpPA : PotentialAction
    {
        public TileClumpInstance? nullableTileClumpInstance;
        public LiftTileClumpPA(Character character, TileCollision collision) : base(character)
        {
            string[] liftableTileClumps = [TileClumpTags.Liftable];
            nullableTileClumpInstance = collision.other.GetTileClumpInstance(liftableTileClumps);
        }

        protected override Fd? GetPriority()
        {
            if (!FeatureGate.lift)
            {
                return null;
            }

            if (nullableTileClumpInstance != null)
            {
                string tileClumpTags = nullableTileClumpInstance.Value.tileClump.tags;
                if (character.bunnyComponent.isBunny)
                {
                    return null;
                }
                else if (tileClumpTags.Contains(TileClumpTags.Sign) && character.dir == Direction.Up)
                {
                    return null;
                }
                else if (tileClumpTags.Contains(TileClumpTags.LiftableGlove1) && !character.inventory.HasItem(ItemType.powerGlove) && !character.inventory.HasItem(ItemType.titansMitt))
                {
                    return null;
                }
                else if (tileClumpTags.Contains(TileClumpTags.LiftableGlove2) && !character.inventory.HasItem(ItemType.powerGlove) && !character.inventory.HasItem(ItemType.titansMitt))
                {
                    return null;
                }
            }
            return GetTileClumpPriority(nullableTileClumpInstance);
        }

        protected override void Execute()
        {
            if (nullableTileClumpInstance == null) return;
            TileClumpInstance tileClumpInstance = nullableTileClumpInstance.Value;
            TileClump tileClump = tileClumpInstance.tileClump;

            /*
            string? overrideTransformClumpName = null;
            TileInstance? topLeftTileInstance = character.layer.GetTileInstance(tileClumpInstance.i1, tileClumpInstance.j1);
            if (topLeftTileInstance?.tileData?.tags == "reveal")
            {
                overrideTransformClumpName = tileClump.transformClumpName2;
            }
            */

            character.layer.TransformTileClump(tileClumpInstance);

            LiftedTile liftedTile = new LiftedTile(character, tileClumpInstance.GetCenterPos(), tileClump.liftSprite, tileClump.breakSprite, tileClump.breakSound);
            character.ChangeState(new LiftState(character, liftedTile.liftableComponent));
        }
    }

    public class LiftActorPA : PotentialAction
    {
        public LiftableComponent? liftableComponent;

        public LiftActorPA(Character character, ActorCollision collision) : base(character)
        {
            if (collision.other.actor.GetComponent<LiftableComponent>() is LiftableComponent lc && lc.CanBeLifted())
            {
                liftableComponent = lc;
            }
        }

        protected override Fd? GetPriority()
        {
            if (liftableComponent == null) return null;
            return liftableComponent.actor.pos.DistanceTo(character.pos) / 16;
        }

        protected override void Execute()
        {
            if (liftableComponent == null) return;
            character.ChangeState(new LiftState(character, liftableComponent));
        }
    }

    public class OpenChestPA : PotentialAction
    {
        public TileClumpInstance? nullableTileClumpInstance;
        public OpenChestPA(Character character, TileCollision collision) : base(character)
        {
            nullableTileClumpInstance = collision.other.GetTileClumpInstance(TileClumpTags.Chest);
        }

        public bool IsGamblingGame()
        {
            // REFACTOR tag sections in editor
            return (character.section.name == "house2" || character.section.name == "house19");
        }

        protected override Fd? GetPriority()
        {
            if (character.dir != Direction.Up) return null;
            return GetTileClumpPriority(nullableTileClumpInstance);
        }

        protected override string GetDisplayText()
        {
            if (IsGamblingGame())
            {
                return "open chest (20 rupees)";
            }
            return "";
        }

        protected override string GetErrorText()
        {
            if (IsGamblingGame() && character.rupees.value < 20)
            {
                return "Not enough rupees";
            }
            return "";
        }

        protected override void Execute()
        {
            if (nullableTileClumpInstance == null) return;
            TileClumpInstance tileClumpInstance = nullableTileClumpInstance.Value;

            character.PlaySound("chest open");

            character.layer.TransformTileClump(tileClumpInstance);

            if (IsGamblingGame())
            {
                character.rupees.DeductOverTime(20);

                // PERF this could be slow?
                TileData[,] tileGrid = character.layer.tileGrid;
                for (int i = 0; i < tileGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < tileGrid.GetLength(1); j++)
                    {
                        TileInstance? tileInstance = character.layer.GetTileInstance(i, j);
                        if (tileInstance != null)
                        {
                            TileClumpInstance? tileClumpInstance2 = tileInstance.Value.GetTileClumpInstanceFromTag(TileClumpTags.Chest);
                            if (tileClumpInstance2 != null)
                            {
                                character.layer.TransformTileClump(tileClumpInstance2.Value);
                            }
                        }
                    }
                }

                InventoryItem inventoryItem;

                int rand = NetcodeSafeRng.RandomRange(0, 2);
                if (rand == 0) inventoryItem = new InventoryItem(ItemType.greenRupee);
                else if (rand == 1) inventoryItem = new InventoryItem(ItemType.rupees20);
                else inventoryItem = new InventoryItem(ItemType.rupees50);

                character.inventory.CollectItem(inventoryItem);

                return;
            }
            else
            {
                Item randomItem = Items.GetRandomItem();
                InventoryItem inventoryItem = new InventoryItem(randomItem.itemType);
                if (!character.inventory.CollectItem(inventoryItem))
                {
                    var fieldItem = new FieldItem(character, tileClumpInstance.GetCenterPos(), inventoryItem, FdPoint.Zero, true);
                }
                else
                {
                    if (inventoryItem.itemType == ItemType.bow || inventoryItem.itemType == ItemType.silverBow)
                    {
                        // character.arrows.addImmediate(5);
                    }
                }
            }
        }
    }

    public class CollectFieldItemPA : PotentialAction
    {
        FieldItem? fieldItem;
        public CollectFieldItemPA(Character character, ActorCollision collision) : base(character)
        {
            if (collision.mine.collider.isWallCollider && collision.other.actor is FieldItem fieldItem)
            {
                this.fieldItem = fieldItem;
            }
        }

        protected override Fd? GetPriority()
        {
            if (fieldItem == null) return null;
            if (!character.inventory.HasEmptySlot()) return null;
            return fieldItem.pos.DistanceTo(character.pos) / 16;
        }

        protected override void Execute()
        {
            if (fieldItem == null) return;
            if (character.inventory.HasEmptySlot())
            {
                character.inventory.CollectItem(fieldItem.inventoryItem);
                fieldItem.DestroySelf();
            }
        }

        protected override string GetDisplayText()
        {
            string itemName = fieldItem?.inventoryItem?.item?.name ?? "";
            return itemName == "" ? "" : "pick up " + itemName;
        }
    }

    public class BuyItemPA : PotentialAction
    {
        ShopItem? shopItem;
        public BuyItemPA(Character character, ActorCollision collision) : base(character)
        {
            if (collision.mine.collider.isWallCollider && collision.other.actor is ShopItem shopItem)
            {
                this.shopItem = shopItem;
            }
        }

        protected override Fd? GetPriority()
        {
            if (shopItem == null) return null;
            return shopItem.pos.DistanceTo(character.pos) / 16;
        }

        protected override void Execute()
        {
            if (shopItem == null) return;

            int firstEmptyBottleSlot = character.inventory.GetFirstItemSlot(ItemType.emptyBottle);
            character.inventory.CollectItem(new InventoryItem(shopItem.itemType), firstEmptyBottleSlot);
            character.rupees.DeductOverTime(shopItem.price);
        }

        protected override string GetDisplayText()
        {
            if (shopItem != null)
            {
                return $"buy {shopItem.item.name} ({shopItem.price} rupees)";
            }
            return "";
        }

        protected override string GetErrorText()
        {
            if (character.rupees.value < shopItem?.price)
            {
                return "Not enough rupees";
            }
            else if (shopItem?.item?.itemToBecome == ItemType.emptyBottle)
            {
                if (!character.inventory.HasItem(ItemType.emptyBottle))
                {
                    return "Need empty bottle";
                }
            }
            else if (!character.inventory.HasEmptySlot())
            {
                return "Need empty slot";
            }
            return "";
        }
    }

    public class StartDialogPA : PotentialAction
    {
        public Dialog? dialog;
        public FdPoint dialogSourcePos;
        public DialogSourceComponent? dialogSourceComponent;
        public bool isSign;
        public StartDialogPA(Character character, ActorCollision actorCollision) : base(character)
        {
            if (actorCollision.other.actor.GetComponent<DialogSourceComponent>() is DialogSourceComponent dsc)
            {
                dialog = dsc.dialog;
                dialogSourceComponent = dsc;
                dialogSourcePos = dsc.actor.pos;
            }
        }

        public StartDialogPA(Character character, TileCollision tileCollision) : base(character)
        {
            if (tileCollision.other.GetTileClumpInstance(TileClumpTags.Sign) is TileClumpInstance tci && character.dir == Direction.Up)
            {
                List<string> tips = Tips.GetTip(tci);
                dialog = new Dialog(tips.ToArray());
                dialogSourcePos = tci.GetCenterPos();
                isSign = true;
            }
        }

        protected override Fd? GetPriority()
        {
            if (dialog == null) return null;
            if (dialogSourceComponent?.inUse == true) return null;
            return dialogSourcePos.DistanceTo(character.pos) / 16;
        }

        protected override void Execute()
        {
            if (dialog == null) return;
            character.ChangeState(new DialogState(character, dialog, dialogSourceComponent));
        }

        protected override string GetDisplayText()
        {
            return isSign ? "read" : "talk";
        }
    }

    public class PullMasterSwordPA : PotentialAction
    {
        MasterSwordWoods? msw;
        public PullMasterSwordPA(Character character, ActorCollision actorCollision) : base(character)
        {
            if (actorCollision.other.actor is MasterSwordWoods msw)
            {
                this.msw = msw;
            }
        }

        protected override Fd? GetPriority()
        {
            if (msw == null || msw.isPulling || msw.isPulled || character.dir != Direction.Down || !character.inventory.HasEmptySlot()) return null;
            return msw.pos.DistanceTo(character.pos) / 16;
        }

        protected override void Execute()
        {
            if (msw == null) return;
            character.ChangeState(new MasterSwordPullState(character, msw));
        }

        protected override string GetDisplayText()
        {
            return "claim Master Sword";
        }

        protected override string GetErrorText()
        {
            if (!character.inventory.HasEmptySlot())
            {
                return "No empty slots";
            }
            return "";
        }
    }
}
