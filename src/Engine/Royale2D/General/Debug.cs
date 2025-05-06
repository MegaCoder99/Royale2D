using SFML.Window;
using Shared;
using System.Diagnostics;

namespace Royale2D
{
    public enum QuickStartType
    {
        None,
        Offline,
        Host,
        Client,
        HostIfDebugging,    // If using this, press Ctrl+F5 initially to start as client, then F5 to start as host with debugging
        ClientIfDebugging   // If using this, press Ctrl+F5 initially to start as host, then F5 to start as client with debugging
    }

    // Contains useful code only for local testing.
    public static class Debug
    {
#if DEBUG
        public static bool debug = true;
#else
        public static bool debug = false;
#endif

        //public static string customAssetsPath = "C:/users/username/desktop/assets_custom";
        public static string customAssetsPath = "";
        public static bool customAssets => customAssetsPath.IsSet();
        public static bool menuDev = false;
        public static bool unlimitedFPS = false;
        public static bool showHitboxes = false;
        public static bool breakpoint = false;
        public static GridCoords? hitTileCoords = null;
        public static bool showActorGrid = false;
        public static int charSpeedModifier = 1;

        public static QuickStartType quickStartType = QuickStartType.Offline;
        public static bool dontLoadSkins = true;
        public static bool disableMusic = true;

        public static string quickStartMapName => Assets.maps.Keys.First();
        public static IntPoint quickStartPos = new IntPoint(300, 300);
        //public static IntPoint quickStartPos = new IntPoint(2237, 2865);   // Ledge testing

        //public static IntPoint quickStartPos = new IntPoint(2797, 2574);   // Wide area testing
        //public static IntPoint quickStartPos = new IntPoint(2047, 1893);   // Entrance testing
        //public static IntPoint quickStartPos = new IntPoint(2056, 3478);   // Wade testing
        //public static IntPoint quickStartPos = new IntPoint(606, 199);     // Mountain testing

        public static int cpuCount = 0;
        public static bool cpuAttack = false;
        public static bool skipBattleBus => !FeatureGate.battlebus || true;
        public static int battleBusMaxTime = 30;
        public static bool debugStorm = false;

        public static string quickStartMatchName = "Test Match";
        public static string quickStartMapSection = "main";

        public static bool oneShotKill = true;
        public static int startRupees = 0;
        public static int startArrows = 90;

        public static int? sword = 1;
        public static int? shield = 1;

        public static bool hasEverything = false;

        public static int delayFrames = 3;
        public static bool frameAdvance;
        public static int maxDelayFrames = 30;
        public static bool paused;
        public static float simulatedDuplicates = 0;
        public static float simulatedLatency = 0;
        public static float simulatedPacketLoss = 0;

        public static List<InventoryItem?> GetDebugItems()
        {
            return [null, null, null, null, null];
            /*
            return [
                new InventoryItem(ItemType.caneOfSomaria),
                new InventoryItem(ItemType.bow),
                new InventoryItem(ItemType.boomerang),
                new InventoryItem(ItemType.hammer),
                new InventoryItem(ItemType.bombs, 99),
            ];
            */
        }

        static int debugPresetIndex = -1;
        public static List<List<InventoryItem?>> GetDebugItemsPresets()
        {
            return [
                [
                    new InventoryItem(ItemType.bow),
                    new InventoryItem(ItemType.bombs, 99),
                    new InventoryItem(ItemType.boomerang),
                    new InventoryItem(ItemType.hookshot),
                    new InventoryItem(ItemType.hammer)
                ],
                [
                    new InventoryItem(ItemType.firerod),
                    new InventoryItem(ItemType.icerod),
                    new InventoryItem(ItemType.bombos),
                    new InventoryItem(ItemType.ether),
                    new InventoryItem(ItemType.quake)
                ],
                [
                    new InventoryItem(ItemType.lamp),
                    new InventoryItem(ItemType.caneOfBryana),
                    new InventoryItem(ItemType.caneOfSomaria),
                    new InventoryItem(ItemType.powder),
                    new InventoryItem(ItemType.cape)
                ],
                [
                    new InventoryItem(ItemType.net),
                    new InventoryItem(ItemType.emptyBottle),
                    new InventoryItem(ItemType.bottledFairy),
                    new InventoryItem(ItemType.flute),
                    new InventoryItem(ItemType.shovel)
                ],
                [
                    new InventoryItem(ItemType.magicBoomerang),
                    new InventoryItem(ItemType.silverBow),
                    new InventoryItem(ItemType.moonPearl),
                    //new InventoryItem(ItemType.bigBomb),
                ]
            ];
        }

        public static void AddDebugActors(WorldSection section, FdPoint charPos)
        {
            //new Fairy(section, charPos.AddXY(32, 32), true);
            //new Bee(section, charPos.AddXY(48, 32), null);
            //new Cucco(section, charPos.AddXY(8, 8));
            //Collectables.CreateHeart(section, charPos.AddXY(16, 16), false);
            //Collectables.CreateRedRupee(section, charPos.AddXY(32, 16), false);
            //Collectables.CreateLargeMagic(section, charPos.AddXY(32, 32), false);
            //Collectables.CreateSmallMagic(section, charPos.AddXY(32, 32), false);
        }

        public static void Start()
        {
            if (quickStartType == QuickStartType.Offline || !FeatureGate.menu)
            {
                CreateAndStartOfflineMatch();
            }
            else if (quickStartType == QuickStartType.Host)
            {
                QuickCreateServer();
            }
            else if (quickStartType == QuickStartType.Client)
            {
                QuickJoinServer();
            }
            else if (quickStartType == QuickStartType.HostIfDebugging)
            {
                if (Debugger.IsAttached)
                {
                    QuickCreateServer();
                }
                else
                {
                    Menu.ChangeMenu(new QuickJoinMenu(QuickJoinServer));
                }
            }
            else if (quickStartType == QuickStartType.ClientIfDebugging)
            {
                if (Debugger.IsAttached)
                {
                    // In this flow we can just join automatically since host was already expected to have created match with Ctrl+F5
                    QuickJoinServer();
                }
                else
                {
                    QuickCreateServer();
                }
            }
        }

        static void QuickCreateServer()
        {
            Helpers.ThrowWhenDebugging(() =>
            {
                var onlineMatch = OnlineMatch.Create(new MatchSettings(quickStartMapName, quickStartMatchName), Options.main.relayServerIp);
                Match.current = onlineMatch;
                Menu.ChangeMenu(new MatchLobbyMenu(Menu.current, onlineMatch));
            }, "Failed to quick create server");
        }

        static void QuickJoinServer()
        {
            Helpers.ThrowWhenDebugging(() =>
            {
                var onlineMatch = OnlineMatch.Join(quickStartMatchName, Options.main.relayServerIp);
                Match.current = onlineMatch;
                Menu.ChangeMenu(new MatchLobbyMenu(Menu.current, onlineMatch));
            }, "Failed to quick join server");
        }

        static bool started;
        public static void Update()
        {
            if (!started)
            {
                Start();
                started = true;
            }

            if (!customAssets)
            {
                return;
            }

            // Check if sfml key pressed
            if (Game.input.IsKeyPressed(Keyboard.Key.F5))
            {
                // Hot reload
                Assets.LoadGuis();
                if (Match.current == null) Menu.ChangeMenu(new MainMenu());
            }

            if (Game.input.IsKeyPressed(Keyboard.Key.R))
            {
                // Hot reload
                Assets.LoadGuis();
                if (Match.current == null) Menu.ChangeMenu(new MainMenu());
            }

            World? world = Match.current?.world;
            if (world != null)
            {
                Character chr = world.mainCharacter;

                if (Game.input.IsKeyPressed(Keyboard.Key.F1))
                {
                    showHitboxes = !showHitboxes;
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.F2))
                {
                    showActorGrid = !showActorGrid;
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.F3))
                {
                    chr.colliderComponent.disabled = !chr.colliderComponent.disabled;
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.F4))
                {
                    /*
                    if (chr != null)
                    {
                        var inputDir = chr.input.GetInputNormFdVec();
                        if (inputDir.IsZero())
                        {
                            chr.damagableComponent.ApplyDamage(new Damager("debug", "0.5"), null, null);
                        }
                        else
                        {
                            chr.ChangeState(new HurtState(chr, inputDir));
                        }
                    }
                    */
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.F5))
                {
                    File.WriteAllText("DebugSavedPos.txt", chr.pos.x.intVal + "," + chr.pos.y.intVal + "," + chr.section.name + "," + chr.dir.ToString());
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.F7))
                {
                    if (File.Exists("DebugSavedPos.txt"))
                    {
                        string text = File.ReadAllText("DebugSavedPos.txt");
                        FdPoint pos = new FdPoint(int.Parse(text.Split(',')[0]), int.Parse(text.Split(',')[1]));
                        string sectionName = text.Split(',')[2];
                        string dirStr = text.Split(',')[3];
                        if (dirStr == "Up") chr.directionComponent.Change(Direction.Up);
                        if (dirStr == "Down") chr.directionComponent.Change(Direction.Down);
                        if (dirStr == "Left") chr.directionComponent.Change(Direction.Left);
                        if (dirStr == "Right") chr.directionComponent.Change(Direction.Right);
                        chr.ChangeSectionDebug(sectionName, pos);
                    }
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.F10))
                {
                    /*
                    Tips.debugCycle++;
                    if (Tips.debugCycle >= Tips.tips.Count)
                    {
                        Tips.debugCycle = 0;
                    }
                    */
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.F11))
                {
                    //MusicManager.SetNearLoopEnd();
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.F6))
                {
                    Debug.cpuAttack = !Debug.cpuAttack;
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.E))
                {
                    Helpers.IncCycle(ref debugPresetIndex, 5);
                    var items = GetDebugItemsPresets()[debugPresetIndex];
                    chr.inventory.items = items;
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.Q))
                {
                    Helpers.DecCycle(ref debugPresetIndex, 5);
                    var items = GetDebugItemsPresets()[debugPresetIndex];
                    chr.inventory.items = items;
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.W))
                {
                    hasEverything = !hasEverything;
                    chr.health.value = chr.health.maxValue;
                    chr.magic.value = chr.magic.maxValue;
                    chr.arrows.value = 99;
                    chr.rupees.value = 999;
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.H))
                {
                    chr.health.value = chr.health.maxValue;
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.B))
                {
                    /*
                    if (chr.bunnyComponent.bunnyTime == 0)
                    {
                        chr.bunnyComponent.Bunnify(60 * 100, Damagers.stormBunnifier, null);
                    }
                    else
                    {
                        chr.bunnyComponent.bunnyTime = 1;
                    }
                    */
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.K))
                {
                    //chr.damagableComponent.ApplyDamage(new Damager("debug", "1000"), null, null);
                }
                else if (Game.input.IsKeyPressed(Keyboard.Key.O))
                {
                    oneShotKill = !oneShotKill;
                }

                if (Game.input.IsKeyHeld(Keyboard.Key.LShift))
                {
                    charSpeedModifier = 5;
                }
                else
                {
                    charSpeedModifier = 1;
                }

                if (world.storm != null)
                {
                    if (Game.input.IsKeyHeld(Keyboard.Key.Tilde))
                    {
                        world.storm.isFastStormTimer = true;
                    }
                    else
                    {
                        world.storm.isFastStormTimer = false;
                    }
                }
            }
        }

        public static void CreateAndStartOfflineMatch()
        {
            Match.current = OfflineMatch.Create(new MatchSettings(quickStartMapName, ""));
            Match.current.Start();
        }

        // TODO once HUD work has started, make showFPS "official"
        public static float lastFps;
        public static List<float> fpsQueue = new List<float>();
        public static float timer;
        public static string debugString1 = "";
        public static string debugString2 = "";
        public static string debugString3 = "";
        public static bool waitingForPlayers;

        public static void RenderToWorld(Drawer drawer, ColliderGrid colliderGrid)
        {
        }

        public static void RenderToScreen(Drawer drawer)
        {
            if (!debug) return;

            fpsQueue.Add(Game.fps);
            timer += Game.spf;
            if (timer > 0.25f)
            {
                lastFps = fpsQueue.Average();
                fpsQueue.Clear();
                timer = 0;
            }

            //drawer.DrawText(lastFps.ToString("0.0"), 0, 0, bitmapFont: BitmapFont.small, batch: false);

            string debugStatus = "";
            if (Debugger.IsAttached) debugStatus = "DEBUGGING";
            if ((Match.current as OnlineMatch)?.isHost == true) debugStatus = debugStatus.DelimAppend(",HOST");
            if ((Match.current as OnlineMatch)?.isClient == true) debugStatus = debugStatus.DelimAppend(",CLIENT");

            if (debugStatus != "")
            {
                //drawer.DrawText(debugStatus, 0, 10, fontType: FontType.Small, batch: false);
            }

            //debugString1 = (Match.current as OnlineMatch)?.world?.inputSyncer?.inputSyncDatas?.FirstOrDefault()?.latestFrameOfTheirsIAcked.ToString() ?? "";
            //debugString2 = (Match.current as OnlineMatch)?.world?.inputSyncer?.inputSyncDatas?.FirstOrDefault()?.latestFrameOfMineTheyAcked.ToString() ?? "";
            //debugString3 = (Match.current as OnlineMatch)?.world?.frameNum.ToString() ?? "";

            /*
            ushort? myPing = (Match.current as OnlineMatch)?.matchUdpClient?.client?.ServerConnection?.GetUshortPing(false);
            debugString1 = "My ping: " + (myPing != null ? myPing.ToString() : "?");

            ushort? otherPlayerPing = (Match.current as OnlineMatch)?.otherPlayer?.ping;
            debugString2 = "Other ping: " + (otherPlayerPing != null ? otherPlayerPing.ToString() : "?");

            int? delayFrames = (Match.current as OnlineMatch)?.world?.inputSyncer?.inputDelayFrames;
            debugString3 = "Delay frames: " + (delayFrames != null ? delayFrames.ToString() : "?");
            */

            debugString1 = Match.current?.world?.mainCharacter?.pos.ToString() ?? "";

            if (Match.current != null && waitingForPlayers)
            {
                drawer.DrawText("WAITING FOR PLAYERS", 0, 60, fontType: FontType.Small, batch: false);
            }

            if (debugString1 != "") drawer.DrawText(debugString1, 0, 20, fontType: FontType.Small, batch: false);
            if (debugString2 != "") drawer.DrawText(debugString2, 0, 30, fontType: FontType.Small, batch: false);
            if (debugString3 != "") drawer.DrawText(debugString3, 0, 40, fontType: FontType.Small, batch: false);
        }
    }
}
