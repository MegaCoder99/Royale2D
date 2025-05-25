namespace Royale2D
{
    public class Character : Actor
    {
        public CharState charState;
        public int playerId;
        public Inventory inventory;
        public Stat health;
        public Stat magic;
        public Stat rupees;
        public Stat arrows;
        public int kills;
        public int place;
        public Boomerang? boomerang;
        public CaneBlock? caneBlock;
        public bool magicCapeOn;
        int footstepTime = 0;
        public List<string> drawboxTagsToHide = ["sword1", "sword2", "sword3", "sword4", "shield1", "shield2", "shield3", "hook"];
        public BattleBusData battleBusData = new BattleBusData();
        public FluteScreenData fluteScreenData = new FluteScreenData();
        public CharMusic charMusic;
        public InputMoveData lastMoveData;

        public readonly ColliderComponent colliderComponent;
        public readonly DirectionComponent directionComponent;
        public readonly ShadowComponent shadowComponent;
        public readonly WadeComponent wadeComponent;
        public readonly ShakeComponent shakeComponent;
        public readonly CameraShakeComponent cameraShakeComponent;
        public readonly BunnyComponent bunnyComponent;
        public readonly BurnableComponent burnableComponent;
        public readonly PoofComponent poofComponent;
        public readonly DamagableComponent damagableComponent;
        public readonly ParentComponent parentComponent;

        public VelComponent velComponent;
        public DamagerComponent damagerComponent;
        public ZComponent zComponent;

        public InventoryItem? selectedItem => inventory.selectedItem;
        public IInputReader input => section.world.worldHost.GetPlayerInputReader(playerId, section.world.frameNum);
        public SyncedPlayerData player => section.world.match.players.First(p => p.id == playerId);
        public BryanaRing? bryanaRing => parentComponent.children.FirstOrDefault(c => c is BryanaRing) as BryanaRing;
        public Direction dir => directionComponent.direction;
        public Fd speed => Fd.New(1, 50);
        public Fd diagonalSpeed => Fd.New(1, 10);

        public Character(WorldSection section, int playerId) : base(section, FdPoint.Zero)
        {
            this.playerId = playerId;

            var mainCollider = new Collider(IntRect.CreateWHCentered(0, 3, 16, 16), isMultiSprite: true, isWallCollider: true);
            var damagableCollider = new Collider(IntRect.CreateWHCentered(0, 0, 8, 10), isMultiSprite: true, isDamagable: true);
            colliderComponent = AddComponent(new ColliderComponent(this, [mainCollider, damagableCollider]));
            colliderComponent.ChangeMoveStrategy(new CharMoveStrategy(colliderComponent, this));
            directionComponent = AddComponent(new DirectionComponent(this, Direction.Down));
            shadowComponent = AddComponent(new ShadowComponent(this, ShadowType.Large, 8));
            wadeComponent = AddComponent(new WadeComponent(this));
            zComponent = AddComponent(new ZComponent(this));
            shakeComponent = AddComponent(new ShakeComponent(this), true);
            cameraShakeComponent = AddComponent(new CameraShakeComponent(this));
            poofComponent = AddComponent(new PoofComponent(this));
            burnableComponent = AddComponent(new BurnableComponent(this));
            bunnyComponent = AddComponent(new BunnyComponent(this, poofComponent, "char_bunny_idle"));
            damagableComponent = AddComponent(new DamagableComponent(this));
            parentComponent = AddComponent(new ParentComponent(this));
            velComponent = AddComponent(new VelComponent(this));
            damagerComponent = AddComponent(new DamagerComponent(this, DamagerType.sword1, null), true);

            inventory = new Inventory(this);
            health = new Stat(StatType.Health, this, startValue: 12, maxValue: 12, addCooldown: 10, addIncAmount: 4, maxValueCap: 40);
            magic = new Stat(StatType.Magic, this, startValue: 32, maxValue: 32, addCooldown: 4);
            rupees = new Stat(StatType.Rupee, this, startValue: Debug.main?.startRupees ?? 0, maxValue: 999, addCooldown: 2);
            arrows = new Stat(StatType.Arrow, this, startValue: Debug.main?.startArrows ?? 0, maxValue: 99, addCooldown: 0);

            charState = new UninitializedState(this);

            charMusic = new CharMusic(this, section.mapSection.defaultMusicName);
        }

        public override void PreUpdate()
        {
            base.PreUpdate();
            lastMoveData = new InputMoveData();
        }

        public override void Update()
        {
            charMusic.Update();
            if (IsSpecChar()) charMusic.UpdateSpecChar();

            if (charState is UninitializedState) return;

            base.Update();

            if (!IsAlive())
            {
                magicCapeOn = false;
                return;
            }

            if (magicCapeOn)
            {
                magic.DeductAtRate(15);
                if (magic.value <= 0)
                {
                    ToggleCape();
                }
            }
            if (bryanaRing != null)
            {
                magic.DeductAtRate(15);
                if (magic.value <= 0)
                {
                    parentComponent.RemoveChild(bryanaRing, true);
                }
            }

            health.Update();
            magic.Update();
            rupees.Update();
            arrows.Update();
            inventory.Update();

            InputMoveData? moveData = charState.MoveCode();
            if (moveData != null)
            {
                lastMoveData = moveData.Value;
                MoveCode(moveData.Value);
            }

            if (charState is IdleState && world.gameMode.IsWinner(this))
            {
                ChangeState(new WinState(this));
            }

            charState.Update();
        }

        public override void RenderUpdate()
        {
            if (charState is UninitializedState) return;
            base.RenderUpdate();
            charState.RenderUpdate();
        }

        public override void Render(Drawer drawer)
        {
            if (charState is UninitializedState) return;

            base.Render(drawer);
            charState.Render(drawer);

            inventory.Render(drawer);

            if (Debug.main?.showHitboxes == true)
            {
                var drawPos = GetCenterPos();
                drawer.DrawPixel(drawPos.x.intVal, drawPos.y.intVal, SFML.Graphics.Color.Red);
                
                //collisionsInFrame.ForEach(collision => collision.DrawDebugData(drawer));
            }

            // var firstPOI = GetFirstPOI();
            // drawer.DrawPixel(firstPOI.x, firstPOI.y, SFML.Graphics.Color.Red, ZIndex.UIGlobal);
        }

        public void ToggleCape()
        {
            magicCapeOn = !magicCapeOn;
            if (magicCapeOn) poofComponent.Poof();
            else poofComponent.Unpoof();
        }

        public override void OnPositionChanged(FdPoint oldPos, FdPoint newPos)
        {
            if (!colliderComponent.disabled)
            {
                if (FeatureGate.swim)
                {
                    if (colliderComponent.IsInTileWithTag(TileTag.Water) && charState is not LedgeJumpState)
                    {
                        if (CanSwim())
                        {
                            ChangeState(new SwimState(this));
                        }
                        else
                        {
                            ChangeState(new SwimJumpState(this, oldPos, false));
                        }
                    }
                    else
                    {
                        if (charState is SwimState)
                        {
                            ChangeState(new IdleState(this));
                        }
                    }
                }

                Entrance? entrance = world.entranceSystem.GetCollidingEntrance(this);
                if (entrance != null)
                {
                    if (!entrance.fall)
                    {
                        entrance.Enter(this);
                    }
                    else if (FeatureGate.pitEntrance)
                    {
                        if (colliderComponent.IsInTileWithTag(TileTag.Pit))
                        {
                            ChangeState(new FallState(this, entrance));
                        }
                    }
                }
            }
        }

        // IMPROVE make this read from collider
        public FdPoint GetCenterPos()
        {
            return pos.AddXY(0, 2);
        }

        public override List<string> GetDrawboxTagsToHide()
        {
            var drawboxTagsToHideCopy = new List<string>(drawboxTagsToHide);
            int swordLevel = inventory.SwordLevel();
            if (swordLevel > 0) drawboxTagsToHideCopy.Remove($"sword{swordLevel}");
            int shieldLevel = inventory.ShieldLevel();
            if (shieldLevel > 0) drawboxTagsToHideCopy.Remove($"shield{shieldLevel}");
            return drawboxTagsToHideCopy;
        }

        public override string GetOverrideTexture()
        {
            return player.skin;
        }

        public override ShaderInstance? GetShaderInstance()
        {
            if (charState is FrozenState)
            {
                return Assets.shaderInstances["frozen"];
            }
            return null;
        }

        public override void OnActorCollision(ActorCollision collision)
        {
            base.OnActorCollision(collision);
            charState.OnActorCollision(collision);

            if (charState is not ShovelState && charState is not DieState && collision.mine.collider.isWallCollider && collision.other.actor.GetComponent<CollectableComponent>() is CollectableComponent cc)
            {
                cc.Collect(this);
            }

            if (collision.other.actor is BigFairy bigFairy)
            {
                bigFairy.Heal(this);
            }

            if (charState is IdleState && collision.other.actor is FluteBird fluteBird && fluteBird.carriedChar == null)
            {
                fluteBird.PickupChar(this);
                ChangeState(new FluteBirdState(this, fluteBird));
            }
        }

        public override void OnTileCollision(TileCollision collision)
        {
            base.OnTileCollision(collision);
            charState.OnTileCollision(collision);

            TileInstance tileInstance = collision.other.tileInstance;
            TileClumpInstance? tileClumpInstance = tileInstance.GetTileClumpInstanceFromTag(TileClumpTags.Door);
            if (tileClumpInstance != null && IsFacingAndCloseToTileClump(tileClumpInstance.Value, isDoorOverrideThreshold: true))
            {
                PlaySound("door open");
                layer.TransformTileClump(tileClumpInstance.Value);
            }
        }

        public bool IsFacingAndCloseToTileClump(TileClumpInstance tileClumpInstance, bool isDoorOverrideThreshold = false)
        {
            FdPoint tileClumpCenterPos = tileClumpInstance.GetCenterPos();
            
            int distFromCenter;
            if (dir == Direction.Up || dir == Direction.Down)
            {
                distFromCenter = Math.Abs((tileClumpCenterPos.x - pos.x).intVal);
                int pixelWidth = tileClumpInstance.GetPixelWidth();
                int overrideThreshold = pixelWidth / 2;
                if (isDoorOverrideThreshold)
                {
                    overrideThreshold = pixelWidth / 4;
                }
                if (distFromCenter <= overrideThreshold)
                {
                    if (dir == Direction.Up) return tileClumpCenterPos.y < pos.y;
                    else return tileClumpCenterPos.y > pos.y;
                }
            }
            else
            {
                distFromCenter = Math.Abs((tileClumpCenterPos.y - pos.y).intVal);
                int pixelHeight = tileClumpInstance.GetPixelWidth();
                int overrideThreshold = pixelHeight / 2;
                if (isDoorOverrideThreshold)
                {
                    overrideThreshold = pixelHeight / 4;
                }
                if (distFromCenter <= overrideThreshold)
                {
                    if (dir == Direction.Left) return tileClumpCenterPos.x < pos.x;
                    else return tileClumpCenterPos.x > pos.x;
                }
            }

            return false;
        }

        public bool IsFacingAndCloseToPos(FdPoint centerPos, int pixelWidth, int pixelHeight)
        {
            int distFromCenter;
            if (dir == Direction.Up || dir == Direction.Down)
            {
                distFromCenter = Math.Abs((centerPos.x - pos.x).intVal);
                int overrideThreshold = pixelWidth / 2;
                
                if (distFromCenter <= overrideThreshold)
                {
                    if (dir == Direction.Up) return centerPos.y < pos.y;
                    else return centerPos.y > pos.y;
                }
            }
            else
            {
                distFromCenter = Math.Abs((centerPos.y - pos.y).intVal);
                int overrideThreshold = pixelHeight / 2;
                
                if (distFromCenter <= overrideThreshold)
                {
                    if (dir == Direction.Left) return centerPos.x < pos.x;
                    else return centerPos.x > pos.x;
                }
            }

            return false;
        }

        public void MoveCode(InputMoveData moveData)
        {
            FdPoint moveAmount = moveData.moveAmount;

            string footstepSound = "";
            if (moveAmount.IsNonZero() && charState is not SwimState)
            {
                if (colliderComponent.IsInTileWithTagAnyLayer(TileTag.Stair, TileTag.StairUpper, TileTag.StairLower))
                {
                    footstepSound = "walk stair";
                }
                else if (colliderComponent.IsInTileWithTag(TileTag.ShallowWater))
                {
                    footstepSound = "walk water";
                }
                else if (colliderComponent.IsInTileWithTag(TileTag.TallGrass))
                {
                    footstepSound = "walk grass";
                }

                if (footstepSound != "")
                {
                    footstepTime--;
                    if (footstepTime <= 0)
                    {
                        footstepTime = charState is DashState ? 7 : 15;
                        PlaySound(footstepSound);
                    }
                }
            }
            else if (footstepTime > 0)
            {
                footstepTime--;
            }

            if (Debug.main != null)
            {
                moveAmount *= Debug.main.charSpeedModifier;
            }

            if (!moveAmount.IsZero())
            {
                IncPos(moveData.moveAmount);
            }
        }

        public override bool IsVisible()
        {
            if (!charState.visible) return false;
            return base.IsVisible();
        }

        public void ChangeMapSection(Entrance startEntrance, Entrance endEntrance)
        {
            FdPoint charOffsetFromEntrance = FdPoint.Zero;
            if (startEntrance.useOffset)
            {
                charOffsetFromEntrance = startEntrance.pos.DirTo(pos);
                if (startEntrance.enterDir == Direction.Left || startEntrance.enterDir == Direction.Right) charOffsetFromEntrance.x = 0;
                if (startEntrance.enterDir == Direction.Up || startEntrance.enterDir == Direction.Down) charOffsetFromEntrance.y = 0;
            }

            WorldSection oldSection = section;

            oldSection.RemoveActor(this);
            section = world.sections.First(section => section.name == endEntrance.sectionName);

            IntPoint offset = Helpers.DirToVec(endEntrance.enterDir) * 18;
            layerIndex = endEntrance.layerIndex;
            pos = endEntrance.pos + offset.ToFdPoint() + charOffsetFromEntrance;
            section.AddActor(this);

            foreach (Actor child in parentComponent.children)
            {
                oldSection.RemoveActor(child);
                section.AddActor(child);
                child.pos = parentComponent.GetChildPosition(child);
            }

            if (!string.IsNullOrEmpty(startEntrance.overrideMusicName))
            {
                charMusic.baseMusicName = startEntrance.overrideMusicName;
            }
            else
            {
                charMusic.baseMusicName = section.mapSection.defaultMusicName;
            }
        }

        public void ChangeSectionDebug(string sectionName, FdPoint pos)
        {
            this.pos = pos;

            if (section.name == sectionName) return;
            section.RemoveActor(this);
            section = world.sections.First(section => section.name == sectionName);
            section.AddActor(this);
        }

        public void AddErrorText(string errorText)
        {
            if (this == world.mainCharacter)
            {
                world.hud.SetAlert1(errorText, 180);
                Game.PlaySound("error");
            }
        }

        public bool ChangeState(CharState newState)
        {
            CharState oldState = charState;
            string changeToError = newState.GetChangeToError();
            if (changeToError != "")
            {
                AddErrorText(changeToError);
                return false;
            }

            if (charState is DieState)
            {
                return false;
            }

            if (!newState.canEnterAsBunny && bunnyComponent.bunnyTime > 0)
            {
                return false;
            }

            if (oldState == null || newState.CanChangeFrom(oldState))
            {
                charState.newState = newState;
                charState.OnExit();
                charState = newState;
                directionComponent.SetDisableFlipX(charState.disableFlipX);
                baseSpriteName = charState.baseSpriteName;
                // In some cases like ledge jump transition to prev state, the state we change to already exists.
                // Detect with time and don't run OnEnter() again since that should only run once
                if (charState.time == 0)
                {
                    charState.OnEnter();
                }
                return true;
            }

            return false;
        }

        public override void OnDamageReceived(Damager damager, Character? attacker, ActorColliderInstance? attackerAci)
        {
            FdPoint recoilUnitVec = FdPoint.Zero;
            if (attackerAci != null)
            {
                recoilUnitVec = attackerAci.GetWorldShape().Center().ToFdPoint().DirToNormalized(pos);
            }

            if (Debug.main?.oneShotKill == true) damager.damage = 100;

            if (damager.damage > 0)
            {
                health.DeductImmediate(damager.damage);
                if (charState is FrozenState fs && damager.hitFrozen)
                {
                    fs.shatter = true;
                    recoilUnitVec = FdPoint.Zero;
                }
                if (health.value <= 0)
                {
                    health.value = 0;
                    if (inventory.HasItem(ItemType.bottledFairy))
                    {
                        ChangeState(new FairyDieState(this));
                    }
                    else
                    {
                        ChangeState(new DieState(this));
                        place = world.gameMode.GetPlace(this);
                        // This is not necessarily attacker, since that's null if the character dies from a non-character source like storm, so credit the actual last attacker
                        Character? killer = damagableComponent.lastAttacker;
                        world.hud.AddKillFeedEntry(new KillFeedEntry(damager.damagerType, this, killer));
                        if (killer != null)
                        {
                            killer.kills++;
                        }
                    }
                }
                else if (damager.flinch)
                {
                    PlaySound("char hurt");
                    damagableComponent.hurtInvulnTime = 60;
                    if (!charState.superArmor)
                    {
                        ChangeState(new HurtState(this, recoilUnitVec));
                    }
                }
                else
                {
                    PlaySound("char hurt");
                }
            }

            if (damager.burnTime > 0)
            {
                burnableComponent.Burn(damager.burnTime, damager.damagerType, attacker);
            }
            else if (damager.bunnyTime > 0)
            {
                bunnyComponent.Bunnify(damager.bunnyTime, attacker);
            }
            else if (damager.stun)
            {
                ChangeState(new StunState(this, recoilUnitVec));
            }
            else if (damager.freeze)
            {
                ChangeState(new FrozenState(this));
            }
        }

        public void Collect(CollectableComponent cc)
        {
            if (cc.healthGain == -1) health.FillMax();
            else if (cc.healthGain > 0)
            {
                if (cc.actor is Fairy) PlaySound("fairy");
                else PlaySound("heart");
                health.AddOverTime(cc.healthGain);
            }

            if (cc.magicGain == -1) magic.FillMax();
            else if (cc.magicGain > 0) magic.AddOverTime(cc.magicGain);

            if (cc.rupeeGain == -1) rupees.FillMax();
            else if (cc.rupeeGain > 0) rupees.AddOverTime(cc.rupeeGain);

            if (cc.arrowGain > 0)
            {
                PlaySound("heart");
                arrows.AddImmediate(cc.arrowGain);
            }
        }

        public bool CanSwim()
        {
            return inventory.HasItem(ItemType.flippers) && !bunnyComponent.isBunny;
        }

        public FdPoint GetOverworldPos()
        {
            return section.GetMainSectionPos(pos);
        }

        public bool IsAlive()
        {
            if (charState is DieState ds && ds.state == 1) return false;
            return true;
        }

        // A character's inputs won't matter in match anymore if this returns true.
        public bool NoMoreInputs()
        {
            // TODO check spectate flag too
            if (charState is DieState ds || charState is WinState ws)
            {
                // 60 frames is more than double max delay window, giving enough buffer time
                // REFACTOR reference constant
                return charState.time > 60;
            }
            return false;
        }

        public bool IsMainChar()
        {
            return this == world.mainCharacter;
        }

        public bool IsSpecChar()
        {
            return this == world.specCharacter;
        }

        public bool IsWinner()
        {
            return world.gameMode.IsWinner(this);
        }

        public void PlaySoundIfMain(string soundName)
        {
            if (IsMainChar()) Game.PlaySound(soundName);
        }

        public void EnterFluteScreen()
        {
            fluteScreenData.isInFluteScreen = true;
            fluteScreenData.dropI = pos.y.intVal / 8;
            fluteScreenData.dropJ = pos.x.intVal / 8;
        }

        public Point GetCamPos()
        {
            if (charState is LedgeJumpState)
            {
                return pos.AddXY(0, -zComponent.z).ToFloatPoint();
            }
            return pos.ToFloatPoint();
        }
    }
}
