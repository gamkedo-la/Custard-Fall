using FishNet.Documenting;
using FishNet.Managing.Server;
using FishNet.Serializing;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FishNet.Runtime
{
    [APIExclude]
    [StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
    public static class ConnectionReadersAndWriters
    {
        public static void WriteClientConnectionChangeBroadcast(this Writer writer,
            ClientConnectionChangeBroadcast value)
        {
            writer.WriteBoolean(value.Connected);
            writer.WriteNetworkConnectionId((short) value.Id);
        }

        public static ClientConnectionChangeBroadcast ReadClientConnectionChangeBroadcast(this Reader reader)
        {
            return new ClientConnectionChangeBroadcast()
            {
                Connected = reader.ReadBoolean(),
                Id = reader.ReadNetworkConnectionId()
            };
        }

        public static void WriteConnectedClientsBroadcast(this Writer writer, ConnectedClientsBroadcast value)
        {
            var count = (ushort) value.ListCache.Written;
            writer.WriteUInt16(count);

            var collection = value.ListCache.Collection;
            for (var i = 0; i < count; i++)
                writer.WriteNetworkConnectionId((short) collection[i]);
        }

        public static ConnectedClientsBroadcast ReadConnectedClientsBroadcast(this Reader reader)
        {
            int count = reader.ReadUInt16();
            var collection = new List<int>(count);
            var result = new ConnectedClientsBroadcast()
            {
                Ids = collection
            };

            for (var i = 0; i < count; i++)
                collection.Add(reader.ReadNetworkConnectionId());

            return result;
        }
    }
}