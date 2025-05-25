using Shared;

namespace Royale2D
{
    public class TileClump
    {
        public int[,] tileIds;
        public string name = "";
        public string properties = "";
        public string tags = "";
        public string transformTileClumpNameCsv = "";

        public string transformClumpName = "";
        // Support a few alternate clumps to transform into
        public string transformClumpName2 = "";
        public string transformClumpName3 = "";
        public string transformClumpName4 = "";

        public string transformSound = "";
        public string liftSprite = "";
        public string breakSound = "";
        public string breakSprite = "";

        public List<TileClumpSubsectionModel> subsections = [];

        public int rows => tileIds.GetLength(0);
        public int cols => tileIds.GetLength(1);

        public void Init()
        {
            if (transformTileClumpNameCsv.IsSet())
            {
                string[] pieces = transformTileClumpNameCsv.Split();
                transformClumpName = pieces[0];
                transformClumpName2 = pieces.SafeGet(1) ?? "";
                transformClumpName3 = pieces.SafeGet(2) ?? "";
                transformClumpName4 = pieces.SafeGet(3) ?? "";
            }

            AutoSetFieldsFromNamingConventions();
            ParseFromProperties();
        }

        public bool CheckIfClumpMatches(int[,] otherTileIds, int i, int j)
        {
            for (int k = 0; k < rows; k++)
            {
                for (int l = 0; l < cols; l++)
                {
                    if (i + k >= otherTileIds.GetLength(0) || j + l >= otherTileIds.GetLength(1))
                    {
                        return false;
                    }
                    if (tileIds[k, l] != otherTileIds[i + k, j + l])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void ParseFromProperties()
        {
            properties.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(property =>
            {
                string[] split = property.Split('=');
                string key = split[0];
                string value = split[1];
                switch (key)
                {
                    case "liftsprite":
                        liftSprite = value;
                        break;
                    case "breaksprite":
                        breakSprite = value;
                        break;
                }
            });
        }

        // Methods like these promote "convention over configuration", reducing configuration required of tile clumps in the editor.
        // However it's a hotly contested subject, and there is an argument explicit is better. We can always respect manual config
        // by IsSet() checks for some cases like liftables where there is more variation and only setting if it doesn't exist.
        public void AutoSetFieldsFromNamingConventions()
        {
            if (tags.Unset())
            {
                tags = TileClumpTags.GetTagsFromName(name);
            }
            if (tags.Contains(TileClumpTags.Chest))
            {
                transformClumpName = name.TrimEndDigits() + "Open";
                transformSound = "chest open";
            }
            if (tags.Contains(TileClumpTags.Door))
            {
                transformClumpName = name.TrimEndDigits() + "Open";
                transformSound = "door open";
            }
            if (tags.Contains(TileClumpTags.CrackedWall))
            {
                transformClumpName = name.TrimEndDigits() + "Open";
            }
            if (tags.Contains(TileClumpTags.Liftable))
            {
                if (!transformClumpName.IsSet())
                {
                    transformClumpName = name + "Base";
                }
            }
            if (tags.Contains(TileClumpTags.Cuttable))
            {
                transformClumpName = name + "Base";
                breakSprite = "grass_break";
            }
            if (tags.Contains(TileClumpTags.Bush))
            {
                breakSound = "grass destroyed";
            }
            if (tags.Contains(TileClumpTags.TallGrass))
            {
                breakSound = "grass destroyed";
            }
            if (!breakSound.IsSet())
            {
                breakSound = "rock break";
            }
        }
    }

    public class TileClumpTags
    {
        public const string Sign = "sign";
        public const string Stake = "stake";
        public const string Bush = "bush";
        public const string Chest = "chest";
        public const string Door = "door";
        public const string TallGrass = "tallgrass";
        public const string CrackedWall = "crackedwall";
        public const string Cuttable = "cuttable";
        public const string Hookable = "hookable";
        public const string ProjFlyOver = "projflyover";
        public const string Liftable = "liftable";
        public const string LiftableGlove1 = "liftableglove1";
        public const string LiftableGlove2 = "liftableglove2";

        public static string GetTagsFromName(string name)
        {
            if (name.StartsWith("Bush") && !name.EndsWith("Base"))
            {
                return Bush + "," + Cuttable + "," + Liftable + "," + ProjFlyOver;
            }
            else if (name.StartsWith("ChestSmall") || name.StartsWith("ChestBig"))
            {
                if (name.EndsWith("Open"))
                {
                    return ProjFlyOver;
                }
                else
                {
                    return Chest + "," + ProjFlyOver;
                }
            }
            else if (name.StartsWith("Grass") && !name.EndsWith("Base"))
            {
                return TallGrass + "," + Cuttable;
            }
            else if (name.StartsWith("Door") && !name.EndsWith("Open"))
            {
                return Door;
            }
            else if (name.StartsWith("CW") && !name.EndsWith("Open"))
            {
                return CrackedWall + ",";
            }
            else if (name.StartsWith("Pot") && !name.EndsWith("Base"))
            {
                return Liftable + ProjFlyOver;
            }
            else if (name.StartsWith("Rock") && name.Contains("Gray") && !name.EndsWith("Base"))
            {
                return Liftable + "," + LiftableGlove1 + "," + ProjFlyOver;
            }
            else if (name.StartsWith("Rock") && name.Contains("Black") && !name.EndsWith("Base"))
            {
                return Liftable + "," + LiftableGlove2 + "," + ProjFlyOver;
            }
            else if (name.StartsWith("Sign") && !name.EndsWith("Base"))
            {
                return Sign + "," + Liftable + "," + ProjFlyOver;
            }
            return "";
        }
    }

    public struct TileClumpInstance
    {
        public TileClump tileClump;
        public int i1;
        public int j1;
        public int i2;
        public int j2;

        public TileClumpInstance(TileClump tileClump, int i1, int j1)
        {
            this.tileClump = tileClump;
            this.i1 = i1;
            this.j1 = j1;
            i2 = i1 + (tileClump.rows - 1);
            j2 = j1 + (tileClump.cols - 1);
        }

        public string GetKey()
        {
            return tileClump.name + ":" + i1 + "," + j1 + "," + i2 + "," + j2;
        }

        public int GetPixelWidth()
        {
            return 8 * (j2 - j1 + 1);
        }

        public int GetPixelHeight()
        {
            return 8 * (i2 - i1 + 1);
        }

        public int GetTileWidth()
        {
            return j2 - j1 + 1;
        }

        public int GetTileHeight()
        {
            return i2 - i1 + 1;
        }

        public FdPoint GetCenterPos()
        {
            return new FdPoint(
                8 * (j1 + ((j2 - j1 + 1) / 2)),
                8 * (i1 + ((i2 - i1 + 1) / 2))
            );
        }
    }
}
