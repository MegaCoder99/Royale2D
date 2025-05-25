namespace Royale2D
{
    public enum ItemType
    {
        sword1,
        sword2,
        sword3,
        sword4,
        shield1,
        shield2,
        shield3,
        greenMail,
        blueMail,
        redMail,
        greenPendant,
        bluePendant,
        redPendant,
        powerGlove,
        titansMitt,
        flippers,
        moonPearl,
        pegasusBoots,
        bombs,
        bow,
        silverBow,
        boomerang,
        magicBoomerang,
        hookshot,
        firerod,
        icerod,
        hammer,
        net,
        lamp,
        caneOfSomaria,
        caneOfBryana,
        cape,
        powder,
        ether,
        quake,
        bombos,
        emptyBottle,
        bottledFairy,
        bottledBee,
        greenPotion,
        redPotion,
        bluePotion,
        flute,
        shovel,
        heartPiece,
        heartContainer,
        greenRupee,
        blueRupee,
        redRupee,
        purpleRupee,
        rupees20,
        rupees50,
        rupees100,
        rupees300,
        arrows10,
        arrows30
    }

    public static class Items
    {
        public static Dictionary<ItemType, Item> items = new Dictionary<ItemType, Item>();

        public static void Init()
        {
            items[ItemType.sword1] = new Item(ItemType.sword1, 0, "Fighter's Sword", 100);
            items[ItemType.sword2] = new Item(ItemType.sword2, 1, "Master Sword", 0);
            items[ItemType.sword3] = new Item(ItemType.sword3, 2, "Tempered Sword", 10);
            items[ItemType.sword4] = new Item(ItemType.sword4, 3, "Golden Sword", 5);
            items[ItemType.shield1] = new Item(ItemType.shield1, 4, "Fighter's Shield", 100);
            items[ItemType.shield2] = new Item(ItemType.shield2, 5, "Magic Shield", 25);
            items[ItemType.shield3] = new Item(ItemType.shield3, 6, "Mirror Shield", 5);
            items[ItemType.greenMail] = new Item(ItemType.greenMail, 7, "Green Mail", 0);
            items[ItemType.blueMail] = new Item(ItemType.blueMail, 8, "Blue Main", 10);
            items[ItemType.redMail] = new Item(ItemType.redMail, 9, "Red Mail", 5);
            items[ItemType.greenPendant] = new Item(ItemType.greenPendant, 45, "Pendant of Courage", 0);
            items[ItemType.bluePendant] = new Item(ItemType.bluePendant, 46, "Pendant of Wisdom", 0);
            items[ItemType.redPendant] = new Item(ItemType.redPendant, 47, "Pendant of Power", 0);
            items[ItemType.powerGlove] = new Item(ItemType.powerGlove, 11, "Power Glove", 100);
            items[ItemType.titansMitt] = new Item(ItemType.titansMitt, 12, "Titan's Mitt", 50);
            items[ItemType.flippers] = new Item(ItemType.flippers, 13, "Flippers", 50);
            items[ItemType.moonPearl] = new Item(ItemType.moonPearl, 14, "Moon Pearl", 50);
            items[ItemType.pegasusBoots] = new Item(ItemType.pegasusBoots, 10, "Pegasus Boots", 25);
            items[ItemType.bombs] = new Item(ItemType.bombs, 20, "Bombs", 100, true, 30, useAction: Bomb.OnUseBomb);
            items[ItemType.bow] = new Item(ItemType.bow, 16, "Bow", 50, useAction: (c) => c.ChangeState(new BowState(c)));
            items[ItemType.silverBow] = new Item(ItemType.silverBow, 17, "Bow", 5, useAction: (c) => c.ChangeState(new BowState(c)));
            items[ItemType.boomerang] = new Item(ItemType.boomerang, 18, "Boomerang", 100, useAction: (c) => c.ChangeState(new BoomerangState(c, false)));
            items[ItemType.magicBoomerang] = new Item(ItemType.magicBoomerang, 19, "Magic Boomerang", 50, useAction: (c) => c.ChangeState(new BoomerangState(c, true)));
            items[ItemType.hookshot] = new Item(ItemType.hookshot, 21, "Hookshot", 25, useAction: (c) => c.ChangeState(new HookshotState(c)));
            items[ItemType.firerod] = new Item(ItemType.firerod, 23, "Fire Rod", 25, useAction: (c) => c.ChangeState(new FireRodState(c)));
            items[ItemType.icerod] = new Item(ItemType.icerod, 24, "Ice Rod", 25, useAction: (c) => c.ChangeState(new IceRodState(c)));
            items[ItemType.hammer] = new Item(ItemType.hammer, 29, "Hammer", 10, useAction: (c) => c.ChangeState(new HammerState(c)));
            items[ItemType.net] = new Item(ItemType.net, 32, "Net", 100, useAction: (c) => c.ChangeState(new BugNetState(c)));
            items[ItemType.lamp] = new Item(ItemType.lamp, 28, "Lamp", 100, useAction: (c) => c.ChangeState(new LampState(c)));
            items[ItemType.caneOfSomaria] = new Item(ItemType.caneOfSomaria, 34, "Cane of Somaria", 25, useAction: (c) => c.ChangeState(new CaneOfSomariaState(c)));
            items[ItemType.caneOfBryana] = new Item(ItemType.caneOfBryana, 35, "Cane of Bryana", 5, useAction: CaneOfBryanaState.OnUse);
            items[ItemType.cape] = new Item(ItemType.cape, 36, "Cape", 10, useAction: CapeState.OnUse);
            items[ItemType.powder] = new Item(ItemType.powder, 38, "Magic Powder", 50, useAction: (c) => c.ChangeState(new MagicPowderState(c)));
            items[ItemType.ether] = new Item(ItemType.ether, 25, "Ether", 10, useAction: (c) => c.ChangeState(new EtherState(c)));
            items[ItemType.quake] = new Item(ItemType.quake, 26, "Quake", 10, useAction: (c) => c.ChangeState(new QuakeState(c)));
            items[ItemType.bombos] = new Item(ItemType.bombos, 27, "Bombos", 10, useAction: (c) => c.ChangeState(new BombosState(c)));
            items[ItemType.emptyBottle] = new Item(ItemType.emptyBottle, 39, "Empty Bottle", 50);
            items[ItemType.bottledFairy] = new Item(ItemType.bottledFairy, 40, "Bottled Fairy", 10, itemToBecome: ItemType.emptyBottle, useAction: Fairy.OnUseBottledFairy);
            items[ItemType.bottledBee] = new Item(ItemType.bottledBee, 44, "Bottled Bee", 10, itemToBecome: ItemType.emptyBottle, useAction: Bee.OnUseBottledBee);
            items[ItemType.greenPotion] = new Item(ItemType.greenPotion, 41, "Green Potion", 25, shopSpriteIndex: 58, itemToBecome: ItemType.emptyBottle, useAction: (c) => c.ChangeState(new PotionState(c, false, true)));
            items[ItemType.redPotion] = new Item(ItemType.redPotion, 42, "Red Potion", 10, shopSpriteIndex: 57, itemToBecome: ItemType.emptyBottle, useAction: (c) => c.ChangeState(new PotionState(c, true, false)));
            items[ItemType.bluePotion] = new Item(ItemType.bluePotion, 43, "Blue Potion", 5, shopSpriteIndex: 59, itemToBecome: ItemType.emptyBottle, useAction: (c) => c.ChangeState(new PotionState(c, true, true)));
            items[ItemType.flute] = new Item(ItemType.flute, 31, "Flute", 5, useAction: (c) => c.ChangeState(new FluteState(c)));
            items[ItemType.shovel] = new Item(ItemType.shovel, 30, "Shovel", 50, useAction: (c) => c.ChangeState(new ShovelState(c)));
            items[ItemType.heartPiece] = new Item(ItemType.heartPiece, 48, "Heart Piece", 25, true, 400);
            items[ItemType.heartContainer] = new Item(ItemType.heartContainer, 49, "Heart Container", 10, false, 1, true);
            items[ItemType.greenRupee] = new Item(ItemType.greenRupee, 62, "Green Rupee", 100, false, 1, true);
            items[ItemType.blueRupee] = new Item(ItemType.blueRupee, 50, "Blue Rupee", 100, false, 1, true);
            items[ItemType.redRupee] = new Item(ItemType.redRupee, 51, "Red Rupee", 50, false, 1, true);
            items[ItemType.purpleRupee] = new Item(ItemType.purpleRupee, 52, "Purple Rupee", 25, false, 1, true);
            items[ItemType.rupees20] = new Item(ItemType.rupees20, 60, "20 Rupees", 10, false, 1, true);
            items[ItemType.rupees50] = new Item(ItemType.rupees50, 61, "50 Rupees", 10, false, 1, true);
            items[ItemType.rupees100] = new Item(ItemType.rupees100, 53, "100 Rupees", 10, false, 1, true);
            items[ItemType.rupees300] = new Item(ItemType.rupees300, 54, "300 Rupees", 5, false, 1, true);
            items[ItemType.arrows10] = new Item(ItemType.arrows10, 55, "10 Arrows", 100, false, 1, true);
            items[ItemType.arrows30] = new Item(ItemType.arrows30, 56, "30 Arrows", 25, false, 1, true);

            // Items that should be more common than their rarity ranks, for balance's sake
            items[ItemType.heartPiece].spawnOddsOverride = 1000;
            items[ItemType.heartContainer].spawnOddsOverride = 100;
            items[ItemType.sword1].spawnOddsOverride = 1000;
            items[ItemType.bow].spawnOddsOverride = 1000;
            items[ItemType.lamp].spawnOddsOverride = 1000;

            if (FeatureGate.allowedItems.Count > 0)
            {
                items[ItemType.boomerang].spawnOddsOverride = 1000;
                items[ItemType.arrows10].spawnOddsOverride = 2500;
            }
        }

        public static Item GetRandomItem()
        {
            // Common = 100
            // Uncommon = 50
            // Rare = 25
            // Epic = 10
            // Legendary = 5

            var itemsList = items.Values.ToList();

            if (FeatureGate.allowedItems.Count > 0)
            {
                itemsList = itemsList.Where(item => FeatureGate.allowedItems.Contains(item.itemType)).ToList();
            }

            int totalWeight = 0;
            foreach (Item item in itemsList)
            {
                if (item.spawnOddsOverride == 0)
                {
                    totalWeight += item.spawnOddsWeight;
                }
                else
                {
                    totalWeight += item.spawnOddsOverride;
                }
            }
            int rand = NetcodeSafeRng.RandomRange(0, totalWeight);
            int previousOdds = 0;
            int currentOdds = 0;
            foreach (Item item in itemsList)
            {
                if (item.spawnOddsOverride == 0)
                {
                    currentOdds += item.spawnOddsWeight;
                }
                else
                {
                    currentOdds += item.spawnOddsOverride;
                }
                if (rand >= previousOdds && rand < currentOdds) return item;
                previousOdds = currentOdds;
            }
            return itemsList[NetcodeSafeRng.RandomRange(0, itemsList.Count - 1)];
        }
    }
}
