using FishNet.Connection;
using FishNet.Managing.Object;
using FishNet.Object;
using FishNet.Observing;
using FishNet.Serializing;
using FishNet.Transporting;
using FishNet.Utility.Performance;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FishNet.Managing.Server
{
    public partial class ServerObjects : ManagedObjects
    {
        #region Private.

        /// <summary>
        /// Cache filled with objects which are being spawned on clients due to an observer change.
        /// </summary>
        private List<NetworkObject> _observerChangeObjectsCache = new(100);

        /// <summary>
        /// NetworkObservers which require regularly iteration.
        /// </summary>
        private List<NetworkObject> _timedNetworkObservers = new();

        /// <summary>
        /// Index in TimedNetworkObservers to start on next cycle.
        /// </summary>
        private int _nextTimedObserversIndex;

        #endregion

        /// <summary>
        /// Called when MonoBehaviours call Update.
        /// </summary>
        partial void PartialOnUpdate()
        {
            UpdateTimedObservers();
        }

        /// <summary>
        /// Progressively updates NetworkObservers with timed conditions.
        /// </summary>
        private void UpdateTimedObservers()
        {
            if (!NetworkManager.IsServer)
                return;
            var observersCount = _timedNetworkObservers.Count;
            if (observersCount == 0)
                return;

            var targetFps = 60;
            /* Multiply required frames based on connection count. This will
             * reduce how quickly observers update slightly but will drastically
             * improve performance. */
            var fpsMultiplier = 1f + (float) (NetworkManager.ServerManager.Clients.Count * 0.01f);
            /* Performing one additional iteration would
            * likely be quicker than casting two ints
            * to a float. */
            var iterations = observersCount / (int) (targetFps * fpsMultiplier) + 1;
            if (iterations > observersCount)
                iterations = observersCount;


            var everyoneWriter = WriterPool.GetWriter();
            var ownerWriter = WriterPool.GetWriter();

            //Index to perform a check on.
            var observerIndex = 0;
            foreach (var conn in NetworkManager.ServerManager.Clients.Values)
            {
                var cacheIndex = 0;
                using (var largeWriter = WriterPool.GetWriter())
                {
                    //Reset index to start on for every connection.
                    observerIndex = 0;
                    /* Run the number of calculated iterations.
                     * This is spaced out over frames to prevent
                     * fps spikes. */
                    for (var i = 0; i < iterations; i++)
                    {
                        observerIndex = _nextTimedObserversIndex + i;
                        /* Compare actual collection size not cached value.
                         * This is incase collection is modified during runtime. */
                        if (observerIndex >= _timedNetworkObservers.Count)
                            observerIndex -= _timedNetworkObservers.Count;

                        /* If still out of bounds something whack is going on.
                        * Reset index and exit method. Let it sort itself out
                        * next iteration. */
                        if (observerIndex < 0 || observerIndex >= _timedNetworkObservers.Count)
                        {
                            _nextTimedObserversIndex = 0;
                            break;
                        }

                        var nob = _timedNetworkObservers[observerIndex];
                        var osc = nob.RebuildObservers(conn, true);
                        if (osc == ObserverStateChange.Added)
                        {
                            everyoneWriter.Reset();
                            ownerWriter.Reset();
                            WriteSpawn(nob, conn, ref everyoneWriter, ref ownerWriter);
                            CacheObserverChange(nob, ref cacheIndex);
                        }
                        else if (osc == ObserverStateChange.Removed)
                        {
                            everyoneWriter.Reset();
                            WriteDespawn(nob, ref everyoneWriter);
                        }
                        else
                        {
                            continue;
                        }

                        /* Only use ownerWriter if an add, and if owner. Owner
                         * doesn't matter if not being added because no owner specific
                         * information would be included. */
                        var writerToUse = osc == ObserverStateChange.Added && nob.Owner == conn
                            ? ownerWriter
                            : everyoneWriter;

                        largeWriter.WriteArraySegment(writerToUse.GetArraySegment());
                    }

                    if (largeWriter.Length > 0)
                        NetworkManager.TransportManager.SendToClient(
                            (byte) Channel.Reliable,
                            largeWriter.GetArraySegment(), conn);

                    //Invoke spawn callbacks on nobs.
                    for (var i = 0; i < cacheIndex; i++)
                        _observerChangeObjectsCache[i].InvokePostOnServerStart(conn);
                }
            }

            everyoneWriter.Dispose();
            ownerWriter.Dispose();
            _nextTimedObserversIndex = observerIndex + 1;
        }

        /// <summary>
        /// Indicates that a networkObserver component should be updated regularly. This is done automatically.
        /// </summary>
        /// <param name="networkObject">NetworkObject to be updated.</param>
        public void AddTimedNetworkObserver(NetworkObject networkObject)
        {
            _timedNetworkObservers.Add(networkObject);
        }

        /// <summary>
        /// Indicates that a networkObserver component no longer needs to be updated regularly. This is done automatically.
        /// </summary>
        /// <param name="networkObject">NetworkObject to be updated.</param>
        public void RemoveTimedNetworkObserver(NetworkObject networkObject)
        {
            _timedNetworkObservers.Remove(networkObject);
        }

        /// <summary>
        /// Caches an observer change.
        /// </summary>
        /// <param name="cacheIndex"></param>
        private void CacheObserverChange(NetworkObject nob, ref int cacheIndex)
        {
            /* If this spawn would exceed cache size then
            * add instead of set value. */
            if (_observerChangeObjectsCache.Count <= cacheIndex)
                _observerChangeObjectsCache.Add(nob);
            else
                _observerChangeObjectsCache[cacheIndex] = nob;

            cacheIndex++;
        }

        /// <summary>
        /// Removes a connection from observers without synchronizing changes.
        /// </summary>
        /// <param name="connection"></param>
        private void RemoveFromObserversWithoutSynchronization(NetworkConnection connection)
        {
            var cacheIndex = 0;

            foreach (var nob in Spawned.Values)
                if (nob.RemoveObserver(connection))
                    CacheObserverChange(nob, ref cacheIndex);

            //Invoke despawn callbacks on nobs.
            for (var i = 0; i < cacheIndex; i++)
                _observerChangeObjectsCache[i].InvokeOnServerDespawn(connection);
        }

        /// <summary>
        /// Rebuilds observers on objects.
        /// </summary>
        /// <param name="connection"></param>
        public void RebuildObservers(NetworkObject[] nobs)
        {
            var count = nobs.Length;
            for (var i = 0; i < count; i++)
                RebuildObservers(nobs[i]);
        }

        /// <summary>
        /// Rebuilds observers on objects.
        /// </summary>
        /// <param name="connection"></param>
        public void RebuildObservers(ListCache<NetworkObject> nobs)
        {
            var count = nobs.Written;
            var collection = nobs.Collection;
            for (var i = 0; i < count; i++)
                RebuildObservers(collection[i]);
        }

        /// <summary>
        /// Rebuilds observers on all objects for a connections.
        /// </summary>
        /// <param name="connection"></param>
        public void RebuildObservers(ListCache<NetworkConnection> connections)
        {
            var count = connections.Written;
            var collection = connections.Collection;
            for (var i = 0; i < count; i++)
                RebuildObservers(collection[i]);
        }

        /// <summary>
        /// Rebuilds observers on all objects for a connections.
        /// </summary>
        /// <param name="connection"></param>
        public void RebuildObservers(NetworkConnection[] connections)
        {
            var count = connections.Length;
            for (var i = 0; i < count; i++)
                RebuildObservers(connections[i]);
        }

        /// <summary>
        /// Rebuilds observers on all objects for a connections.
        /// </summary>
        /// <param name="connection"></param>
        public void RebuildObservers(List<NetworkConnection> connections)
        {
            var count = connections.Count;
            for (var i = 0; i < count; i++)
                RebuildObservers(connections[i]);
        }

        /// <summary>
        /// Rebuilds observers on all objects for a connection.
        /// </summary>
        /// <param name="connection"></param>
        public void RebuildObservers(NetworkConnection connection)
        {
            var everyoneWriter = WriterPool.GetWriter();
            var ownerWriter = WriterPool.GetWriter();

            var observerCacheIndex = 0;
            using (var largeWriter = WriterPool.GetWriter())
            {
                observerCacheIndex = 0;
                foreach (var nob in Spawned.Values)
                {
                    //If observer state changed then write changes.
                    var osc = nob.RebuildObservers(connection, false);
                    if (osc == ObserverStateChange.Added)
                    {
                        everyoneWriter.Reset();
                        ownerWriter.Reset();
                        WriteSpawn(nob, connection, ref everyoneWriter, ref ownerWriter);
                        CacheObserverChange(nob, ref observerCacheIndex);
                    }
                    else if (osc == ObserverStateChange.Removed)
                    {
                        everyoneWriter.Reset();
                        WriteDespawn(nob, ref everyoneWriter);
                    }
                    else
                    {
                        continue;
                    }

                    /* Only use ownerWriter if an add, and if owner. Owner //cleanup see if rebuild timed and this can be joined or reuse methods.
                     * doesn't matter if not being added because no owner specific
                     * information would be included. */
                    var writerToUse = osc == ObserverStateChange.Added && nob.Owner == connection
                        ? ownerWriter
                        : everyoneWriter;

                    largeWriter.WriteArraySegment(writerToUse.GetArraySegment());
                }

                if (largeWriter.Length > 0)
                    NetworkManager.TransportManager.SendToClient(
                        (byte) Channel.Reliable,
                        largeWriter.GetArraySegment(), connection);
            }

            //Dispose of writers created in this method.
            everyoneWriter.Dispose();
            ownerWriter.Dispose();

            //Invoke spawn callbacks on nobs.
            for (var i = 0; i < observerCacheIndex; i++)
                _observerChangeObjectsCache[i].InvokePostOnServerStart(connection);
        }

        /// <summary>
        /// Rebuilds observers for cached connections for a NetworkObject.
        /// </summary>
        private void RebuildObservers(NetworkObject networkObject, ListCache<NetworkConnection> cache)
        {
            var everyoneWriter = WriterPool.GetWriter();
            var ownerWriter = WriterPool.GetWriter();

            var written = cache.Written;
            for (var i = 0; i < written; i++)
            {
                var conn = cache.Collection[i];

                everyoneWriter.Reset();
                ownerWriter.Reset();
                //If observer state changed then write changes.
                var osc = networkObject.RebuildObservers(conn, false);
                if (osc == ObserverStateChange.Added)
                    WriteSpawn(networkObject, conn, ref everyoneWriter, ref ownerWriter);
                else if (osc == ObserverStateChange.Removed)
                    WriteDespawn(networkObject, ref everyoneWriter);
                else
                    continue;

                /* Only use ownerWriter if an add, and if owner. Owner
                 * doesn't matter if not being added because no owner specific
                 * information would be included. */
                var writerToUse = osc == ObserverStateChange.Added && networkObject.Owner == conn
                    ? ownerWriter
                    : everyoneWriter;

                if (writerToUse.Length > 0)
                {
                    NetworkManager.TransportManager.SendToClient(
                        (byte) Channel.Reliable,
                        writerToUse.GetArraySegment(), conn);

                    //If a spawn is being sent.
                    if (osc == ObserverStateChange.Added)
                        networkObject.InvokePostOnServerStart(conn);
                }
            }

            //Dispose of writers created in this method.
            everyoneWriter.Dispose();
            ownerWriter.Dispose();
        }


        /// <summary>
        /// Rebuilds observers for all connections for a NetworkObject.
        /// </summary>
        /// <param name="nob">NetworkObject to rebuild on.</param>
        internal void RebuildObservers(NetworkObject nob)
        {
            var cache = ListCaches.NetworkConnectionCache;
            cache.Reset();
            foreach (var item in NetworkManager.ServerManager.Clients.Values)
                cache.AddValue(item);

            RebuildObservers(nob, cache);
        }

        /// <summary>
        /// Rebuilds observers for a connection on NetworkObject.
        /// </summary>
        /// <param name="nob">NetworkObject to rebuild on.</param>
        /// <param name="conn">Connection to rebuild for.</param>
        internal void RebuildObservers(NetworkObject nob, NetworkConnection conn)
        {
            var cache = ListCaches.NetworkConnectionCache;
            cache.Reset();
            cache.AddValue(conn);

            RebuildObservers(nob, cache);
        }

        /// <summary>
        /// Rebuilds observers for connections on NetworkObject.
        /// </summary>
        /// <param name="networkObject">NetworkObject to rebuild on.</param>
        /// <param name="connections">Connections to rebuild for.
        public void RebuildObservers(NetworkObject networkObject, NetworkConnection[] connections)
        {
            var cache = ListCaches.NetworkConnectionCache;
            cache.Reset();
            cache.AddValues(connections);
            RebuildObservers(networkObject, cache);
        }

        /// <summary>
        /// Rebuilds observers for connections on NetworkObject.
        /// </summary>
        /// <param name="networkObject">NetworkObject to rebuild on.</param>
        /// <param name="connections">Connections to rebuild for.
        public void RebuildObservers(NetworkObject networkObject, List<NetworkConnection> connections)
        {
            var cache = ListCaches.NetworkConnectionCache;
            cache.Reset();
            cache.AddValues(connections);
            RebuildObservers(networkObject, cache);
        }
    }
}