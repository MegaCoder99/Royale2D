namespace Royale2D
{
    public class InputSyncer
    {
        public LocalPlayerSyncedInputs localPlayer;
        public List<RemotePlayerSyncedInputs> remotePlayers = new List<RemotePlayerSyncedInputs>();
        public OnlineWorldHost onlineWorldHost;
        public OnlineMatch onlineMatch => onlineWorldHost.onlineMatch;
        public Func<FrameInput> getFrameInput;

        public int frameNum;
        public int lastCompleteFrame;

        public int delayFrames;
        public readonly int maxDelayFrames;
        public int frameNumPlusDelay => frameNum + delayFrames;

        // Diagnostics only
        public int sentBytes;
        public int receivedBytes;

        // Simulator project only
        public int lastFrameChecked;

        public InputSyncer(
            SyncedPlayerData localPlayer,
            List<SyncedPlayerData> remotePlayers,
            OnlineWorldHost onlineWorldHost,
            Func<FrameInput> getFrameInput,
            int initialDelayFrames,
            int maxDelayFrames)
        {
            this.localPlayer = new LocalPlayerSyncedInputs(localPlayer);
            this.remotePlayers = remotePlayers.Select(rp => new RemotePlayerSyncedInputs(rp)).ToList();
            this.onlineWorldHost = onlineWorldHost;
            this.getFrameInput = getFrameInput;
            this.delayFrames = initialDelayFrames;
            this.maxDelayFrames = maxDelayFrames;
        }

        public HashSet<int> desyncDetectFramesUsed = new();
        public void Update()
        {
            if (onlineMatch.disconnected)
            {
                return; // Client at this point is toast.
            }

            if (!localPlayer.frameToInputs.ContainsKey(frameNumPlusDelay))
            {
                localPlayer.frameToInputs[frameNumPlusDelay] = getFrameInput.Invoke();
            }

            // This for block serves 2 purpose: it initializes initial empty frame inputs on frame 0, and also fills in empty frame inputs if frame delay increased dynamically
            for (int i = frameNumPlusDelay - 1; i >= frameNumPlusDelay - 1 - maxDelayFrames; i--)
            {
                if (i >= 0 && !localPlayer.frameToInputs.ContainsKey(i))
                {
                    localPlayer.frameToInputs[i] = new FrameInput(0);
                }
            }

            foreach (RemotePlayerSyncedInputs remotePlayer in remotePlayers)
            {
                if (!remotePlayer.disconnected)
                {
                    SendInputs(remotePlayer);
                }
                else
                {
                    UpdateDisconnectData(remotePlayer);
                }
            }

            if (!IsWaitingForPlayers())
            {
                onlineWorldHost.world.Update(frameNum);

                frameNum++;
            }

            int lastCommonCompletedFrame = remotePlayers.Min(p => p.GetLastCompleteFrame());
            if (lastCommonCompletedFrame % 60 == 0 && !desyncDetectFramesUsed.Contains(lastCommonCompletedFrame))
            {
                desyncDetectFramesUsed.Add(lastCommonCompletedFrame);
                string hash = onlineWorldHost.world.GetStateHash();
                //Console.WriteLine($"Frame {lastCommonCompletedFrame} has hash {hash}");
                ClientToServerDesyncDetectUM desyncDetectUM = new ClientToServerDesyncDetectUM(localPlayer.id, lastCommonCompletedFrame, hash);
                UdpHelper.Send(onlineMatch.matchUdpClient.client, desyncDetectUM);
            }
        }

        public void SendInputs(RemotePlayerSyncedInputs otherPlayer)
        {
            if (onlineMatch.disconnected) return;
            if (otherPlayer.disconnected) return;

            Dictionary<int, FrameInput> framesToSend = GetInputFramesToSend(otherPlayer);
            // PERF do we even need to send if framesToSend is empty? (Consider the lastAckedFrame param too)
            int lastAckedFrame = otherPlayer.GetLastCompleteFrame();
            PeerToPeerInputUM sendInputMessage = new PeerToPeerInputUM(framesToSend, lastAckedFrame, localPlayer.id, otherPlayer.id);
            sentBytes += sendInputMessage.GetByteSize();    // NETCODE factor in packet overhead, batching, etc.
            UdpHelper.Send(onlineMatch.matchUdpClient.client, sendInputMessage);
        }

        public Dictionary<int, FrameInput> GetInputFramesToSend(RemotePlayerSyncedInputs otherPlayer)
        {
            Dictionary<int, FrameInput> framesToSend = new Dictionary<int, FrameInput>();

            for (int i = otherPlayer.lastFrameOfMineTheyAcked + 1; i <= frameNum + delayFrames; i++)
            {
                if (localPlayer.frameToInputs.ContainsKey(i))
                {
                    framesToSend[i] = localPlayer.frameToInputs[i];
                }
            }

            return framesToSend;
        }

        public void OnReceiveInputMessage(PeerToPeerInputUM message)
        {
            receivedBytes += message.GetByteSize();
            RemotePlayerSyncedInputs otherPlayer = GetRemotePlayerById(message.senderId);
            if (!otherPlayer.disconnected)
            {
                if (message.inputs != null)
                {
                    foreach ((int frame, FrameInput receivedInputs) in message.inputs)
                    {
                        otherPlayer.frameToInputs[frame] = receivedInputs;
                    }
                }
                otherPlayer.lastFrameOfMineTheyAcked = message.lastAckedFrame;
            }
        }

        // When a player disconnects, we need consensus across all peers on what their final inputs were before their disconnect to avoid desyncs
        // Some peers may have gotten more of the final inputs of the DC'ing player than others. We need the server to synthesize all such inputs
        // from all remaining peers and sync them back to the rest of the remaining peers
        public void UpdateDisconnectData(RemotePlayerSyncedInputs disconnectedPlayer)
        {
            if (disconnectedPlayer.disconnectData.finalFrameInputs == null)
            {
                int lastCompletedFrameISaw = disconnectedPlayer.GetLastCompleteFrame();
                var lastCompletedFramesISaw = new Dictionary<int, FrameInput>();

                // Should never be behind more than the number of delay frames - 1, even with respect to a DC'ing player.
                // The extra - 2 is just safety to account for potential off-by-one errors, and maxDelayFrames covers us for dynamic delay frames
                for (int i = Math.Max(frameNum - maxDelayFrames - 3, 0); i <= lastCompletedFrameISaw; i++)
                {
                    lastCompletedFramesISaw[i] = disconnectedPlayer.frameToInputs[i];
                }

                UdpHelper.Send(onlineMatch.matchUdpClient.client, new ClientToServerDcFramesUM(localPlayer.id, disconnectedPlayer.id, lastCompletedFramesISaw));
            }
            else if (!disconnectedPlayer.disconnectData.finalFrameInputsProcessed)
            {
                Console.WriteLine($"Player {localPlayer.id} obtained disconnect data from player {disconnectedPlayer.id}");
                foreach (KeyValuePair<int, FrameInput> entry in disconnectedPlayer.disconnectData.finalFrameInputs)
                {
                    disconnectedPlayer.frameToInputs[entry.Key] = entry.Value;
                }

                // ASSERT can assert that there isn't an empty framenum before the last frame in disconnectData.lastFrameInputs

                disconnectedPlayer.disconnectData.finalFrameInputsProcessed = true;
            }
        }

        public void OnReceiveDisconnectFrames(int disconnectorId, Dictionary<int, FrameInput> finalFrameInputs)
        {
            RemotePlayerSyncedInputs disconnector = GetRemotePlayerById(disconnectorId);
            if (disconnector.disconnectData != null)
            {
                // ASSERT can assert disconnector.disconnectData != null
                disconnector.disconnectData.finalFrameInputs = finalFrameInputs;
            }
        }

        public bool IsWaitingForPlayers()
        {
            if (onlineMatch.matchUdpClient.IsLagging())
            {
                return true;
            }

            foreach (RemotePlayerSyncedInputs remotePlayer in remotePlayers)
            {
                if (remotePlayer.IsWaitingForPlayer(frameNum))
                {
                    return true;
                }
            }
            return false;
        }

        public RemotePlayerSyncedInputs GetRemotePlayerById(int id)
        {
            return remotePlayers.First(p => p.id == id);
        }

        public void DumpDebugInputs(int playerId)
        {
            string inputs = "";
            if (playerId == localPlayer.id)
            {
                foreach (KeyValuePair<int, FrameInput> entry in localPlayer.frameToInputs)
                {
                    inputs += $"Frame {entry.Key}: {entry.Value.bits}\n";
                }
            }
            else
            {
                var remotePlayer = remotePlayers.First(rp => rp.id == playerId);
                foreach (KeyValuePair<int, FrameInput> entry in remotePlayer.frameToInputs)
                {
                    inputs += $"Frame {entry.Key}: {entry.Value.bits}\n";
                }
            }
            File.WriteAllText($@"C:\users\username\desktop\dump_player_{localPlayer.id}.txt", inputs);
        }
    }
}
