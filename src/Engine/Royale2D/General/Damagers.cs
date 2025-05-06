namespace Royale2D
{
    public enum DamagerType
    {
        debug,
        debugKill,
        bomb,
        boomerang,
        storm,
        stormBunnifier,
        hookshot,
        arrow,
        sword1,
        sword2,
        sword3,
        sword4,
        hammer,
        spinAttack1,
        spinAttack2,
        spinAttack3,
        spinAttack4,
        swordBeam,
        fireRod,
        iceRod,
        lamp,
        magicPowder,
        water,
        caneBlock,
        caneBlockProj,
        bee,
        cuccoThrow,
        bush,
        sign,
        rockSmallGray,
        rockBigGray,
        rockSmallBlack,
        rockBigBlack
    };

    public class Damagers
    {
        public static Dictionary<DamagerType, Damager> damagers = new Dictionary<DamagerType, Damager>();

        public static void Init()
        {
            damagers[DamagerType.debug] = new Damager(DamagerType.debug, "debug", "0.5");
            damagers[DamagerType.debugKill] = new Damager(DamagerType.debugKill, "debug kill", "1000");
            damagers[DamagerType.bomb] = new Damager(DamagerType.bomb, "bomb", "1", item: ItemType.bombs, selfDamage: true, hitFrozen: true);
            damagers[DamagerType.boomerang] = new Damager(DamagerType.boomerang, "boomerang", "0", item: ItemType.boomerang, stun: true);
            damagers[DamagerType.storm] = new Damager(DamagerType.storm, "storm", "0.5", damageCooldown: 60, flinch: false, hitFrozen: true, killFeedSuffix: "was engulfed in Twilight");
            damagers[DamagerType.stormBunnifier] = new Damager(DamagerType.stormBunnifier, "storm bunnifier", "0", bunnyTime: 5);
            damagers[DamagerType.hookshot] = new Damager(DamagerType.hookshot, "hookshot", "0", item: ItemType.hookshot, stun: true);
            damagers[DamagerType.arrow] = new Damager(DamagerType.arrow, "arrow", "0.5", item: ItemType.bow);
            damagers[DamagerType.sword1] = new Damager(DamagerType.sword1, "sword 1", "0.5", item: ItemType.sword1);
            damagers[DamagerType.sword2] = new Damager(DamagerType.sword2, "sword 2", "1", item: ItemType.sword2);
            damagers[DamagerType.sword3] = new Damager(DamagerType.sword3, "sword 3", "1.5", item: ItemType.sword3);
            damagers[DamagerType.sword4] = new Damager(DamagerType.sword4, "sword 4", "2", item: ItemType.sword4);
            damagers[DamagerType.hammer] = new Damager(DamagerType.hammer, "hammer", "1", item: ItemType.hammer, hitFrozen: true);
            damagers[DamagerType.spinAttack1] = new Damager(DamagerType.spinAttack1, "spin attack 1", "1", item: ItemType.sword1);
            damagers[DamagerType.spinAttack2] = new Damager(DamagerType.spinAttack2, "spin attack 2", "2", item: ItemType.sword2);
            damagers[DamagerType.spinAttack3] = new Damager(DamagerType.spinAttack3, "spin attack 3", "3", item: ItemType.sword3);
            damagers[DamagerType.spinAttack4] = new Damager(DamagerType.spinAttack4, "spin attack 4", "4", item: ItemType.sword4);
            damagers[DamagerType.swordBeam] = new Damager(DamagerType.swordBeam, "sword beam", "0.5", item: ItemType.sword2);
            damagers[DamagerType.fireRod] = new Damager(DamagerType.fireRod, "fire rod", "0.5", item: ItemType.firerod, burnTime: 120);
            damagers[DamagerType.iceRod] = new Damager(DamagerType.iceRod, "ice rod", "0", item: ItemType.icerod, freeze: true);
            damagers[DamagerType.lamp] = new Damager(DamagerType.lamp, "lamp", "0.5", item: ItemType.lamp, burnTime: 120);
            damagers[DamagerType.magicPowder] = new Damager(DamagerType.magicPowder, "magic powder", "0", item: ItemType.powder, bunnyTime: 240);
            damagers[DamagerType.water] = new Damager(DamagerType.water, "water", "0.5", killFeedSuffix: "drowned");
            damagers[DamagerType.caneBlock] = new Damager(DamagerType.caneBlock, "cane block", "0.5", false, item: ItemType.caneOfSomaria);
            damagers[DamagerType.caneBlockProj] = new Damager(DamagerType.caneBlockProj, "cane block proj", "0.5", false, item: ItemType.caneOfSomaria);
            damagers[DamagerType.bee] = new Damager(DamagerType.bee, "bee", "0.5", killFeedSuffix: "was slain by a bee");
            damagers[DamagerType.cuccoThrow] = new Damager(DamagerType.cuccoThrow, "cucco", "0.5", killFeedSuffix: "chickened out");
            damagers[DamagerType.bush] = new Damager(DamagerType.bush, "bush", "0.5");
            damagers[DamagerType.sign] = new Damager(DamagerType.sign, "sign", "1");
            damagers[DamagerType.rockSmallGray] = new Damager(DamagerType.rockSmallGray, "rock gray small", "1");
            damagers[DamagerType.rockBigGray] = new Damager(DamagerType.rockBigGray, "rock gray big", "2");
            damagers[DamagerType.rockSmallBlack] = new Damager(DamagerType.rockSmallBlack, "rock black small", "1.5");
            damagers[DamagerType.rockBigBlack] = new Damager(DamagerType.rockBigBlack, "rock black big", "2.5");
        }
    }
}
