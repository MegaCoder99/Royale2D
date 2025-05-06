namespace Royale2D
{
    public class Dialog
    {
        public List<string> pages;

        public Dialog(params string[] pagesParams)
        {
            pages = pagesParams.ToList();
        }
    }

    public class DialogSourceComponent : Component
    {
        public Dialog dialog;
        public bool inUse;

        public DialogSourceComponent(Actor actor, Dialog dialog) : base(actor)
        {
            this.dialog = dialog;
        }
    }
}
