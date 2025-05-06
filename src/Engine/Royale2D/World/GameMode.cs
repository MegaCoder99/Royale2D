namespace Royale2D
{
    public class GameMode
    {
        public World world;
        public List<Character> winners = new List<Character>();
        public int winTime;

        public List<Character> characters => world.characters;

        public GameMode(World world)
        {
            this.world = world;
        }

        public void Update()
        {
            if (winners.Count == 0)
            {
                winners = GetWinners();
            }
            else
            {
                winTime++;
            }
        }

        public List<Character> GetWinners()
        {
            // NETCODE ignore spectators, players that left, etc
            if (characters.Count < 2) return [];

            List<Character> charsAlive = characters.Where(c => c.IsAlive()).ToList();

            if (charsAlive.Count == 1)
            {
                return charsAlive;
            }

            return [];
        }

        public bool IsOver()
        {
            return winners.Count > 0;
        }

        public bool IsWinner(Character character)
        {
            return winners.Any(c => c == character);
        }

        public int GetPlace(Character character)
        {
            List<Character> charsAlive = characters.Where(c => c.IsAlive() && c != character).ToList();
            return charsAlive.Count + 1;
        }

        public int GetCharactersLeft()
        {
            return characters.Where(c => c.IsAlive()).Count();
        }

        public bool IsOverTransition()
        {
            return IsOver() && !ShowOverLeave();
        }

        public bool ShowOverLeave()
        {
            return winTime > 600;
        }
    }
}
