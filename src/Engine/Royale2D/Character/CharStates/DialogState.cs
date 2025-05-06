using Shared;

namespace Royale2D
{
    public class DialogState : CharState
    {
        public List<string> pages;
        public int pageNum;
        bool isBottleSalesman;
        bool isFortuneTeller;
        bool paidFortuneTeller;
        DialogSourceComponent? dialogSourceComponent;
        
        public string? fullLine1;
        public string? fullLine2;
        public string? fullLine3;

        int line1Progress;
        int line2Progress;
        int line3Progress;

        public string? line1 => fullLine1?.Substring(0, line1Progress);
        public string? line2 => fullLine2?.Substring(0, line2Progress);
        public string? line3 => fullLine3?.Substring(0, line3Progress);
        Actor? dialogSourceActor => dialogSourceComponent?.actor;

        public DialogState(Character character, Dialog dialog, DialogSourceComponent? dialogSourceComponent) : base(character)
        {
            baseSpriteName = "char_idle";
            pages = new List<string>(dialog.pages);
            this.dialogSourceComponent = dialogSourceComponent;
            isBottleSalesman = dialogSourceActor?.spriteName == "npc_bottle_salesman";
            isFortuneTeller = dialogSourceActor?.spriteName == "npc_fortune_teller";
            canEnterAsBunny = true;

            NextPage();
        }

        public override void Update()
        {
            base.Update();

            if (fullLine1 != null && line1Progress < fullLine1.Length)
            {
                line1Progress++;
            }
            else if (fullLine2 != null && line2Progress < fullLine2.Length)
            {
                line2Progress++;
            }
            else if (fullLine3 != null && line3Progress < fullLine3.Length)
            {
                line3Progress++;
            }
            else
            {
                if (input.IsPressed(Control.Action))
                {
                    pageNum++;
                    line1Progress = 0;
                    line2Progress = 0;
                    line3Progress = 0;
                    if (pageNum >= pages.Count)
                    {
                        if (isBottleSalesman)
                        {
                            isBottleSalesman = false;
                            if (character.rupees.value >= 40)
                            {
                                character.rupees.DeductOverTime(40);
                                if (character.inventory.HasEmptySlot())
                                {
                                    character.inventory.CollectItem(new InventoryItem(ItemType.emptyBottle));
                                }
                                else
                                {
                                    character.PlaySound("throw");
                                    new FieldItem(character, dialogSourceActor!.pos, new InventoryItem(ItemType.emptyBottle), new FdPoint(0, 1), true);
                                }
                                ChangeState(new IdleState(character) { dashChargeHoldLock = true });
                            }
                            else
                            {
                                pages.Add("You don't have enough rupees!");
                                NextPage();
                            }
                        }
                        else if (isFortuneTeller)
                        {
                            isFortuneTeller = false;
                            if (character.rupees.value >= 100)
                            {
                                paidFortuneTeller = true;
                                pages.Add("Hocus pocus! I have marked\nthe spot on your map where\nthe Twilight will shrink.");
                                pages.Add("I hope you will be healthy.\nYeehah ha hah!");
                                character.rupees.DeductOverTime(100);
                                NextPage();
                            }
                            else
                            {
                                pages.Add("You don't have enough rupees!");
                                NextPage();
                            }
                        }
                        else
                        {
                            if (paidFortuneTeller)
                            {
                                character.health.FillMax();
                                character.magic.FillMax();
                            }
                            ChangeState(new IdleState(character) { dashChargeHoldLock = true });
                        }
                    }
                    else
                    {
                        NextPage();
                    }
                }
            }

            
            if (input.IsPressed(Control.Attack))
            {
                ChangeState(new IdleState(character));
            }
        }

        public void NextPage()
        {
            fullLine1 = pages[pageNum].Split('\n').SafeGet(0);
            fullLine2 = pages[pageNum].Split('\n').SafeGet(1);
            fullLine3 = pages[pageNum].Split('\n').SafeGet(2);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (dialogSourceComponent != null) dialogSourceComponent.inUse = false;
        }
    }
}
