namespace Royale2D
{
    public class JoinMatchMenu : EnterTextMenu
    {
        static string lastTextUsed = Debug.quickStartMatchName;

        public JoinMatchMenu(Menu prevMenu) : 
            base(prevMenu, "JOIN MATCH", "Enter match name:", 15, null)
        {
            submitAction = OnSubmit;
            text = lastTextUsed;
        }

        public void OnSubmit(string input)
        {
            lastTextUsed = input;
            try
            {
                var onlineMatch = OnlineMatch.Join(input, Options.main.relayServerIp);
                Match.current = onlineMatch;
                // NETCODE late joiners
                ChangeMenu(new MatchLobbyMenu(this, onlineMatch));
            }
            catch (Exception e)
            {
                ChangeMenu(MessageMenu.CreateErrorMenu(this, title, e.Message));
            }
        }
    }
}