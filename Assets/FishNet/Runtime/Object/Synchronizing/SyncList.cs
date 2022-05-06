﻿using FishNet.Documenting;
using FishNet.Managing.Logging;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FishNet.Object.Synchronizing
{
    public class SyncList<T> : SyncBase, IList<T>, IReadOnlyList<T>
    {
        #region Types.

        /// <summary>
        /// Information needed to invoke a callback.
        /// </summary>
        private struct CachedOnChange
        {
            internal readonly SyncListOperation Operation;
            internal readonly int Index;
            internal readonly T Previous;
            internal readonly T Next;

            public CachedOnChange(SyncListOperation operation, int index, T previous, T next)
            {
                Operation = operation;
                Index = index;
                Previous = previous;
                Next = next;
            }
        }

        /// <summary>
        /// Information about how the collection has changed.
        /// </summary>
        private struct ChangeData
        {
            internal readonly SyncListOperation Operation;
            internal readonly int Index;
            internal readonly T Item;

            public ChangeData(SyncListOperation operation, int index, T item)
            {
                Operation = operation;
                Index = index;
                Item = item;
            }
        }

        /// <summary>
        /// Custom enumerator to prevent garbage collection.
        /// </summary>
        [APIExclude]
        public struct Enumerator : IEnumerator<T>
        {
            public T Current { get; private set; }
            private readonly SyncList<T> _list;
            private int _index;

            public Enumerator(SyncList<T> list)
            {
                _list = list;
                _index = -1;
                Current = default;
            }

            public bool MoveNext()
            {
                if (++_index >= _list.Count)
                    return false;
                Current = _list[_index];
                return true;
            }

            public void Reset()
            {
                _index = -1;
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }

        #endregion

        #region Public.

        /// <summary>
        /// Implementation from List<T>. Not used.
        /// </summary>
        [APIExclude]
        public bool IsReadOnly => false;

        /// <summary>
        /// Delegate signature for when SyncList changes.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="index"></param>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        [APIExclude]
        public delegate void SyncListChanged(SyncListOperation op, int index, T oldItem, T newItem, bool asServer);

        /// <summary>
        /// Called when the SyncList changes.
        /// </summary>
        public event SyncListChanged OnChange;

        /// <summary>
        /// Collection of objects.
        /// </summary>
        public readonly IList<T> Collection;

        /// <summary>
        /// Copy of objects on client portion when acting as a host.
        /// </summary>
        public readonly IList<T> ClientHostCollection = new List<T>();

        /// <summary>
        /// Number of objects in the collection.
        /// </summary>
        public int Count => Collection.Count;

        #endregion

        #region Private.

        /// <summary>
        /// Values upon initialization.
        /// </summary>
        private IList<T> _initialValues = new List<T>();

        /// <summary>
        /// Comparer to see if entries change when calling public methods.
        /// </summary>
        private readonly IEqualityComparer<T> _comparer;

        /// <summary>
        /// Changed data which will be sent next tick.
        /// </summary>
        private readonly List<ChangeData> _changed = new();

        /// <summary>
        /// Server OnChange events waiting for start callbacks.
        /// </summary>
        private readonly List<CachedOnChange> _serverOnChanges = new();

        /// <summary>
        /// Client OnChange events waiting for start callbacks.
        /// </summary>
        private readonly List<CachedOnChange> _clientOnChanges = new();

        /// <summary>
        /// True if values have changed since initialization.
        /// The only reasonable way to reset this during a Reset call is by duplicating the original list and setting all values to it on reset.
        /// </summary>
        private bool _valuesChanged;

        #endregion

        [APIExclude]
        public SyncList() : this(new List<T>(), EqualityComparer<T>.Default)
        {
        }

        [APIExclude]
        public SyncList(IEqualityComparer<T> comparer) : this(new List<T>(),
            comparer == null ? EqualityComparer<T>.Default : comparer)
        {
        }

        [APIExclude]
        public SyncList(IList<T> collection, IEqualityComparer<T> comparer = null)
        {
            _comparer = comparer == null ? EqualityComparer<T>.Default : comparer;
            Collection = collection;
            //Add each in collection to clienthostcollection.
            foreach (var item in collection)
                ClientHostCollection.Add(item);
        }

        /// <summary>
        /// Called when the SyncType has been registered, but not yet initialized over the network.
        /// </summary>
        protected override void Registered()
        {
            base.Registered();
            foreach (var item in Collection)
                _initialValues.Add(item);
        }

        /// <summary>
        /// Gets the collection being used within this SyncList.
        /// </summary>
        /// <param name="asServer">True if returning the server value, false if client value. The values will only differ when running as host. While asServer is true the most current values on server will be returned, and while false the latest values received by client will be returned.</param>
        /// <returns></returns>
        public List<T> GetCollection(bool asServer)
        {
            var asClientAndHost = !asServer && NetworkManager.IsServer;
            var collection = asClientAndHost ? ClientHostCollection : Collection;
            return collection as List<T>;
        }

        /// <summary>
        /// Adds an operation and invokes locally.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        /// <param name="prev"></param>
        /// <param name="next"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddOperation(SyncListOperation operation, int index, T prev, T next)
        {
            /* Only check this if NetworkManager is set.
             * It may not be set if results are being populated
             * before initialization. */
            if (NetworkManager == null)
                return;

            if (Settings.WritePermission == WritePermission.ServerOnly && !NetworkManager.IsServer)
            {
                if (NetworkManager.CanLog(LoggingType.Warning))
                    Debug.LogWarning($"Cannot complete operation {operation} as server when server is not active.");
                return;
            }

            /* Set as changed even if cannot dirty.
            * Dirty is only set when there are observers,
            * but even if there are not observers
            * values must be marked as changed so when
            * there are observers, new values are sent. */
            _valuesChanged = true;

            /* If unable to dirty then do not add to changed.
             * A dirty may fail if the server is not started
             * or if there's no observers. Changed doesn't need
             * to be populated in this situations because clients
             * will get the full collection on spawn. If we
             * were to also add to changed clients would get the full
             * collection as well the changed, which would double results. */
            if (base.Dirty())
            {
                var change = new ChangeData(operation, index, next);
                _changed.Add(change);
            }

            var asServer = true;
            InvokeOnChange(operation, index, prev, next, asServer);
        }

        /// <summary>
        /// Called after OnStartXXXX has occurred.
        /// </summary>
        /// <param name="asServer">True if OnStartServer was called, false if OnStartClient.</param>
        protected internal override void OnStartCallback(bool asServer)
        {
            base.OnStartCallback(asServer);
            var collection = asServer ? _serverOnChanges : _clientOnChanges;

            if (OnChange != null)
                foreach (var item in collection)
                    OnChange.Invoke(item.Operation, item.Index, item.Previous, item.Next, asServer);

            collection.Clear();
        }

        /// <summary>
        /// Writes all changed values.
        /// </summary>
        /// <param name="writer"></param>
        ///<param name="resetSyncTick">True to set the next time data may sync.</param>
        public override void WriteDelta(PooledWriter writer, bool resetSyncTick = true)
        {
            base.WriteDelta(writer, resetSyncTick);
            writer.WriteUInt32((uint) _changed.Count);

            for (var i = 0; i < _changed.Count; i++)
            {
                var change = _changed[i];
                writer.WriteByte((byte) change.Operation);

                //Clear does not need to write anymore data so it is not included in checks.
                if (change.Operation == SyncListOperation.Add)
                {
                    writer.Write(change.Item);
                }
                else if (change.Operation == SyncListOperation.RemoveAt)
                {
                    writer.WriteUInt32((uint) change.Index);
                }
                else if (change.Operation == SyncListOperation.Insert || change.Operation == SyncListOperation.Set)
                {
                    writer.WriteUInt32((uint) change.Index);
                    writer.Write(change.Item);
                }
            }

            _changed.Clear();
        }

        /// <summary>
        /// Writes all values if not initial values.
        /// </summary>
        /// <param name="writer"></param>
        public override void WriteFull(PooledWriter writer)
        {
            if (!_valuesChanged)
                return;

            base.WriteHeader(writer, false);
            writer.WriteUInt32((uint) Collection.Count);
            for (var i = 0; i < Collection.Count; i++)
            {
                writer.WriteByte((byte) SyncListOperation.Add);
                writer.Write(Collection[i]);
            }
        }

        /// <summary>
        /// Sets current values.
        /// </summary>
        /// <param name="reader"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [APIExclude]
        public override void Read(PooledReader reader)
        {
            var asServer = false;
            /* When !asServer don't make changes if server is running.
            * This is because changes would have already been made on
            * the server side and doing so again would result in duplicates
            * and potentially overwrite data not yet sent. */
            var asClientAndHost = !asServer && NetworkManager.IsServer;
            var collection = asClientAndHost ? ClientHostCollection : Collection;

            var changes = (int) reader.ReadUInt32();
            for (var i = 0; i < changes; i++)
            {
                var operation = (SyncListOperation) reader.ReadByte();
                var index = -1;
                T prev = default;
                T next = default;

                //Add.
                if (operation == SyncListOperation.Add)
                {
                    next = reader.Read<T>();
                    index = collection.Count;
                    collection.Add(next);
                }
                //Clear.
                else if (operation == SyncListOperation.Clear)
                {
                    collection.Clear();
                }
                //Insert.
                else if (operation == SyncListOperation.Insert)
                {
                    index = (int) reader.ReadUInt32();
                    next = reader.Read<T>();
                    collection.Insert(index, next);
                }
                //RemoveAt.
                else if (operation == SyncListOperation.RemoveAt)
                {
                    index = (int) reader.ReadUInt32();
                    prev = collection[index];
                    collection.RemoveAt(index);
                }
                //Set
                else if (operation == SyncListOperation.Set)
                {
                    index = (int) reader.ReadUInt32();
                    next = reader.Read<T>();
                    prev = collection[index];
                    collection[index] = next;
                }


                InvokeOnChange(operation, index, prev, next, false);
            }

            //If changes were made invoke complete after all have been read.
            if (changes > 0)
                InvokeOnChange(SyncListOperation.Complete, -1, default, default, false);
        }

        /// <summary>
        /// Invokes OnChanged callback.
        /// </summary>
        private void InvokeOnChange(SyncListOperation operation, int index, T prev, T next, bool asServer)
        {
            if (OnChange != null)
            {
                if (asServer)
                {
                    if (NetworkBehaviour.OnStartServerCalled)
                        OnChange.Invoke(operation, index, prev, next, asServer);
                    else
                        _serverOnChanges.Add(new CachedOnChange(operation, index, prev, next));
                }
                else
                {
                    if (NetworkBehaviour.OnStartClientCalled)
                        OnChange.Invoke(operation, index, prev, next, asServer);
                    else
                        _clientOnChanges.Add(new CachedOnChange(operation, index, prev, next));
                }
            }
        }

        /// <summary>
        /// Resets to initialized values.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _changed.Clear();
            ClientHostCollection.Clear();
            Collection.Clear();

            foreach (var item in _initialValues)
            {
                Collection.Add(item);
                ClientHostCollection.Add(item);
            }
        }


        /// <summary>
        /// Adds value.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            Add(item, true);
        }

        private void Add(T item, bool asServer)
        {
            Collection.Add(item);
            if (asServer)
            {
                if (NetworkManager == null)
                    ClientHostCollection.Add(item);
                AddOperation(SyncListOperation.Add, Collection.Count - 1, default, item);
            }
        }

        /// <summary>
        /// Adds a range of values.
        /// </summary>
        /// <param name="range"></param>
        public void AddRange(IEnumerable<T> range)
        {
            foreach (var entry in range)
                Add(entry, true);
        }

        /// <summary>
        /// Clears all values.
        /// </summary>
        public void Clear()
        {
            Clear(true);
        }

        private void Clear(bool asServer)
        {
            Collection.Clear();
            if (asServer)
            {
                if (NetworkManager == null)
                    ClientHostCollection.Clear();
                AddOperation(SyncListOperation.Clear, -1, default, default);
            }
        }

        /// <summary>
        /// Returns if value exist.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        /// <summary>
        /// Copies values to an array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(T[] array, int index)
        {
            Collection.CopyTo(array, index);
        }

        /// <summary>
        /// Gets the index of value.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            for (var i = 0; i < Collection.Count; ++i)
                if (_comparer.Equals(item, Collection[i]))
                    return i;
            return -1;
        }

        /// <summary>
        /// Finds index using match.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindIndex(Predicate<T> match)
        {
            for (var i = 0; i < Collection.Count; ++i)
                if (match(Collection[i]))
                    return i;
            return -1;
        }

        /// <summary>
        /// Finds value using match.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public T Find(Predicate<T> match)
        {
            var i = FindIndex(match);
            return i != -1 ? Collection[i] : default;
        }

        /// <summary>
        /// Finds all values using match.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<T> FindAll(Predicate<T> match)
        {
            var results = new List<T>();
            for (var i = 0; i < Collection.Count; ++i)
                if (match(Collection[i]))
                    results.Add(Collection[i]);
            return results;
        }

        /// <summary>
        /// Inserts value at index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            Insert(index, item, true);
        }

        private void Insert(int index, T item, bool asServer)
        {
            Collection.Insert(index, item);
            if (asServer)
            {
                if (NetworkManager == null)
                    ClientHostCollection.Insert(index, item);
                AddOperation(SyncListOperation.Insert, index, default, item);
            }
        }

        /// <summary>
        /// Inserts a range of values.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="range"></param>
        public void InsertRange(int index, IEnumerable<T> range)
        {
            foreach (var entry in range)
            {
                Insert(index, entry);
                index++;
            }
        }

        /// <summary>
        /// Removes a value.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            var index = IndexOf(item);
            var result = index >= 0;
            if (result)
                RemoveAt(index);

            return result;
        }

        /// <summary>
        /// Removes value at index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="asServer"></param>
        public void RemoveAt(int index)
        {
            RemoveAt(index, true);
        }

        private void RemoveAt(int index, bool asServer)
        {
            var oldItem = Collection[index];
            Collection.RemoveAt(index);
            if (asServer)
            {
                if (NetworkManager == null)
                    ClientHostCollection.RemoveAt(index);
                AddOperation(SyncListOperation.RemoveAt, index, oldItem, default);
            }
        }

        /// <summary>
        /// Removes all values within the collection.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int RemoveAll(Predicate<T> match)
        {
            var toRemove = new List<T>();
            for (var i = 0; i < Collection.Count; ++i)
                if (match(Collection[i]))
                    toRemove.Add(Collection[i]);

            foreach (var entry in toRemove) Remove(entry);

            return toRemove.Count;
        }

        /// <summary>
        /// Gets or sets value at an index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T this[int i]
        {
            get => Collection[i];
            set => Set(i, value, true, true);
        }

        /// <summary>
        /// Marks an index as dirty.
        /// While using this operation previous value will be the same as next.
        /// </summary>
        /// <param name="index"></param>
        public void Dirty(int index)
        {
            var asServer = true;

            var value = Collection[index];
            if (asServer)
                AddOperation(SyncListOperation.Set, index, value, value);
        }

        /// <summary>
        /// Sets value at index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Set(int index, T value, bool force = true)
        {
            Set(index, value, true, force);
        }

        private void Set(int index, T value, bool asServer, bool force)
        {
            var sameValue = !force && !_comparer.Equals(Collection[index], value);
            if (!sameValue)
            {
                var prev = Collection[index];
                Collection[index] = value;
                if (asServer)
                {
                    if (NetworkManager == null)
                        ClientHostCollection[index] = value;
                    AddOperation(SyncListOperation.Set, index, prev, value);
                }
            }
        }


        /// <summary>
        /// Returns Enumerator for collection.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator()
        {
            return new(this);
        }

        [APIExclude]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        [APIExclude]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}