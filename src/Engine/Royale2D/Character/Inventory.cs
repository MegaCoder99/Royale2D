using Shared;

namespace Royale2D
{
    public class Inventory
    {
        public List<InventoryItem?> items = Debug.main?.GetDebugItems() ?? [null, null, null, null, null];
        public int selectedItemIndex;
        public Character character;
        public int itemGetTime;
        public int itemGetSpriteIndex;

        public InventoryItem? selectedItem => items[selectedItemIndex];

        public Inventory(Character character)
        {
            this.character = character;
        }

        public void Update()
        {
            if (itemGetTime > 0)
            {
                itemGetTime++;
                if (itemGetTime > 60)
                {
                    itemGetTime = 0;
                }
            }

            if (character.input.IsPressed(Control.ItemLeft))
            {
                Helpers.DecCycle(ref selectedItemIndex, items.Count);
            }
            else if (character.input.IsPressed(Control.ItemRight))
            {
                Helpers.IncCycle(ref selectedItemIndex, items.Count);
            }
        }

        public void Render(Drawer drawer)
        {
            if (itemGetTime > 0)
            {
                float offsetY = itemGetTime > 30 ? 0.5f : (itemGetTime / 60f);
                FdPoint charPos = character.GetRenderPos();
                Assets.GetSprite("field_item").Render(drawer, charPos.x.floatVal, charPos.y.floatVal - 5 - offsetY * 24, itemGetSpriteIndex, ZIndex.UIGlobal);
            }
        }

        public bool CollectItem(InventoryItem inventoryItem, int overwriteSlot = -1)
        {
            bool collected = false;
            bool alreadyAnimed = false;
            if (inventoryItem.item.usesQuantity)
            {
                int firstIndex = -1;
                for (int i = 0; i < items.Count; i++)
                {
                    InventoryItem? item = items[i];
                    if (item != null && item.item == inventoryItem.item && !item.IsMaxed())
                    {
                        int loop = 0;
                        while (!item.IsMaxed() && inventoryItem.quantity > 0)
                        {
                            loop++; if (loop > 10000) { throw new Exception("INFINITE LOOP IN CHARACTER COLLECT"); }
                            if (firstIndex == -1) firstIndex = i;
                            item.quantity++;
                            inventoryItem.quantity--;
                        }
                    }
                }

                InventoryItem? firstIndexItem = items.SafeGet(firstIndex);
                if (inventoryItem.item.itemType == ItemType.heartPiece && firstIndexItem != null && firstIndexItem.quantity >= 4)
                {
                    firstIndexItem.quantity -= 4;
                    if (firstIndexItem.quantity == 0) items[firstIndex] = null;
                    inventoryItem.itemType = ItemType.heartContainer;

                    inventoryItem.quantity = 1;
                    collected = true;
                    AddItem(inventoryItem, 0);
                }
                else if (inventoryItem.quantity == 0)
                {
                    collected = true;
                }

                if (firstIndex != -1)
                {
                    itemGetSpriteIndex = inventoryItem.item.spriteIndex;
                    itemGetTime = 1;
                    character.PlaySound("item get 1");
                    alreadyAnimed = true;
                }

            }

            int slot = GetEmptySlot();
            if (overwriteSlot >= 0)
            {
                slot = overwriteSlot;
                items[slot] = null;
            }
            if (slot == -1 && !inventoryItem.item.immediate)
            {
                return collected;
            }

            if (inventoryItem.item.immediate || items[slot] == null)
            {
                if (!alreadyAnimed)
                {
                    AddItem(inventoryItem, slot);
                    itemGetSpriteIndex = inventoryItem.item.spriteIndex;
                    itemGetTime = 1;
                    collected = true;
                    character.PlaySound("item get 1");
                }
            }

            return collected;
        }

        public void AddItem(InventoryItem inventoryItem, int slot)
        {
            if (inventoryItem.item.immediate)
            {
                if (inventoryItem.itemType == ItemType.heartContainer)
                {
                    character.health.IncreaseMaxValue(4);
                    character.health.AddOverTime(4);
                }
                if (inventoryItem.itemType == ItemType.arrows10) character.arrows.AddImmediate(10);
                if (inventoryItem.itemType == ItemType.arrows30) character.arrows.AddImmediate(30);
                if (inventoryItem.itemType == ItemType.greenRupee) character.rupees.AddOverTime(1);
                if (inventoryItem.itemType == ItemType.blueRupee) character.rupees.AddOverTime(5);
                if (inventoryItem.itemType == ItemType.redRupee) character.rupees.AddOverTime(20);
                if (inventoryItem.itemType == ItemType.purpleRupee) character.rupees.AddOverTime(50);
                if (inventoryItem.itemType == ItemType.rupees20) character.rupees.AddOverTime(20);
                if (inventoryItem.itemType == ItemType.rupees50) character.rupees.AddOverTime(50);
                if (inventoryItem.itemType == ItemType.rupees100) character.rupees.AddOverTime(100);
                if (inventoryItem.itemType == ItemType.rupees300) character.rupees.AddOverTime(300);
                return;
            }

            items[slot] = inventoryItem;
        }

        public void DropSelectedItem(int slot)
        {
            if (items[slot] == null) return;
            InventoryItem inventoryItem = items[slot]!;

            FieldItem fieldItem = new FieldItem(character, character.GetCenterPos(), inventoryItem, Helpers.DirToFdVec(character.dir), true);
            items[slot] = null;

            character.PlaySound("throw");
        }

        public void DropRupees(int amount)
        {
            if (amount == 0 || amount > character.rupees.value) return;
            character.rupees.DeductImmediate(amount);
            Collectables.CreateRupeeLoot(character.section, character.GetCenterPos(), Helpers.DirToFdVec(character.dir), (int)character.rupees.value);
            character.PlaySound("throw");
        }

        public void DropArrows(int amount)
        {
            if (amount == 0 || amount > character.arrows.value) return;
            character.arrows.DeductImmediate(amount);
            Collectables.CreateArrowLoot(character.section, character.GetCenterPos(), Helpers.DirToFdVec(character.dir), (int)character.arrows.value);
            character.PlaySound("throw");
        }

        public bool HasItem(ItemType itemType)
        {
            if (itemType == ItemType.flippers ||
                itemType == ItemType.pegasusBoots ||
                itemType == ItemType.powerGlove ||
                itemType == ItemType.titansMitt)
            {
                if (Debug.main?.hasEverything == true) return true;
            }
            if (itemType == ItemType.sword1 && Debug.main?.sword == 1) return true;
            if (itemType == ItemType.sword2 && Debug.main?.sword == 2) return true;
            if (itemType == ItemType.sword3 && Debug.main?.sword == 3) return true;
            if (itemType == ItemType.sword4 && Debug.main?.sword == 4) return true;
            return items.Any(i => i?.item.itemType == itemType);
        }

        public bool HasSword()
        {
            return HasItem(ItemType.sword1) || HasItem(ItemType.sword2) || HasItem(ItemType.sword3) || HasItem(ItemType.sword4);
        }

        public int SwordLevel()
        {
            if (Debug.main?.sword != null) return Debug.main.sword.Value;
            if (HasItem(ItemType.sword4)) return 4;
            if (HasItem(ItemType.sword3)) return 3;
            if (HasItem(ItemType.sword2)) return 2;
            if (HasItem(ItemType.sword1)) return 1;
            return 0;
        }

        public bool HasShield()
        {
            return HasItem(ItemType.shield1) || HasItem(ItemType.shield2) || HasItem(ItemType.shield3);
        }

        public int ShieldLevel()
        {
            if (Debug.main?.shield != null) return Debug.main.shield.Value;
            if (HasItem(ItemType.shield3)) return 3;
            if (HasItem(ItemType.shield2)) return 2;
            if (HasItem(ItemType.shield1)) return 1;
            return 0;
        }

        public int GetEmptySlot()
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetFirstItemSlot(ItemType itemType)
        {
            return items.FindIndex(i => i?.itemType == itemType);
        }

        public bool HasEmptySlot()
        {
            return GetEmptySlot() != -1;
        }

        public bool TransformFirstItem(ItemType from, ItemType to)
        {
            int index = items.FindIndex(i => i?.item.itemType == from);
            if (index == -1) return false;
            items[index] = new InventoryItem(to);
            return true;
        }

        public void UseSelectedItem()
        {
            if (selectedItem == null) return;
            
            if (selectedItem.item.usesQuantity) selectedItem.quantity--;
            selectedItem.item.useAction?.Invoke(character);
            
            if (selectedItem.item.itemToBecome != null)
            {
                items[selectedItemIndex] = new InventoryItem(selectedItem.item.itemToBecome.Value);
            }
            else if (selectedItem.quantity == 0)
            {
                items[selectedItemIndex] = null;
            }
        }

        public DamagerType? GetSwordDamagerType()
        {
            if (HasItem(ItemType.sword4)) return DamagerType.sword4;
            if (HasItem(ItemType.sword3)) return DamagerType.sword3;
            if (HasItem(ItemType.sword2)) return DamagerType.sword2;
            if (HasItem(ItemType.sword1)) return DamagerType.sword1;
            return null;
        }

        public DamagerType? GetSwordSpinDamagerType()
        {
            if (HasItem(ItemType.sword4)) return DamagerType.spinAttack4;
            if (HasItem(ItemType.sword3)) return DamagerType.spinAttack3;
            if (HasItem(ItemType.sword2)) return DamagerType.spinAttack2;
            if (HasItem(ItemType.sword1)) return DamagerType.spinAttack1;
            return null;
        }
    }
}
