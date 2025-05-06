using Shared;

namespace Royale2D
{
    public class HUD
    {
        public Character character;
        public List<KillFeedEntry> killFeed = [];
        public string alert1 = "";
        public int alert1Time;
        public string alert2 = "";
        public int alert2Time;
        public Minimap minimap;
        public Storm storm;

        public static bool hideMap;       // Not synced so making static and set to default value in constructor

        public World world => character.world;
        public Gui gui => Assets.guis["hud"];

        // REFACTOR probably can go in OnlineMatch
        public List<ChatFeedEntry> chatFeed = new List<ChatFeedEntry>();

        public HUD(Character character, Storm storm)
        {
            this.character = character;
            this.storm = storm;
            minimap = new Minimap(
                mapWidth: world.map.mainSection.pixelWidth,
                mapHeight: world.map.mainSection.pixelHeight,
                minimapWidth: 75,
                minimapHeight: 75,
                startX: world.map.minimapSmallSpriteData.topLeftX,
                startY: world.map.minimapSmallSpriteData.topLeftY,
                endX: world.map.minimapSmallSpriteData.bottomRightX,
                endY: world.map.minimapSmallSpriteData.bottomRightY,
                storm,
                "minimap_storm");
            hideMap = true;
        }

        public void Update()
        {
            Helpers.DecClampZero(ref alert1Time);
            Helpers.DecClampZero(ref alert2Time);

            for (int i = killFeed.Count - 1; i >= 0; i--)
            {
                KillFeedEntry entry = killFeed[i];
                entry.time++;
                if (entry.time > 600)
                {
                    killFeed.RemoveAt(i);
                }
            }

            for (int i = chatFeed.Count - 1; i >= 0; i--)
            {
                ChatFeedEntry entry = chatFeed[i];
                entry.time++;
                if (entry.time > 600)
                {
                    chatFeed.RemoveAt(i);
                }
            }
        }

        public void Render(Drawer drawer)
        {
            if (!FeatureGate.hud) return;

            gui.SetNodeText("clock-text", storm.GetStormDisplayTime());
            gui.SetNodeText("kills-text", character.kills.ToString("00"));
            gui.SetNodeText("alive-text", world.gameMode.GetCharactersLeft().ToString("00"));
            gui.SetNodeText("rupees-text", character.rupees.value.ToString("000"));
            gui.SetNodeText("arrows-text", character.arrows.value.ToString("00"));

            if (character.IsMainChar() && character.charState is DialogState ds)
            {
                gui.GetNodeById("text-box").hidden = false;
                gui.GetNodeById("text-box-image").hidden = false;
                gui.SetNodeText("line1", ds.line1);
                gui.SetNodeText("line2", ds.line2);
                gui.SetNodeText("line3", ds.line3);
            }
            else
            {
                gui.GetNodeById("text-box").hidden = true;
                gui.GetNodeById("text-box-image").hidden = true;
            }

            if (world.gameMode.ShowOverLeave())
            {
                if (world.match is OnlineMatch om)
                {
                    if (!om.isHost)
                    {
                        gui.SetNodeText("match-over-text", "Waiting for host...");
                    }
                }
                else
                {
                    gui.SetNodeText("match-over-text", "Press Z to leave match");
                }
            }

            if (world.gameMode.IsOver())
            {
                if (world.gameMode.IsWinner(world.mainCharacter))
                {
                    // Winner, show nothing
                }
                else
                {
                    Character? winner = world.gameMode.winners.FirstOrDefault();
                    gui.SetNodeText("alert1", $"{winner?.player.name} wins!");
                    if (!world.startedSpecAfterDeath)
                    {
                        gui.SetNodeText("alert2", $"You placed {Helpers.GetPlaceText(character.place)}");
                    }
                }
            }
            else
            {
                if (character.IsAlive() && character == world.mainCharacter)
                {
                    gui.SetNodeText("alert1", alert1Time > 0 ? alert1 : "");
                    gui.SetNodeText("alert2", alert2Time > 0 ? alert2 : "");
                }
                else if (!character.IsAlive() && character == world.mainCharacter)
                {
                    gui.SetNodeText("alert1", $"You placed {Helpers.GetPlaceText(character.place)}");
                    gui.SetNodeText("alert2", "Press X to spectate");
                }
                else if (character != world.mainCharacter)
                {
                    gui.SetNodeText("alert1", "Now spectating " + character.player.name);
                    gui.SetNodeText("alert2", "Left/right: change player");
                }
            }

            if (storm.isStormWait) gui.SetNodeSprite("clock", "hud_clock");
            else gui.SetNodeSprite("clock", "hud_storm");

            gui.Render(drawer);

            // HEARTS SECTION //

            List<Node> hearts = gui.GetNodesById("heart");

            for (int i = 0; i < hearts.Count; i++)
            {
                ImageNode heart = hearts[i] as ImageNode;
                Point heartPos = heart.GetPos();

                int heartNum = i + 1;   // One-indexed for simplicity
                if (heartNum > character.health.maxValue / 4) break;

                // Frame 0 = full, 1 = half, 2 = empty
                int frame = 0;
                bool isHealthWholeNumber = character.health.value % 4 == 0;
                if (heartNum > MyMath.Ceil(character.health.value / 4f))
                {
                    frame = 2;
                }
                else if (heartNum == MyMath.Ceil(character.health.value / 4f) && !isHealthWholeNumber)
                {
                    frame = 1;
                }

                Assets.GetSprite(heart.spriteName).Render(drawer, heartPos, frame);
            }

            // MAGIC SECTION //

            ImageNode magicNode = gui.GetNodeById("magic-meter") as ImageNode;
            Point magicPos = magicNode.GetPos().AddXY(3, 3);
            float magicW = 96 * ((float)character.magic.value / character.magic.maxValue);
            drawer.DrawRectWH(magicPos.x, magicPos.y, magicW, 6, true, new SFML.Graphics.Color(33, 198, 41), thickness: 0);

            // ITEMS SECTION //

            for (int i = 0; i < character.inventory.items.Count; i++)
            {
                InventoryItem? item = character.inventory.items[i];
                ImageNode itemNode = gui.GetNodeById("item" + (i + 1)) as ImageNode;
                if (item != null)
                {
                    if ((item.itemType == ItemType.bow || item.itemType == ItemType.silverBow) && character.arrows.value == 0)
                    {
                        Assets.GetSprite("hud_item").Render(drawer, itemNode.GetPos().AddXY(11, 11), 15);
                    }
                    else
                    {
                        Assets.GetSprite("hud_item").Render(drawer, itemNode.GetPos().AddXY(11, 11), item.item.spriteIndex);
                    }
                }
                if (character.inventory.selectedItemIndex == i)
                {
                    Assets.GetSprite("hud_item_box_selected").Render(drawer, itemNode.GetPos().AddXY(11, 11), 0);
                }
                if (item != null && item.item.usesQuantity)
                {
                    drawer.DrawText(item.quantity.ToString(), itemNode.GetPos().x + 18, itemNode.GetPos().y + 12, AlignX.Right, fontType: FontType.Small, letterSpacing: -1);
                }
            }

            // MAP SECTION //

            Div mapHiddenDiv = gui.GetNodeById("map-hidden") as Div;
            Div mapShownDiv = gui.GetNodeById("map-shown") as Div;
            if (hideMap)
            {
                mapHiddenDiv.hidden = false;
                mapShownDiv.hidden = true;
            }
            else
            {
                mapHiddenDiv.hidden = true;
                mapShownDiv.hidden = false;

                ImageNode mapNode = gui.GetNodeById("map") as ImageNode;
                Point mapPos = mapNode.GetPos();

                world.map.minimapSmallSpriteData.sprite.Render(drawer, new Point(mapPos.x - 5, mapPos.y - 5), 0);

                minimap.Render(drawer, (int)mapPos.x - 80, (int)mapPos.y - 80, () =>
                {
                    minimap.DrawSpriteOnMinimap("map_player", character.GetOverworldPos().ToFloatPoint());
                });
            }

            // CHAT/KILLFEED SECTION //

            List<Div> chatFeedDivs = [
                gui.GetNodeById("chatfeed5") as Div,
                gui.GetNodeById("chatfeed4") as Div,
                gui.GetNodeById("chatfeed3") as Div,
                gui.GetNodeById("chatfeed2") as Div,
                gui.GetNodeById("chatfeed1") as Div
            ];
            for (int i = 0; i < chatFeed.Count; i++)
            {
                ChatFeedEntry entry = chatFeed[i];
                Point pos = chatFeedDivs[i].GetPos();
                drawer.DrawText(entry.text, pos.x, pos.y, AlignX.Left, fontType: FontType.Small, letterSpacing: -1);
            }

            List<Div> killFeedDivs = [
                gui.GetNodeById("killfeed5") as Div,
                gui.GetNodeById("killfeed4") as Div,
                gui.GetNodeById("killfeed3") as Div,
                gui.GetNodeById("killfeed2") as Div,
                gui.GetNodeById("killfeed1") as Div
            ];
            for (int i = 0; i < killFeed.Count; i++)
            {
                KillFeedEntry entry = killFeed[i];
                Point pos = killFeedDivs[i].GetPos();
                entry.Render(drawer, pos);
            }

            if (world.gameMode.IsWinner(character))
            {
                drawer.DrawTexture("victory", 0, 0);
            }
        }

        public void AddChatFeedEntry(ChatFeedEntry entry)
        {
            chatFeed.Add(entry);
            if (chatFeed.Count > 5)
            {
                chatFeed.RemoveAt(0);
            }
        }

        public void AddKillFeedEntry(KillFeedEntry entry)
        {
            killFeed.Add(entry);
            if (killFeed.Count > 5)
            {
                killFeed.RemoveAt(0);
            }
        }

        public void SetAlert1(string alert, int time = 300)
        {
            alert1 = alert;
            alert1Time = time;
        }

        public void SetAlert2(string alert, int time = 300)
        {
            alert2 = alert;
            alert2Time = time;
        }

        public void ToggleMap()
        {
            if (hideMap) Game.PlaySound("map on");
            else Game.PlaySound("map off");
            hideMap = !hideMap;
        }
    }
}
