#if UNITY_EDITOR || DEVELOPMENT_BUILD
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FishNet.Managing.Debugging
{
    internal class ParseLogger
    {
        /// <summary>
        /// Contains the last several non-split packets to arrive. This is used for debugging.
        /// </summary>
        private Queue<PacketId> _incomingPacketIds = new();

        /// <summary>
        /// Maximum number of packets allowed to be queued.
        /// </summary>
        private const int PACKET_COUNT = 5;

        /// <summary>
        /// Resets data.
        /// </summary>
        internal void Reset()
        {
            _incomingPacketIds.Clear();
        }

        /// <summary>
        /// Adds a packet to data.
        /// </summary>
        /// <param name="pId"></param>
        internal void AddPacket(PacketId pId)
        {
            _incomingPacketIds.Enqueue(pId);
            if (_incomingPacketIds.Count > PACKET_COUNT)
                _incomingPacketIds.Dequeue();
        }

        /// <summary>
        /// Prints current data.
        /// </summary>
        internal void Print(NetworkManager nm)
        {
            if (nm == null)
            {
                if (!NetworkManager.StaticCanLog(LoggingType.Error))
                    return;
            }
            else
            {
                if (!nm.CanLog(LoggingType.Error))
                    return;
            }

            var sb = new StringBuilder();
            foreach (var item in _incomingPacketIds)
                sb.Insert(0, $"{item.ToString()}{Environment.NewLine}");

            var lastNob = Reader.LastNetworkObject;
            var nobData = lastNob == null ? "Unset" : $"Id {lastNob.ObjectId} on gameObject {lastNob.name}";
            var lastNb = Reader.LastNetworkBehaviour;
            var nbData = lastNb == null ? "Unset" : lastNb.GetType().Name;

            Debug.LogError(
                $"The last {_incomingPacketIds.Count} packets to arrive are: {Environment.NewLine}{sb.ToString()}");
            Debug.LogError($"The last parsed NetworkObject is {nobData}, and NetworkBehaviour {nbData}.");

            Reset();
        }
    }
}
#endif