namespace Royale2D
{
    public class KillFeedEntry
    {
        public int time;
        public string? text;
        public Character? victim;
        public Character? killer;
        public DamagerType? damagerType;

        public Damager? damager => damagerType != null ? Damagers.damagers[damagerType.Value] : null;

        public KillFeedEntry(string text)
        {
            this.text = text;
        }

        public KillFeedEntry(DamagerType damagerType, Character victim, Character? killer)
        {
            this.damagerType = damagerType;
            this.victim = victim;
            this.killer = killer;

            if (damager != null && damager.Value.killFeedSuffix != "")
            {
                text = victim.player.name + " " + damager.Value.killFeedSuffix;
            }
        }

        public void Render(Drawer drawer, Point pos)
        {
            // Custom text scenario like "Player claimed master sword"
            if (text != null)
            {
                drawer.DrawText(text, pos.x, pos.y, AlignX.Left, fontType: FontType.Small, letterSpacing: -1);
            }
            // Scenario 1: player A kills player B. Do [PLAYER A] [DAMAGER ICON] [PLAYER B]
            else if (damager != null && victim != null && killer != null && victim != killer)
            {
                float killerNameWidth = drawer.DrawText(killer.player.name, pos.x, pos.y, AlignX.Left, AlignY.Middle, fontType: FontType.Small, letterSpacing: -1);
                (string spriteName, int frameIndex) = damager.Value.GetKillfeedSpriteAndIndex();
                Sprite sprite = Assets.GetSprite(spriteName);
                int spriteWidth = sprite.frames[frameIndex].rect.w;
                float spritePosXOff = killerNameWidth + (spriteWidth / 2) + 5;
                float victimPosXOff = spritePosXOff + (spriteWidth / 2) + 5;
                sprite.Render(drawer, pos.x + spritePosXOff, pos.y, frameIndex);
                drawer.DrawText(victim.player.name, pos.x + victimPosXOff, pos.y, AlignX.Left, AlignY.Middle, fontType: FontType.Small, letterSpacing: -1);
            }
            // Scenario 2: player A kills themself or dies from env damage. Just do [DAMAGER ICON] [PLAYER]
            else if (damager != null && victim != null && ((killer != null && victim == killer) || (killer == null)))
            {
                (string spriteName, int frameIndex) = damager.Value.GetKillfeedSpriteAndIndex();
                Sprite sprite = Assets.GetSprite(spriteName);
                int spriteWidth = sprite.frames[frameIndex].rect.w;
                float spritePosXOff = (spriteWidth / 2) + 5;
                float victimPosXOff = spritePosXOff + (spriteWidth / 2) + 5;
                sprite.Render(drawer, pos.x + spritePosXOff, pos.y, frameIndex);
                drawer.DrawText(victim.player.name, pos.x + victimPosXOff, pos.y, AlignX.Left, AlignY.Middle, fontType: FontType.Small, letterSpacing: -1);
            }
        }
    }
}
