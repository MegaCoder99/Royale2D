using Lidgren.Network;
using Shared;
using System.Collections.Concurrent;
using System.Net;

namespace Royale2D
{
    public static class Extensions
    {
        public static string TrimEndDigits(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            int index = input.Length;

            // Find the first non-digit character from the end
            while (index > 0 && char.IsDigit(input[index - 1]))
            {
                index--;
            }

            // Return the substring without the trailing digits
            return input.Substring(0, index);
        }

        public static void StableSort<T>(this List<T> list, Comparison<T> comparison)
        {
            var indexedList = list.Select((item, index) => new { Item = item, Index = index })
                                   .ToList();

            indexedList.Sort((x, y) => {
                int result = comparison(x.Item, y.Item);
                return result != 0 ? result : x.Index.CompareTo(y.Index);
            });

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = indexedList[i].Item;
            }
        }

        public static T GetRandomElement<T>(this List<T> list)
        {
            return list[NetcodeSafeRng.RandomRange(0, list.Count - 1)];
        }

        public static Rectangle AddXY(this Rectangle rectangle, int x, int y)
        {
            return new Rectangle(rectangle.X + x, rectangle.Y + y, rectangle.Width, rectangle.Height);
        }

        public static int IntMultFrac(this int intVal, int numerator, int denominator)
        {
            return (intVal * numerator) / denominator;
        }

        public static bool IsValidIpAddress(this string ipAddressString)
        {
            if (string.IsNullOrEmpty(ipAddressString)) return false;
            return IPAddress.TryParse(ipAddressString, out _);
        }

        public static ushort GetUshortPing(this NetConnection netConnection, bool oneWayEstimate)
        {
            return (ushort)MyMath.Round(netConnection.AverageRoundtripTime * 1000 * (oneWayEstimate ? 0.5f : 1f));
        }

        public static void RemoveItem<T>(this ConcurrentBag<T> bag, T itemToRemove)
        {
            // Temporary collection to hold items that are not being removed
            ConcurrentBag<T> tempBag = new ConcurrentBag<T>();

            // Flag to indicate if the item was removed
            bool itemRemoved = false;

            while (bag.TryTake(out T item))
            {
                if (!itemRemoved && item.Equals(itemToRemove))
                {
                    // If the item matches the one to remove, do not add it to tempBag
                    itemRemoved = true; // Ensure only one instance is removed
                }
                else
                {
                    // Otherwise, transfer the item to tempBag
                    tempBag.Add(item);
                }
            }

            // Replace the original bag's contents with those in tempBag
            foreach (T remainingItem in tempBag)
            {
                bag.Add(remainingItem);
            }
        }

        /*
        public static NetConnection? ConnectWaitSuccess(this NetPeer peer, string ip, int port, float timeoutInSeconds, out string errorMessage)
        {
            errorMessage = "";
            NetConnection connection = peer.Connect(ip, port);
            int iterations = 0;

            while (connection.Status != NetConnectionStatus.Connected)
            {
                NetIncomingMessage? im;
                while ((im = peer.ReadMessage()) != null)
                {
                    peer.Recycle(im);
                }
                Thread.Sleep(100);
                iterations++;
                if (iterations > timeoutInSeconds * 10)
                {
                    connection.Disconnect("UDP Timeout");
                    errorMessage = "UDP Timeout";
                    return null;
                }
            }
            return connection;
        }

        public static NetConnection? ConnectWaitSuccess(this NetPeer peer, string ip, int port, float timeoutInSeconds, out string errorMessage)
        {
            errorMessage = "";
            NetConnection connection = peer.Connect(ip, port);
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (connection.Status != NetConnectionStatus.Connected)
            {
                NetIncomingMessage? im;
                while ((im = peer.ReadMessage()) != null)
                {
                    if (im.MessageType == NetIncomingMessageType.StatusChanged)
                    {
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();
                        if (status == NetConnectionStatus.Disconnected)
                        {
                            errorMessage = "Disconnected";
                            return null;
                        }
                    }
                    peer.Recycle(im);
                }

                if (stopwatch.Elapsed.TotalSeconds > timeoutInSeconds)
                {
                    connection.Disconnect("UDP Timeout");
                    errorMessage = "UDP Timeout";
                    return null;
                }

                Thread.Sleep(100);
            }

            return connection;
        }
        */
    }
}
