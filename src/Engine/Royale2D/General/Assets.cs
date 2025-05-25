using SFML.Graphics;
using System.Text;
using Image = SFML.Graphics.Image;
using Color = SFML.Graphics.Color;
using SFML.Audio;
using Shared;

namespace Royale2D
{
    public static class Assets
    {
#if DEBUG
        public static readonly FolderPath assetPath = Debug.main?.customAssets == true ?
            new FolderPath(Debug.main.customAssetsPath) :
            new FolderPath("../../../../../../assets");
#else
        public static readonly FolderPath assetPath = new FolderPath("./assets");
#endif

        public static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
        public static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        public static Dictionary<string, Map> maps = new Dictionary<string, Map>();
        public static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
        public static Dictionary<string, ShaderInstance> shaderInstances = new Dictionary<string, ShaderInstance>();
        public static Dictionary<string, string> shaderCodes = new Dictionary<string, string>();
        public static Dictionary<string, Gui> guis = new Dictionary<string, Gui>();
        public static Dictionary<string, SoundBuffer> soundBuffers = new Dictionary<string, SoundBuffer>();
        public static Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();
        public static Dictionary<string, MusicData> musicDatas = new Dictionary<string, MusicData>();
        public static Dictionary<string, BitmapFont> bitmapFonts = new Dictionary<string, BitmapFont>();

        private static Sprite? _emptySprite;
        public static Sprite emptySprite
        {
            get
            {
                if (_emptySprite == null)
                {
                    var emptyFrame = new Frame(new IntRect(0, 0, 8, 8), 4, IntPoint.Zero, "");
                    emptyFrame.texture = new Texture(8, 8);
                    _emptySprite = new Sprite([emptyFrame], "");
                    _emptySprite.Init("empty");
                }
                return _emptySprite;
            }
        }

        public static Sprite GetSprite(string spriteName)
        {
            if (!sprites.ContainsKey(spriteName))
            {
                return emptySprite;
            }
            return sprites[spriteName];
        }

        public static Sound GetSound(string soundName)
        {
            if (!sounds.ContainsKey(soundName))
            {
                return sounds["empty"];
            }
            return sounds[soundName];
        }

        public static SoundBuffer GetSoundBuffer(string soundBufferName)
        {
            if (!soundBuffers.ContainsKey(soundBufferName))
            {
                return soundBuffers["empty"];
            }
            return soundBuffers[soundBufferName];
        }

        public static List<string> skins = new List<string>();

        public const string noShaderSupportMsg = "The system does not support shaders.";

        public static void Init()
        {
            LoadImages();
            LoadSprites();
            LoadSkins();
            LoadMaps();
            LoadBitmapFonts();
            LoadGuis();
            LoadSounds();
            LoadMusics();
            LoadShaders();
        }

        // IMPROVE For mod-ability, these load functions could be recursive, and do Add() so it throws on dups

        public static void LoadMusics()
        {
            List<FilePath> musicFilePaths = assetPath.AppendFolder("music").GetFiles(false, "ogg", "mp3");

            foreach (FilePath musicFilePath in musicFilePaths)
            {
                var musicData = new MusicData(musicFilePath);
                musicDatas[musicData.name] = musicData;
            }
        }

        public static void LoadSounds()
        {
            List<FilePath> soundFilePaths = assetPath.AppendFolder("sounds").GetFiles(false, "wav", "ogg");

            foreach (FilePath soundFilePath in soundFilePaths)
            {
                SoundBuffer soundBuffer = new SoundBuffer(soundFilePath.fullPath);
                string baseFileName = soundFilePath.fileNameNoExt;
                soundBuffers[baseFileName] = soundBuffer;
                Sound sound = new Sound(soundBuffer);
                sounds[baseFileName] = sound;
            }
        }

        public static void LoadGuis()
        {
            List<FilePath> guiFilePaths = assetPath.AppendFolder("gui").GetFiles(false, "xml");

            foreach (FilePath guiFilePath in guiFilePaths)
            {
                Gui gui = Gui.FromFile(guiFilePath.fullPath);
                string baseFileName = guiFilePath.fileNameNoExt;
                guis[baseFileName] = gui;
            }
        }

        public static void LoadImages()
        {
            List<FilePath> images = assetPath.AppendFolder("images").GetFiles(true, "png");
            foreach (FilePath path in images)
            {
                string name = path.fileNameNoExt;
                Texture texture = new Texture(path.fullPath);
                textures.Add(name, texture);
            }
        }

        public static void LoadSprites()
        {
            var spriteWorkspace = new SpriteWorkspace(assetPath.fullPath);
            spriteWorkspace.LoadFromDisk(true);

            List<SpritesheetModel> spritesheets = spriteWorkspace.spritesheets;
            foreach (SpritesheetModel spritesheet in spritesheets)
            {
                Texture texture = new(spritesheet.filePath.fullPath);
                textures.Add(spritesheet.name.Split('.')[0], texture);
            }

            List<FilePath> spriteFilePaths = assetPath.AppendFolder("sprites").GetFiles(true, "json");

            foreach (FilePath spriteFilePath in spriteFilePaths)
            {
                Sprite sprite = spriteFilePath.DeserializeJson<Sprite>();
                sprite.Init(spriteFilePath.fileNameNoExt);
                sprites[sprite.name] = sprite;
            }
        }

        public static void LoadSkins()
        {
            List<FilePath> skinFilePaths = assetPath.AppendFolder("skins").GetFiles(false, "png");

            foreach (FilePath skinFilePath in skinFilePaths)
            {
                string skinName = skinFilePath.fileNameNoExt;
                Texture texture = new Texture(skinFilePath.fullPath);
                textures.Add(skinName, texture);
                skins.Add(skinName);
            }
        }

        public static void LoadMaps()
        {
            List<FolderPath> mapFolderPaths = assetPath.AppendFolder("maps").GetFolders().ToList();

            foreach (FolderPath mapFolderPath in mapFolderPaths)
            {
                MapWorkspace mapWorkspace = new MapWorkspace(mapFolderPath.fullPath);
                mapWorkspace.LoadFromDisk(true);
                var map = new Map(mapWorkspace);
                maps[map.name] = map;
            }
        }

        public static void LoadBitmapFonts()
        {
            List<FilePath> bitmapFontFilePaths = assetPath.AppendFolder("bitmap_fonts").GetFiles(false, "json");

            foreach (FilePath bitmapFontFilePath in bitmapFontFilePaths)
            {
                BitmapFont bitmapFont = bitmapFontFilePath.DeserializeJson<BitmapFont>();
                string baseFileName = bitmapFontFilePath.fileNameNoExt;
                bitmapFonts[baseFileName] = bitmapFont;
            }
        }

        public static Color ConvertStringToColor(string colorString)
        {
            if (colorString.StartsWith("#"))
            {
                colorString = colorString.Substring(1);
            }

            if (colorString.Length != 6)
            {
                throw new ArgumentException("Invalid color string format. It should be in the format #RRGGBB or RRGGBB.");
            }

            byte r = byte.Parse(colorString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(colorString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(colorString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color(r, g, b);
        }

        public static void LoadShaders()
        {
            List<FilePath> files = assetPath.AppendFolder("shaders").GetFiles(false, "frag");

            for (int i = 0; i < files.Count; i++)
            {
                string shaderContents = File.ReadAllText(files[i].fullPath);
                string shaderName = files[i].fileNameNoExt;
                
                shaderCodes[shaderName] = shaderContents;

                try
                {
                    shaders[shaderName] = CreateShader(shaderName);
                    shaderInstances[shaderName] = new ShaderInstance(shaderName);
                }
                catch (Exception e)
                {
                    /*
                    if (e.Message.Contains(Helpers.noShaderSupportMsg))
                    {
                        Global.shadersNotSupported = true;
                    }
                    else
                    {
                        Global.shadersFailed.Add(shaderName);
                    }
                    */
                }
            }
        }

        // Very slow, only do once on startup
        public static Shader CreateShader(string shaderName)
        {
            string shaderCode = shaderCodes[shaderName];

            var result = CreateShaderHelper(shaderCode, "");
            if (result != null) return result;

            if (!Shader.IsAvailable)
            {
                var ex = new Exception(noShaderSupportMsg);
                //Logger.logException(ex, false);
                throw ex;
            }

            var ex2 = new Exception("Could not load shaders after trying all possible opengl versions.");
            //Logger.logException(ex2, false);
            throw ex2;
        }

        // Very slow, only do once on startup
        public static Shader? CreateShaderHelper(string shaderCode, string header)
        {
            if (!string.IsNullOrEmpty(header)) header += Environment.NewLine;
            byte[] byteArray = Encoding.ASCII.GetBytes(header + shaderCode);
            MemoryStream stream = new MemoryStream(byteArray);
            try
            {
                return new Shader(null, null, stream);
            }
            catch
            {
                stream.Dispose();
                return null;
            }
        }

        // Fast way to get a new shader instance that remembers SetUniform state while reusing the same base underlying shader
        public static ShaderInstance? CreateShaderInstance(string shaderName)
        {
            if (!shaders.ContainsKey(shaderName))
            {
                return null;
            }
            return new ShaderInstance(shaderName);
        }
    }
}
