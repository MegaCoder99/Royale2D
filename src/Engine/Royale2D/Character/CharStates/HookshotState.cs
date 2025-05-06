namespace Royale2D
{
    public class HookshotState : CharState
    {
        public HookshotHook? hook;

        public HookshotState(Character character) : base(character)
        {
            baseSpriteName = "char_hookshot";
        }

        public override void Update()
        {
            base.Update();

            FdPoint? poi = character.GetFirstPOIOrNull();
            if (poi != null && !once)
            {
                hook = new HookshotHook(character, poi.Value);
                once = true;
            }

            if (hook?.reverseDir == false && input.IsPressed(Control.Item) && character.selectedItem?.itemType == ItemType.hookshot)
            {
                hook.DoReverseDir();
            }

            if (hook != null)
            {
                character.PlaySound("hookshot", true);
            }

            if (hook != null && hook.hooked)
            {
                hook.MoveOwner();
            }

            if (hook != null && hook.returnedToUser)
            {
                character.ChangeState(new IdleState(character));
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            character.colliderComponent.disableWallCollider = true;
        }

        public override void OnExit()
        {
            base.OnExit();
            hook?.OnRemove();
            hook?.DestroySelf();
            character.colliderComponent.disableWallCollider = false;
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
