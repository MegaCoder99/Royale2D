namespace Royale2D
{
    public class BonkState : CharState
    {
        FdPoint dashUnitVelOnHit;

        public BonkState(Character character, FdPoint dashUnitVelOnHit) : base(character)
        {
            baseSpriteName = "char_hurt";
            this.dashUnitVelOnHit = dashUnitVelOnHit;
        }

        public override void Update()
        {
            base.Update();
            if (character.zComponent.HasLanded())
            {
                character.ChangeState(new IdleState(character));
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            character.velComponent = character.ResetComponent(new VelComponent(character));
        }

        public override void OnEnter()
        {
            base.OnEnter();

            character.PlaySound("ram");

            character.zComponent = character.ResetComponent(new ZComponent(character, zVel: Fd.New(1, 50), useGravity: true, playLandSound: true));
            character.velComponent = character.ResetComponent(new VelComponent(character, dashUnitVelOnHit * -1));

            float shakeX = dashUnitVelOnHit.x.abs > 0 ? 1 : 0;
            float shakeY = dashUnitVelOnHit.y.abs > 0 ? 1 : 0;
            character.cameraShakeComponent.Shake(shakeX, shakeY);
        }
    }
}
