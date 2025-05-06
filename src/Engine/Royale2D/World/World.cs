namespace Royale2D
{
    // Think of the "World" classes as instances of the "Map" model classes, which are immutable "blueprints" that are edited from map editor.
    // World => Map, WorldSection => MapSection, WorldSectionLayer => MapSectionLayer
    // The world ONLY exists when the match starts, not when waiting in lobby
    public class World
    {
        public List<WorldSection> sections = new List<WorldSection>();
        public int frameNum;   // Never change manually. Should only be modified by Update()
        public Storm storm;
        public BattleBusScreen battleBusScreen;
        public FluteScreen fluteScreen;
        public GameMode gameMode;
        public MasterSwordWoods? masterSwordWoods;
        public SoundManager soundManager = new SoundManager();
        public List<Character> characters = new List<Character>();

        // Notes on these two character variables: They are both always set (to avoid null exceptions). specCharacter is the same as mainCharacter when not spectating. When dead, mainCharacter
        // is the (dead) player's character, and specCharacter is whatever character is being spectated. Characters are not removed when dead, this vastly simplifies a lot of the code
        public Character mainCharacter;
        public Character specCharacter;

        public bool startedSpecAfterDeath;
        public HUD hud;
        public Camera camera;

        public WorldSection mainSection;
        
        public WorldHost worldHost;
        public Match match => worldHost.match;
        public Map map => worldHost.map;
        public EntranceSystem entranceSystem => worldHost.entranceSystem;
        public TextureManager textureManager => worldHost.textureManager;

        public World(WorldHost worldHost)
        {
            this.worldHost = worldHost;

            foreach (MapSection mapSection in map.sections)
            {
                var worldSection = new WorldSection(this, mapSection);
                sections.Add(worldSection);
                masterSwordWoods = masterSwordWoods ?? worldSection.actors.FirstOrDefault(a => a is MasterSwordWoods) as MasterSwordWoods;
            }

            mainSection = sections.First(s => s.name == "main");

            foreach (SyncedPlayerData player in match.players)
            {
                Character character = new Character(mainSection, player.id);
                characters.Add(character);
                if (player.id == match.mainPlayerId)
                {
                    mainCharacter = character;
                    ChangeSpecChar(character);
                }

                if (Debug.skipBattleBus)
                {
                    character.battleBusData.dropped = true;
                    int offX = player.id * 16;
                    int offY = 0;
                    character.pos = Debug.quickStartPos.ToFdPoint().AddXY(offX, offY);
                    character.ChangeState(new IdleState(character));
                }
            }

            storm = new Storm(this);
            battleBusScreen = new BattleBusScreen(this, storm);
            fluteScreen = new FluteScreen(this, storm);
            gameMode = new GameMode(this);

            camera = new Camera(specCharacter.pos.ToFloatPoint(), specCharacter.section);
            hud = new HUD(specCharacter, storm);

            if (Debug.debug && specCharacter != null)
            {
                Debug.AddDebugActors(mainSection, specCharacter.pos);
            }
        }

        // REFACTOR not needed?
        public void ChangeSpecChar(Character newSpecChar)
        {
            specCharacter = newSpecChar;
        }

        public void Update(int frameNum)
        {
            this.frameNum = frameNum;

            foreach (WorldSection section in sections)
            {
                section.Update();
            }

            soundManager.Update(camera);

            camera.enabled = specCharacter.battleBusData.dropped;
            camera.Update(specCharacter.GetCamPos(), specCharacter.section);
            hud.Update();

            storm.Update();
            battleBusScreen.Update();
            fluteScreen.Update();
            gameMode.Update();

            Debug.frameAdvance = false;
        }

        public void Render()
        {
            // PERF slow? Can optimize
            foreach (TileAnimation tileAnimation in map.tileAnimations.Values)
            {
                tileAnimation.Update();
            }

            if (!specCharacter.battleBusData.dropped)
            {
                battleBusScreen.Render(Game.hudDrawer);
            }
            else if (specCharacter.fluteScreenData.isInFluteScreen)
            {
                fluteScreen.Render(Game.hudDrawer);
            }
            else
            {
                camera.Render();
                hud.Render(Game.hudDrawer);
            }
        }

        // Nothing in this method should be updating game state that impacts match outcome, or else desyncs could happen.
        // For example, if menu option for rupee/arrow dropping, that should NOT be here, should somehow be invoked in Update()
        public void UpdateNonSyncedInputs()
        {
            Menu? defaultMenu = null;
            if (gameMode.ShowOverLeave() && match is OnlineMatch om && om.isHost)
            {
                defaultMenu = new CreateMatchMenu(null, true, false);
                if (Menu.current == null) Menu.current = defaultMenu;
            }

            if (Game.input.IsPressed(Control.Menu))
            {
                if (Menu.current == defaultMenu)
                {
                    Menu.ChangeMenu(new InGameMainMenu(this));
                }
                else
                {
                    Menu.ChangeMenu(defaultMenu);
                }
            }

            if (gameMode.IsOver()) return;

            if (Game.input.IsPressed(Control.Map))
            {
                hud.ToggleMap();
            }

            if (!mainCharacter.IsAlive())
            {
                if (!startedSpecAfterDeath)
                {
                    if (Game.input.IsPressed(Control.Action))
                    {
                        startedSpecAfterDeath = true;
                        SetNextSpecChar(1);
                    }
                }
                else
                {
                    if (Game.input.IsPressed(Control.Left))
                    {
                        SetNextSpecChar(-1);
                    }
                    else if (Game.input.IsPressed(Control.Right))
                    {
                        SetNextSpecChar(1);
                    }
                }
            }
        }

        public void SetNextSpecChar(int dir)
        {
            List<Character> canidateChars = characters.Where(c => c.IsAlive()).ToList();
            if (canidateChars.Count == 0) return;
            int specIndex = canidateChars.IndexOf(specCharacter) + dir;
            if (specIndex < 0) specIndex = canidateChars.Count - 1;
            if (specIndex >= canidateChars.Count) specIndex = 0;
            ChangeSpecChar(canidateChars[specIndex]);
        }

        public string GetStateHash()
        {
            string hash = "";
            for (int i = 0; i < characters.Count; i++)
            {
                hash += characters[i].pos.x.GetInternalVal() + "," + characters[i].pos.y.GetInternalVal() + "," + characters[i].sprite.name + "|";
            }

            return hash;
            //Helpers.ComputeMd5Hash();
        }
    }
}
