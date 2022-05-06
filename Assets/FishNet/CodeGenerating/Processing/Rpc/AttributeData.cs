﻿using FishNet.CodeGenerating.Helping;
using FishNet.Object.Helping;
using MonoFN.Cecil;
using System.Collections.Generic;

namespace FishNet.CodeGenerating.Processing.Rpc
{
    internal static class AttributeDataExtensions
    {
        /// <summary>
        /// Returns RpcTypes in datas.
        /// </summary>
        public static List<RpcType> GetRpcTypes(this List<AttributeData> datas)
        {
            //RpcTypes for originalMd.
            var rpcTypes = new List<RpcType>();
            foreach (var ad in datas)
                rpcTypes.Add(ad.RpcType);

            return rpcTypes;
        }

        /// <summary>
        /// Gets CustomAttribute for rpcType
        /// </summary>
        public static CustomAttribute GetAttribute(this List<AttributeData> datas, RpcType rpcType)
        {
            for (var i = 0; i < datas.Count; i++)
                if (datas[i].RpcType == rpcType)
                    return datas[i].Attribute;

            CodegenSession.LogError($"RpcType {rpcType} not found in datas.");
            return null;
        }


        /// <summary>
        /// Returns RpcType as flag through combining datas.
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static RpcType GetCombinedRpcType(this List<AttributeData> datas)
        {
            var result = RpcType.None;
            for (var i = 0; i < datas.Count; i++)
                result |= datas[i].RpcType;

            return result;
        }
    }

    internal class AttributeData
    {
        public readonly CustomAttribute Attribute;
        public readonly RpcType RpcType;

        public AttributeData(CustomAttribute attribute, RpcType rpcType)
        {
            Attribute = attribute;
            RpcType = rpcType;
        }
    }
}