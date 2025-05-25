using Shared;

namespace Royale2D
{
    // IMPROVE abstract read/write with my documents settings
    public class Options
    {
        public string playerName = "player1";
        public Guid playerGuid = Guid.NewGuid();    // IMPROVE move to separate file so it has less chance of erasure
        public string skin = "";
        //public float musicVolume = 1;
        //public float soundVolume = 1;
        //public bool showFPS = false;
        //public bool showSysReqPrompt = true;
        //public bool enableDeveloperConsole;

        public string relayServerIp = Helpers.GetLocalIpv4Address();

        // Video settings
        public bool fullScreen = false;
        public UIQuality uiQuality = UIQuality.High; 
        
        public int windowScale = 4;
        public static readonly List<string> WindowScaleOptions = new List<string>
        { 
            "1x", "2x", "3x", "4x", "5x", "6x", "7x", "8x", "9x", "10x", "11x", "12x", "13x", "14x", "15x" 
        };
        public uint GetWindowScale()
        {
            return (uint)windowScale + 1;
        }

        public int soundVolume = 100;
        public int musicVolume = 100;

        /*
        public bool disableShaders;
        public bool areShadersDisabled()
        {
            if (Global.disableShaderOverride) return true;
            return disableShaders;
        }
        */

        public const int MaxPlayerNameLength = 10;

        private static Options _main;
        public static Options main
        {
            get
            {
                if (_main == null)
                {
                    string text = FilePath.New("options.txt").ReadAllText();
                    if (string.IsNullOrEmpty(text))
                    {
                        _main = new Options();
                    }
                    else
                    {
                        try
                        {
                            _main = JsonHelpers.DeserializeJson<Options>(text);
                        }
                        catch
                        {
                            throw new Exception("Your options.txt file is corrupted, or does no longer work with this version. Please delete it and launch the game again.");
                        }
                    }

                    _main.Validate();
                }

                return _main;
            }
        }

        public void SaveToFile()
        {
            //string text = JsonConvert.SerializeObject(_main);
            //Helpers.WriteToFile("options.txt", text);
        }

        public void Validate()
        {
            /*
            if (playerName != null && playerName.Length > MaxPlayerNameLength)
            {
                playerName = playerName.Substring(0, MaxPlayerNameLength);
            }
            
            playerName = Regex.Replace(playerName, @"[^\u0000-\u007F]+", "?"); //Remove non ASCII chars to prevent possible issues
            */
        }

        public PlayerRequestData getPlayerRequestData()
        {
            return new PlayerRequestData(playerName, skin, playerGuid);
        }

        /*
        public bool IsDeveloperConsoleEnabled()
        {
            if (Debug.debug)
            {
                return true;
            }
            else
            {
                return enableDeveloperConsole;
            }
        }
        */
    }
}
