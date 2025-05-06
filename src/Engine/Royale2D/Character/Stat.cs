namespace Royale2D
{
    public enum StatType
    {
        Health,
        Magic,
        Rupee,
        Bomb,
        Arrow
    }

    public class Stat
    {
        public int value;
        public int maxValue;
        public int amountToAdd;
        public int addCooldown;
        public int addIncAmount;
        public int addTime;
        public int maxValueCap = 1000000;
        public StatType statType;
        public Character character;
        int soundTime;
        int deductRate;

        public Stat(StatType statType, Character character, int startValue, int maxValue, int addCooldown, int addIncAmount = 1, int maxValueCap = 1000000)
        {
            this.statType = statType;
            this.character = character;
            this.value = startValue;
            this.maxValue = maxValue;
            this.addCooldown = addCooldown;
            this.addIncAmount = addIncAmount;
            this.maxValueCap = maxValueCap;
        }

        public void Update()
        {
            if (amountToAdd != 0)
            {
                addTime++;
                if (addTime >= addCooldown)
                {
                    addTime = 0;
                    for (int i = 0; i < addIncAmount; i++)
                    {
                        if (amountToAdd > 0)
                        {
                            amountToAdd--;
                            value++;
                        }
                        else if (amountToAdd < 0)
                        {
                            amountToAdd++;
                            value--;
                        }
                        if (value > maxValue)
                        {
                            value = maxValue;
                            amountToAdd = 0;
                            break;
                        }
                        else if (value < 0)
                        {
                            value = 0;
                            amountToAdd = 0;
                            break;
                        }
                    }
                    if (statType == StatType.Health) character.PlaySoundIfMain("life refill");
                    else if (statType == StatType.Magic) character.PlaySoundIfMain("magic meter 1");
                }
                if (statType == StatType.Rupee)
                {
                    if (soundTime == 0)
                    {
                        character.PlaySoundIfMain("wallet 1");
                    }
                    soundTime++;
                    if (soundTime > 5) soundTime = 0;
                }
            }
            else
            {
                soundTime = 0;
            }
        }

        public void AddImmediate(int amount)
        {
            value += amount;
            if (value > maxValue) value = maxValue;
        }

        public void AddOverTime(int amount)
        {
            amountToAdd += amount;
            addTime = addCooldown;
        }

        public void DeductImmediate(int amount)
        {
            value -= amount;
            if (value < 0) value = 0;
        }

        public void DeductOverTime(int amount)
        {
            amountToAdd -= amount;
            addTime = addCooldown;
        }

        // Unlike DeductOverTime, this is to be called every frame
        public void DeductAtRate(int everyNFrames)
        {
            deductRate++;
            if (deductRate >= everyNFrames)
            {
                deductRate = 0;
                DeductImmediate(1);
            }
        }

        public void FillMax()
        {
            int rest = maxValue - value;
            AddOverTime(rest);
        }

        public bool IsMax()
        {
            return value == maxValue;
        }

        public bool IsChanging()
        {
            return amountToAdd > 0;
        }

        public void IncreaseMaxValue(int amount)
        {
            if (maxValue >= maxValueCap) return;
            maxValue += amount;
        }
    }
}
