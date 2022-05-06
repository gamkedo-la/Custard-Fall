// Decompiled with JetBrains decompiler
// Type: FishNet.Runtime.ScenedReadersAndWriters
// Assembly: FishNet.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 398967D9-11C0-455C-B750-DCE87EFCCBEC
// Assembly location: D:DevelopmentPersonalFishNetsFishNet - DeveloperLibraryScriptAssembliesFishNet.Runtime.dll

using FishNet.Documenting;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Serializing;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FishNet.Runtime
{
    [APIExclude]
    [StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
    public static class ScenedReadersAndWriters
    {
        public static void Write___FishNetu002EManagingu002EScenedu002EBroadcastu002ELoadScenesBroadcast(
            this Writer writer,
            LoadScenesBroadcast value)
        {
            Write___FishNetu002EManagingu002EScenedu002EDatau002ELoadQueueData(writer, value.QueueData);
        }

        public static void Write___FishNetu002EManagingu002EScenedu002EDatau002ELoadQueueData(
            this Writer writer,
            LoadQueueData value)
        {
            if (value == null)
            {
                writer.WriteBoolean(true);
            }
            else
            {
                writer.WriteBoolean(false);
                Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneLoadData(writer, value.SceneLoadData);
                Write___Systemu002EStringu005Bu005D(writer, value.GlobalScenes);
            }
        }

        public static void Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneLoadData(
            this Writer writer,
            SceneLoadData value)
        {
            if (value == null)
            {
                writer.WriteBoolean(true);
            }
            else
            {
                writer.WriteBoolean(false);
                Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupDatau005Bu005D(writer,
                    value.SceneLookupDatas);
                Write___FishNetu002EObjectu002ENetworkObjectu005Bu005D(writer, value.MovedNetworkObjects);
                writer.WriteByte((byte) value.ReplaceScenes);
                Write___FishNetu002EManagingu002EScenedu002EDatau002ELoadParams(writer, value.Params);
                Write___FishNetu002EManagingu002EScenedu002EDatau002ELoadOptions(writer, value.Options);
            }
        }

        public static void Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupDatau005Bu005D(
            this Writer writer,
            SceneLookupData[] value)
        {
            if (value == null)
            {
                var num = -1;
                writer.WritePackedWhole((ulong) (uint) num);
            }
            else
            {
                var length = value.Length;
                writer.WritePackedWhole((ulong) (uint) length);
                for (var index = 0; index < length; ++index)
                    Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupData(writer, value[index]);
            }
        }

        public static void Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupData(
            this Writer writer,
            SceneLookupData value)
        {
            if ((object) value == null)
            {
                writer.WriteBoolean(true);
            }
            else
            {
                writer.WriteBoolean(false);
                writer.WriteInt32(value.Handle);
                writer.WriteString(value.Name);
            }
        }

        public static void Write___FishNetu002EObjectu002ENetworkObjectu005Bu005D(
            this Writer writer,
            NetworkObject[] value)
        {
            if (value == null)
            {
                var num = -1;
                writer.WritePackedWhole((ulong) (uint) num);
            }
            else
            {
                var length = value.Length;
                writer.WritePackedWhole((ulong) (uint) length);
                for (var index = 0; index < length; ++index)
                    writer.WriteNetworkObject(value[index]);
            }
        }

        public static void Write___EmptyStartScenesBroadcast(this Writer write, EmptyStartScenesBroadcast value)
        {
        }

        public static EmptyStartScenesBroadcast Read___EmptyStartScenesBroadcast(this Reader reader)
        {
            return new EmptyStartScenesBroadcast();
        }


        public static void Write___FishNetu002EManagingu002EScenedu002EDatau002ELoadParams(
            this Writer writer,
            LoadParams value)
        {
            if (value == null)
            {
                writer.WriteBoolean(true);
            }
            else
            {
                writer.WriteBoolean(false);
                writer.WriteBytesAndSize(value.ClientParams);
            }
        }

        public static void Write___FishNetu002EManagingu002EScenedu002EDatau002ELoadOptions(
            this Writer writer,
            LoadOptions value)
        {
            if (value == null)
                writer.WriteBoolean(true);
            else
                writer.WriteBoolean(false);
        }

        public static void Write___Systemu002EStringu005Bu005D(this Writer writer, string[] value)
        {
            if (value == null)
            {
                var num = -1;
                writer.WritePackedWhole((ulong) (uint) num);
            }
            else
            {
                var length = value.Length;
                writer.WritePackedWhole((ulong) (uint) length);
                for (var index = 0; index < length; ++index)
                    writer.WriteString(value[index]);
            }
        }

        public static LoadScenesBroadcast Read___FishNetu002EManagingu002EScenedu002EBroadcastu002ELoadScenesBroadcast(
            this Reader reader)
        {
            return new LoadScenesBroadcast()
            {
                QueueData = Read___FishNetu002EManagingu002EScenedu002EDatau002ELoadQueueData(reader)
            };
        }

        public static LoadQueueData Read___FishNetu002EManagingu002EScenedu002EDatau002ELoadQueueData(
            this Reader reader)
        {
            if (reader.ReadBoolean())
                return (LoadQueueData) null;
            return new LoadQueueData()
            {
                SceneLoadData = Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneLoadData(reader),
                GlobalScenes = Read___Systemu002EStringu005Bu005D(reader)
            };
        }

        public static SceneLoadData Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneLoadData(
            this Reader reader)
        {
            if (reader.ReadBoolean())
                return (SceneLoadData) null;
            return new SceneLoadData()
            {
                SceneLookupDatas =
                    Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupDatau005Bu005D(reader),
                MovedNetworkObjects = Read___FishNetu002EObjectu002ENetworkObjectu005Bu005D(reader),
                ReplaceScenes = (ReplaceOption) reader.ReadByte(),
                Params = Read___FishNetu002EManagingu002EScenedu002EDatau002ELoadParams(reader),
                Options = Read___FishNetu002EManagingu002EScenedu002EDatau002ELoadOptions(reader)
            };
        }

        public static SceneLookupData[] Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupDatau005Bu005D(
            this Reader reader)
        {
            var length = (int) reader.ReadPackedWhole();
            if (length == -1)
                return (SceneLookupData[]) null;
            var sceneLookupDataArray = new SceneLookupData[length];
            for (var index = 0; index < length; ++index)
                sceneLookupDataArray[index] =
                    Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupData(reader);
            return sceneLookupDataArray;
        }

        public static SceneLookupData Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupData(
            this Reader reader)
        {
            if (reader.ReadBoolean())
                return (SceneLookupData) null;
            return new SceneLookupData()
            {
                Handle = reader.ReadInt32(),
                Name = reader.ReadString()
            };
        }

        public static NetworkObject[] Read___FishNetu002EObjectu002ENetworkObjectu005Bu005D(
            this Reader reader)
        {
            var length = (int) reader.ReadPackedWhole();
            if (length == -1)
                return (NetworkObject[]) null;
            var networkObjectArray = new NetworkObject[length];
            for (var index = 0; index < length; ++index)
                networkObjectArray[index] = reader.ReadNetworkObject();
            return networkObjectArray;
        }

        public static LoadParams Read___FishNetu002EManagingu002EScenedu002EDatau002ELoadParams(
            this Reader reader)
        {
            if (reader.ReadBoolean())
                return (LoadParams) null;
            return new LoadParams()
            {
                ClientParams = reader.ReadBytesAndSizeAllocated()
            };
        }

        public static LoadOptions Read___FishNetu002EManagingu002EScenedu002EDatau002ELoadOptions(
            this Reader reader)
        {
            return reader.ReadBoolean() ? (LoadOptions) null : new LoadOptions();
        }

        public static string[] Read___Systemu002EStringu005Bu005D(this Reader reader)
        {
            var length = (int) reader.ReadPackedWhole();
            if (length == -1)
                return (string[]) null;
            var strArray = new string[length];
            for (var index = 0; index < length; ++index)
                strArray[index] = reader.ReadString();
            return strArray;
        }

        public static void Write___FishNetu002EManagingu002EScenedu002EBroadcastu002EUnloadScenesBroadcast(
            this Writer writer,
            UnloadScenesBroadcast value)
        {
            Write___FishNetu002EManagingu002EScenedu002EDatau002EUnloadQueueData(writer, value.QueueData);
        }

        public static void Write___FishNetu002EManagingu002EScenedu002EDatau002EUnloadQueueData(
            this Writer writer,
            UnloadQueueData value)
        {
            if (value == null)
            {
                writer.WriteBoolean(true);
            }
            else
            {
                writer.WriteBoolean(false);
                Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneUnloadData(writer, value.SceneUnloadData);
                Write___Systemu002EStringu005Bu005D(writer, value.GlobalScenes);
            }
        }

        public static void Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneUnloadData(
            this Writer writer,
            SceneUnloadData value)
        {
            if (value == null)
            {
                writer.WriteBoolean(true);
            }
            else
            {
                writer.WriteBoolean(false);
                Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupDatau005Bu005D(writer,
                    value.SceneLookupDatas);
                Write___FishNetu002EManagingu002EScenedu002EDatau002EUnloadParams(writer, value.Params);
                Write___FishNetu002EManagingu002EScenedu002EDatau002EUnloadOptions(writer, value.Options);
            }
        }

        public static void Write___FishNetu002EManagingu002EScenedu002EDatau002EUnloadParams(
            this Writer writer,
            UnloadParams value)
        {
            if (value == null)
            {
                writer.WriteBoolean(true);
            }
            else
            {
                writer.WriteBoolean(false);
                writer.WriteBytesAndSize(value.ClientParams);
            }
        }

        public static void Write___FishNetu002EManagingu002EScenedu002EDatau002EUnloadOptions(
            this Writer writer,
            UnloadOptions value)
        {
            if (value == null)
                writer.WriteBoolean(true);
            else
                writer.WriteBoolean(false);
        }

        public static UnloadScenesBroadcast
            Read___FishNetu002EManagingu002EScenedu002EBroadcastu002EUnloadScenesBroadcast(
                this Reader reader)
        {
            return new UnloadScenesBroadcast()
            {
                QueueData = Read___FishNetu002EManagingu002EScenedu002EDatau002EUnloadQueueData(reader)
            };
        }

        public static UnloadQueueData Read___FishNetu002EManagingu002EScenedu002EDatau002EUnloadQueueData(
            this Reader reader)
        {
            if (reader.ReadBoolean())
                return (UnloadQueueData) null;
            return new UnloadQueueData()
            {
                SceneUnloadData = Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneUnloadData(reader),
                GlobalScenes = Read___Systemu002EStringu005Bu005D(reader)
            };
        }

        public static SceneUnloadData Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneUnloadData(
            this Reader reader)
        {
            if (reader.ReadBoolean())
                return (SceneUnloadData) null;
            return new SceneUnloadData()
            {
                SceneLookupDatas =
                    Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupDatau005Bu005D(reader),
                Params = Read___FishNetu002EManagingu002EScenedu002EDatau002EUnloadParams(reader),
                Options = Read___FishNetu002EManagingu002EScenedu002EDatau002EUnloadOptions(reader)
            };
        }

        public static UnloadParams Read___FishNetu002EManagingu002EScenedu002EDatau002EUnloadParams(
            this Reader reader)
        {
            if (reader.ReadBoolean())
                return (UnloadParams) null;
            return new UnloadParams()
            {
                ClientParams = reader.ReadBytesAndSizeAllocated()
            };
        }

        public static UnloadOptions Read___FishNetu002EManagingu002EScenedu002EDatau002EUnloadOptions(
            this Reader reader)
        {
            return reader.ReadBoolean() ? (UnloadOptions) null : new UnloadOptions();
        }

        public static void Write___FishNetu002EManagingu002EScenedu002EBroadcastu002EClientScenesLoadedBroadcast(
            this Writer writer,
            ClientScenesLoadedBroadcast value)
        {
            Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupDatau005Bu005D(writer,
                value.SceneLookupDatas);
        }

        public static ClientScenesLoadedBroadcast
            Read___FishNetu002EManagingu002EScenedu002EBroadcastu002EClientScenesLoadedBroadcast(
                this Reader reader)
        {
            return new ClientScenesLoadedBroadcast()
            {
                SceneLookupDatas = Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupDatau005Bu005D(reader)
            };
        }

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeOnce()
        {
            GenericWriter<SceneLookupData>.Write =
                new Action<Writer, SceneLookupData>(
                    Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupData);
            GenericWriter<SceneLookupData[]>.Write =
                new Action<Writer, SceneLookupData[]>(
                    Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupDatau005Bu005D);
            GenericWriter<LoadParams>.Write =
                new Action<Writer, LoadParams>(Write___FishNetu002EManagingu002EScenedu002EDatau002ELoadParams);
            GenericWriter<LoadOptions>.Write =
                new Action<Writer, LoadOptions>(Write___FishNetu002EManagingu002EScenedu002EDatau002ELoadOptions);
            GenericWriter<SceneLoadData>.Write =
                new Action<Writer, SceneLoadData>(Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneLoadData);
            GenericWriter<LoadQueueData>.Write =
                new Action<Writer, LoadQueueData>(Write___FishNetu002EManagingu002EScenedu002EDatau002ELoadQueueData);
            GenericWriter<LoadScenesBroadcast>.Write =
                new Action<Writer, LoadScenesBroadcast>(
                    Write___FishNetu002EManagingu002EScenedu002EBroadcastu002ELoadScenesBroadcast);
            GenericWriter<UnloadParams>.Write =
                new Action<Writer, UnloadParams>(Write___FishNetu002EManagingu002EScenedu002EDatau002EUnloadParams);
            GenericWriter<UnloadOptions>.Write =
                new Action<Writer, UnloadOptions>(Write___FishNetu002EManagingu002EScenedu002EDatau002EUnloadOptions);
            GenericWriter<SceneUnloadData>.Write =
                new Action<Writer, SceneUnloadData>(
                    Write___FishNetu002EManagingu002EScenedu002EDatau002ESceneUnloadData);
            GenericWriter<UnloadQueueData>.Write =
                new Action<Writer, UnloadQueueData>(
                    Write___FishNetu002EManagingu002EScenedu002EDatau002EUnloadQueueData);
            GenericWriter<UnloadScenesBroadcast>.Write =
                new Action<Writer, UnloadScenesBroadcast>(
                    Write___FishNetu002EManagingu002EScenedu002EBroadcastu002EUnloadScenesBroadcast);
            GenericWriter<ClientScenesLoadedBroadcast>.Write =
                new Action<Writer, ClientScenesLoadedBroadcast>(
                    Write___FishNetu002EManagingu002EScenedu002EBroadcastu002EClientScenesLoadedBroadcast);
            GenericReader<SceneLookupData>.Read =
                new Func<Reader, SceneLookupData>(Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupData);
            GenericReader<SceneLookupData[]>.Read =
                new Func<Reader, SceneLookupData[]>(
                    Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneLookupDatau005Bu005D);
            GenericReader<LoadParams>.Read =
                new Func<Reader, LoadParams>(Read___FishNetu002EManagingu002EScenedu002EDatau002ELoadParams);
            GenericReader<LoadOptions>.Read =
                new Func<Reader, LoadOptions>(Read___FishNetu002EManagingu002EScenedu002EDatau002ELoadOptions);
            GenericReader<SceneLoadData>.Read =
                new Func<Reader, SceneLoadData>(Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneLoadData);
            GenericReader<LoadQueueData>.Read =
                new Func<Reader, LoadQueueData>(Read___FishNetu002EManagingu002EScenedu002EDatau002ELoadQueueData);
            GenericReader<LoadScenesBroadcast>.Read =
                new Func<Reader, LoadScenesBroadcast>(
                    Read___FishNetu002EManagingu002EScenedu002EBroadcastu002ELoadScenesBroadcast);
            GenericReader<UnloadParams>.Read =
                new Func<Reader, UnloadParams>(Read___FishNetu002EManagingu002EScenedu002EDatau002EUnloadParams);
            GenericReader<UnloadOptions>.Read =
                new Func<Reader, UnloadOptions>(Read___FishNetu002EManagingu002EScenedu002EDatau002EUnloadOptions);
            GenericReader<SceneUnloadData>.Read =
                new Func<Reader, SceneUnloadData>(Read___FishNetu002EManagingu002EScenedu002EDatau002ESceneUnloadData);
            GenericReader<UnloadQueueData>.Read =
                new Func<Reader, UnloadQueueData>(Read___FishNetu002EManagingu002EScenedu002EDatau002EUnloadQueueData);
            GenericReader<UnloadScenesBroadcast>.Read =
                new Func<Reader, UnloadScenesBroadcast>(
                    Read___FishNetu002EManagingu002EScenedu002EBroadcastu002EUnloadScenesBroadcast);
            GenericReader<ClientScenesLoadedBroadcast>.Read =
                new Func<Reader, ClientScenesLoadedBroadcast>(
                    Read___FishNetu002EManagingu002EScenedu002EBroadcastu002EClientScenesLoadedBroadcast);
        }
    }
}