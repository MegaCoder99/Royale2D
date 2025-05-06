namespace Royale2D
{
    public abstract class CharState
    {
        public Character character;
        public string baseSpriteName = "";
        public bool once;   // Generic flag for one-time actions, use it for any custom scenarios in derived classes as needed
        public int time;
        bool timeZeroOnce;
        public bool disableFlipX;
        public bool idleOnAnimEnd;
        public string enterSound = "";
        public bool visible = true;
        bool projOnce;
        public int magicCost;
        public bool canLedgeJump;
        public bool dashChargeHoldLock;
        public bool superArmor;
        public bool intangible;
        public bool noMoveSprites;
        public bool strafe;
        public bool canEnterAsBunny;
        public DamagerType? damagerType;
        public CharState? newState;    // REFACTOR remove, pass as OnExit parameter
        
        public IInputReader input => character.input;
        public Damager? damager => damagerType != null ? Damagers.damagers[damagerType.Value] : null;
        
        public CharState(Character character)
        {
            this.character = character;
        }

        public virtual void Update()
        {
            // We WANT time to be 0 on the first frame of existance, 0 index simplifies a lot of calculations and is consistent with other places
            if (timeZeroOnce) time++;
            else timeZeroOnce = true;

            if (character.spriteInstance.IsAnimOver() && idleOnAnimEnd)
            {
                character.ChangeState(new IdleState(character));
                return;
            }

            // This should only be used for simple projectiles that are "fire and forget" with no special logic
            // For complex ones like boomerang/hookshot, handle it yourself in the state
            FdPoint? poi = character.GetFirstPOIOrNull();
            if (poi != null && !projOnce)
            {
                Actor? proj = GetProj(poi.Value, character.dir);
                projOnce = true;
            }

            if (damager != null)
            {
                FdPoint? contactPos = GetDamagerContactPos();

                if (contactPos != null)
                {
                    TileInstance? tileInstance = character.layer.GetTileInstance(contactPos.Value.y.intVal / 8, contactPos.Value.x.intVal / 8);
                    if (tileInstance != null)
                    {
                        TileClumpInstance? tileClumpInstance = tileInstance.Value.GetTileClumpInstanceFromTag(TileClumpTags.Cuttable);
                        if (tileClumpInstance != null)
                        {
                            character.layer.TransformTileClumpWithAnim(tileClumpInstance.Value, character);

                            // IMPROVE should rupees and other collectables be center aligned?
                            FdPoint collectablePos = new FdPoint((tileClumpInstance.Value.j1 * 8) + 8, (tileClumpInstance.Value.i1 * 8) + 8);
                            character.section.CreateRandomPickup(collectablePos, true);
                        }
                    }
                }
            }
        }

        public virtual void RenderUpdate()
        {
        }

        public virtual void Render(Drawer drawer)
        {
        }

        public virtual void OnActorCollision(ActorCollision collision)
        {
        }

        public virtual void OnTileCollision(TileCollision collision)
        {
        }

        public virtual void OnEnter()
        {
            if (enterSound != "") character.PlaySound(enterSound);
            if (magicCost > 0) character.magic.DeductImmediate(magicCost);
            if (damagerType != null)
            {
                character.damagerComponent = character.ResetComponent(new DamagerComponent(character, damagerType.Value, character));
            }
        }

        public virtual void OnExit()
        {
            character.damagerComponent.disabled = true;
        }

        public virtual string GetChangeToError()
        {
            if (magicCost > 0 && character.magic.value < magicCost)
            {
                return "Not enough magic";
            }
            return "";
        }

        public virtual bool CanChangeFrom(CharState oldState)
        {
            return GetType() != oldState.GetType();
        }

        public virtual Actor? GetProj(FdPoint pos, Direction dir)
        {
            return null;
        }

        public virtual InputMoveData? MoveCode()
        {
            return null;
        }

        public virtual FdPoint GetDamagerContactPos()
        {
            if (this is SpinAttackState)
            {
                List<FdPoint> contactPosArr = [FdPoint.FromXY(16, -16), FdPoint.FromXY(16, 0), FdPoint.FromXY(16, 16), FdPoint.FromXY(0, 16), FdPoint.FromXY(-16, 16), FdPoint.FromXY(-16, 0), FdPoint.FromXY(-16, -16), FdPoint.FromXY(0, -16)];
                int off = 0;
                if (character.directionComponent.direction == Direction.Down) off = 0;
                if (character.directionComponent.direction == Direction.Up) off = 4;
                if (character.directionComponent.direction == Direction.Left) off = 2;
                if (character.directionComponent.direction == Direction.Right) off = 6;

                if (character.frameIndex == 2) return character.pos + contactPosArr[(0 + off) % 8];
                if (character.frameIndex == 3) return character.pos + contactPosArr[(1 + off) % 8];
                if (character.frameIndex == 4) return character.pos + contactPosArr[(2 + off) % 8];
                if (character.frameIndex == 5) return character.pos + contactPosArr[(3 + off) % 8];
                if (character.frameIndex == 6) return character.pos + contactPosArr[(4 + off) % 8];
                if (character.frameIndex == 7) return character.pos + contactPosArr[(5 + off) % 8];
                if (character.frameIndex == 8) return character.pos + contactPosArr[(6 + off) % 8];
                if (character.frameIndex == 9) return character.pos + contactPosArr[(7 + off) % 8];
            }

            return character.pos + character.directionComponent.ForwardFdVec(16);
        }

        public InputMoveData BasicWalkMoveCode(int speedModifier = 1)
        {
            if (Debug.debug && speedModifier == 1) speedModifier = Debug.charSpeedModifier;

            FdPoint moveAmount = FdPoint.Zero;

            Fd speedToUse = character.speed * speedModifier;
            Fd diagonalSpeedToUse = character.diagonalSpeed * speedModifier;
            if (character.colliderComponent.IsInTileWithTagAnyLayer(TileTag.Steps, TileTag.Stair, TileTag.StairUpper, TileTag.StairLower))
            {
                speedToUse /= 2;
                diagonalSpeedToUse /= 2;
            }

            if (input.IsHeld(Control.Left))
            {
                moveAmount.x = -speedToUse;
                if (!strafe) character.directionComponent.Change(Direction.Left);
            }
            else if (input.IsHeld(Control.Right))
            {
                moveAmount.x = speedToUse;
                if (!strafe) character.directionComponent.Change(Direction.Right);
            }

            if (input.IsHeld(Control.Down))
            {
                moveAmount.y = speedToUse;
                if (!strafe) character.directionComponent.Change(Direction.Down);
            }
            else if (input.IsHeld(Control.Up))
            {
                moveAmount.y = -speedToUse;
                if (!strafe) character.directionComponent.Change(Direction.Up);
            }

            if (moveAmount.x != 0 && moveAmount.y != 0)
            {
                moveAmount.x = diagonalSpeedToUse * moveAmount.x.sign;
                moveAmount.y = diagonalSpeedToUse * moveAmount.y.sign;
            }

            return new InputMoveData(moveAmount, moveAmount, speedToUse, character.diagonalSpeed * speedModifier);
        }

        public void ChangeState(CharState newState)
        {
            character.ChangeState(newState);
        }
    }

    // REFACTOR 2 things:
    // 1-consider renaming to InputMovementData
    // 2-everywhere that uses variables of this should also be renamed to above
    public struct InputMoveData
    {
        public FdPoint moveAmount;
        public FdPoint inputMoveAmount;
        public Fd nudgeSpeed;
        public Fd diagonalNudgeSpeed;

        public InputMoveData(FdPoint moveAmount, FdPoint inputMoveAmount, Fd nudgeSpeed, Fd diagonalNudgeSpeed)
        {
            this.moveAmount = moveAmount;
            this.inputMoveAmount = inputMoveAmount;
            this.nudgeSpeed = nudgeSpeed;
            this.diagonalNudgeSpeed = diagonalNudgeSpeed;
        }
    }
}
