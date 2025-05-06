using ProtoBuf;
using Shared;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Royale2D
{
    public class Helpers : SharedHelpers
    {
        public static (float x, float y) GetAlignmentOriginFloat(string alignment, float w, float h)
        {
            switch (alignment)
            {
                case Alignment.TopLeft:
                    return (0, 0);
                case Alignment.TopMid:
                    return (MyMath.Floor(w / 2), 0);
                case Alignment.TopRight:
                    return (w, 0);
                case Alignment.MidLeft:
                    return (0, MyMath.Floor(h / 2));
                case Alignment.Center:
                    return (MyMath.Floor(w / 2), MyMath.Floor(h / 2));
                case Alignment.MidRight:
                    return (w, MyMath.Floor(h / 2));
                case Alignment.BotLeft:
                    return (0, h);
                case Alignment.BotMid:
                    return (MyMath.Floor(w / 2), h);
                case Alignment.BotRight:
                    return (w, h);
                default:
                    {
#if DEBUG
                        throw new ArgumentException("Invalid alignment value");
#else
                        return (0, 0);
#endif
                    }
            }
        }

        public static (int x, int y) GetAlignmentOriginInt(string alignment, int w, int h)
        {
            switch (alignment)
            {
                case Alignment.TopLeft:
                    return (0, 0);
                case Alignment.TopMid:
                    return (w / 2, 0);
                case Alignment.TopRight:
                    return (w, 0);
                case Alignment.MidLeft:
                    return (0, h / 2);
                case Alignment.Center:
                    return (w / 2, h / 2);
                case Alignment.MidRight:
                    return (w, h / 2);
                case Alignment.BotLeft:
                    return (0, h);
                case Alignment.BotMid:
                    return (w / 2, h);
                case Alignment.BotRight:
                    return (w, h);
                default:
                    {
#if DEBUG
                        throw new ArgumentException("Invalid alignment value");
#else
                        return (0, 0);
#endif
                    }
            }
        }

        public static string ComputeMd5Hash(byte[] data)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(data);

                // Convert the byte array to a hexadecimal string
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sBuilder.Append(hashBytes[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static FdPoint GetInputNormFdVec(IInputReader inputReader)
        {
            FdPoint move = FdPoint.Zero;
            if (inputReader.IsHeld(Control.Left))
            {
                move.x = -1;
            }
            else if (inputReader.IsHeld(Control.Right))
            {
                move.x = 1;
            }
            if (inputReader.IsHeld(Control.Up))
            {
                move.y = -1;
            }
            else if (inputReader.IsHeld(Control.Down))
            {
                move.y = 1;
            }

            if (move.x != 0 && move.y != 0)
            {
                if (move.x != 0) move.x *= Fd.New(0, 71);
                if (move.y != 0) move.y *= Fd.New(0, 71);
            }

            return move;
        }


        // FYI this is hard coded. So if a client wants to use different ports, they can't unless we refactor this and have client tell server of custom port to use.
        // REFACTOR move elsewhere
        public static int GetUdpPeerListenPort(int playerId)
        {
            return RelayServer.baseUdpPort + 1 + playerId;
        }

        public static void CalculateMinimapOffsets(int mapWidth, int mapHeight, int minimapWidth, int minimapHeight, out float offsetX, out float offsetY, out float minimapRatio)
        {
            // Calculate the ratio of the map dimensions to the minimap dimensions
            float mapAspectRatio = (float)mapWidth / mapHeight;
            float minimapAspectRatio = (float)minimapWidth / minimapHeight;

            if (mapAspectRatio > minimapAspectRatio)
            {
                // Map is proportionally wider than the minimap
                float scale = (float)minimapWidth / mapWidth;
                float scaledHeight = mapHeight * scale;
                offsetY = (minimapHeight - scaledHeight) / 2;
                offsetX = 0;
                minimapRatio = scale;
            }
            else
            {
                // Map is proportionally taller than the minimap
                float scale = (float)minimapHeight / mapHeight;
                float scaledWidth = mapWidth * scale;
                offsetX = (minimapWidth - scaledWidth) / 2;
                offsetY = 0;
                minimapRatio = scale;
            }
        }


        public static List<T> RepeatedDefault<T>(int count)
        {
            return Repeated(default(T), count);
        }

        public static List<T> Repeated<T>(T value, int count)
        {
            List<T> ret = new List<T>(count);
            ret.AddRange(Enumerable.Repeat(value, count));
            return ret;
        }

        static Random random = new Random();
        // Do NOT use this for gameplay logic, only rendering, or you will get desyncs. Use NetcodeSafety.RandomRange() for gameplay logic
        public static int RandomRange(int start, int end)
        {
            return random.Next(start, end + 1);
        }

        public static IntPoint DirToVec(Direction dir)
        {
            if (dir == Direction.Up) return new IntPoint(0, -1);
            if (dir == Direction.Down) return new IntPoint(0, 1);
            if (dir == Direction.Left) return new IntPoint(-1, 0);
            if (dir == Direction.Right) return new IntPoint(1, 0);
            return new IntPoint(0, 1);
        }

        public static FdPoint DirToFdVec(Direction dir)
        {
            if (dir == Direction.Up) return new FdPoint(0, -1);
            if (dir == Direction.Down) return new FdPoint(0, 1);
            if (dir == Direction.Left) return new FdPoint(-1, 0);
            if (dir == Direction.Right) return new FdPoint(1, 0);
            return new FdPoint(0, 1);
        }

        public static string BoolToYesNo(bool val)
        {
            return val ? "Yes" : "No";
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }

        public static byte[] Serialize<T>(T obj)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        public static T DeserializeBase64<T>(string encodedObj)
        {
            return Deserialize<T>(Convert.FromBase64String(encodedObj));
        }

        public static string SerializeBase64<T>(T obj)
        {
            return Convert.ToBase64String(Serialize(obj));
        }


        public static T CloneProtobuf<T>(T obj)
        {
            return Deserialize<T>(Serialize(obj));
        }

        // Do NOT use this for anything besides local debugging since it may not get the right IPv4 address in some cases
        public static string GetLocalIpv4Address()
        {
#if DEBUG
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
#endif
            return "";
        }


        public static string InsertNewlines(string message, int charsPerLine)
        {
            string retMsg = "";
            int i = 0;
            foreach (char c in message)
            {
                retMsg += c;
                if (i == charsPerLine && c == ' ')
                {
                    retMsg += "\n";
                    i = 0;
                }
                else
                {
                    i++;
                }
                i++;
            }
            return retMsg;
        }

        public static void Assert(bool condition, string message, bool throwOnFailureInDebug = false)
        {
#if DEBUG
            if (!condition)
            {
                Console.WriteLine("Assertion failed: " + message);
                if (throwOnFailureInDebug)
                {
                    throw new Exception("Assertion failed: " + message);
                }
                else
                {
                    Debugger.Break();
                }
            }
#endif
        }

        public static void ThrowWhenDebugging(Action action, string errorMessagePrefix)
        {
            if (Debugger.IsAttached)
            {
                action.Invoke();
            }
            else
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(errorMessagePrefix + ": " + ex.Message);
                }
            }
        }

        public static void IncCycle(ref int itemIndex, int count)
        {
            itemIndex++;
            if (itemIndex >= count) itemIndex = 0;
        }

        public static void DecCycle(ref int itemIndex, int count)
        {
            itemIndex--;
            if (itemIndex < 0) itemIndex = count - 1;
        }

        public static void IncClamp(ref int itemIndex, int count)
        {
            itemIndex++;
            if (itemIndex >= count) itemIndex = count - 1;
        }

        public static void DecClampZero(ref int itemIndex)
        {
            if (itemIndex > 0)
            {
                itemIndex--;
                if (itemIndex < 0) itemIndex = 0;
            }
        }

        public static string GetPlaceText(int place)
        {
            if (place <= 0) return "0th";
            int lastDigit = place % 10;
            int lastTwoDigits = place % 100;
            if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
            {
                return place + "th";
            }
            switch (lastDigit)
            {
                case 1:
                    return place + "st";
                case 2:
                    return place + "nd";
                case 3:
                    return place + "rd";
                default:
                    return place + "th";
            }
        }

        public static bool AreClose(float v1, float v2, float epsilon)
        {
            return Math.Abs(v1 - v2) < epsilon;
        }

        public static T LoadDevData<T>(string devDataName)
        {
            return JsonHelpers.DeserializeJsonFile<T>(Assets.assetPath.AppendFolder("dev").AppendFile(devDataName + ".json"));
        }

        public static SFML.Graphics.Image ScaleDownImage(SFML.Graphics.Image original, uint newWidth, uint newHeight)
        {
            var scaled = new SFML.Graphics.Image(newWidth, newHeight);

            float xRatio = original.Size.X / (float)newWidth;
            float yRatio = original.Size.Y / (float)newHeight;

            for (uint y = 0; y < newHeight; y++)
            {
                for (uint x = 0; x < newWidth; x++)
                {
                    uint px = (uint)(x * xRatio);
                    uint py = (uint)(y * yRatio);
                    scaled.SetPixel(x, y, original.GetPixel(px, py));
                }
            }

            return scaled;
        }


        // Return all GridCoords that this rectangle covers
        public static List<GridCoords> GetOverlappingGridCoords<T>(T[,] grid, IntRect rect, int cellSize)
        {
            var gridCoords = new List<GridCoords>();
            var topLeftGridCoords = new GridCoords(MyMath.Floor(rect.y1 / cellSize), MyMath.Floor(rect.x1 / cellSize));
            var botRightGridCoords = new GridCoords(MyMath.Floor(rect.y2 / cellSize), MyMath.Floor(rect.x2 / cellSize));
            for (var i = topLeftGridCoords.i; i <= botRightGridCoords.i; i++)
            {
                for (var j = topLeftGridCoords.j; j <= botRightGridCoords.j; j++)
                {
                    if (grid.InRange(i, j))
                    {
                        gridCoords.Add(new GridCoords(i, j));
                    }
                }
            }
            return gridCoords;
        }

        public static List<GridCoords> GetOverlappingGridCoords(int width, int height, IntRect rect, int cellSize)
        {
            var gridCoords = new List<GridCoords>();
            var topLeftGridCoords = new GridCoords(MyMath.Floor(rect.y1 / cellSize), MyMath.Floor(rect.x1 / cellSize));
            var botRightGridCoords = new GridCoords(MyMath.Floor(rect.y2 / cellSize), MyMath.Floor(rect.x2 / cellSize));
            for (var i = topLeftGridCoords.i; i <= botRightGridCoords.i; i++)
            {
                for (var j = topLeftGridCoords.j; j <= botRightGridCoords.j; j++)
                {
                    if (i >= 0 && j >= 0 && i < height && j < width)
                    {
                        gridCoords.Add(new GridCoords(i, j));
                    }
                }
            }
            return gridCoords;
        }

        public static void PruneFrameDictBelow<T>(Dictionary<int, T> frameDict, int frameNumCutoff, bool inclusive)
        {
            foreach (int key in frameDict.Keys.ToList())
            {
                if (key < frameNumCutoff + (inclusive ? 1 : 0))
                {
                    frameDict.Remove(key);
                }
            }
        }

        public static void PruneFrameDictAbove<T>(Dictionary<int, T> frameDict, int frameNumCutoff, bool inclusive)
        {
            foreach (int key in frameDict.Keys.ToList())
            {
                if (key > frameNumCutoff - (inclusive ? 1 : 0))
                {
                    frameDict.Remove(key);
                }
            }
        }

        // Only works for i and j that are in ushort size range. Only designed for map tile grid coords
        public static int GetIJHashCode(int i, int j)
        {
            ushort uI = (ushort)i;
            ushort uJ = (ushort)j;
            return uI << 16 | uJ;
        }
    }
}
