namespace Royale2D
{
    public class IdleState : CharState
    {
        public IdleState(Character character) : base(character)
        {
            baseSpriteName = "char_idle";
            canLedgeJump = true;
            canEnterAsBunny = true;
        }

        public override void Update()
        {
            base.Update();

            PotentialAction? potentialAction = GetPotentialAction();

            string paText = potentialAction?.GetDisplayTextExternal() ?? "";
            if (paText != "")
            {
                // ANOW X hard coded
                character.world.hud.SetAlert1("Press X to " + paText, 2);
            }

            if (FeatureGate.attack && input.IsPressed(Control.Attack) || (Debug.cpuAttack && character.player.isBot && Game.frameCount % 60 == 0))
            {
                character.ChangeState(new SwordSwingState(character));
            }
            else if (input.IsPressed(Control.Action))
            {
                if (potentialAction != null)
                {
                    dashChargeHoldLock = true;
                    potentialAction.ExecuteExternal();
                }
            }
            else if (input.IsHeld(Control.Action) && !dashChargeHoldLock && character.inventory.HasItem(ItemType.pegasusBoots))
            {
                character.ChangeState(new DashChargeState(character));
            }
            else if (input.IsPressed(Control.Item))
            {
                character.inventory.UseSelectedItem();
            }

            if (input.IsPressed(Control.Toss))
            {
                character.inventory.DropSelectedItem(character.inventory.selectedItemIndex);
            }

            if (!input.IsHeld(Control.Action))
            {
                dashChargeHoldLock = false;
            }
        }

        public PotentialAction? GetPotentialAction()
        {
            List<PotentialAction> potentialActions = [];
            List<ActorCollision> actorCollisions = character.colliderComponent.GetMainActorCollisions(character.directionComponent.ForwardFdVec(2));
            List<TileCollision> tileCollisions = character.colliderComponent.GetMainTileCollisions(character.directionComponent.ForwardFdVec(2));

            foreach (ActorCollision actorCollision in actorCollisions)
            {
                potentialActions.Add(new CollectFieldItemPA(character, actorCollision));
                potentialActions.Add(new LiftActorPA(character, actorCollision));
                potentialActions.Add(new BuyItemPA(character, actorCollision));
                potentialActions.Add(new StartDialogPA(character, actorCollision));
                potentialActions.Add(new PullMasterSwordPA(character, actorCollision));
            }

            foreach (TileCollision tileCollision in tileCollisions)
            {
                potentialActions.Add(new LiftTileClumpPA(character, tileCollision));
                potentialActions.Add(new OpenChestPA(character, tileCollision));
                potentialActions.Add(new StartDialogPA(character, tileCollision));
            }

            return potentialActions
                .Where(pa => pa.GetPriorityExternal() != null)
                .OrderBy(pa => pa.GetPriorityExternal())
                .FirstOrDefault();
        }

        public override InputMoveData? MoveCode()
        {
            return BasicWalkMoveCode();
        }
    }
}
