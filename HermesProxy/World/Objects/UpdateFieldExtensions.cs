using Framework.Util;
using HermesProxy.Enums;
using HermesProxy.World.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects;

public static class UpdateFieldExtensions
{
    private static TypeCode GetTypeCodeOfReturnValue<TK>()
    {
        var type = typeof(TK);
        var typeCode = Type.GetTypeCode(type);
        switch (typeCode)
        {
            case TypeCode.UInt32:
            case TypeCode.Int32:
            case TypeCode.Single:
            case TypeCode.Double:
                return typeCode;
            default:
            {
                typeCode = Type.GetTypeCode(Nullable.GetUnderlyingType(type));
                switch (typeCode)
                {
                    case TypeCode.UInt32:
                    case TypeCode.Int32:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return typeCode;
                    default:
                        break;
                }
                break;
            }
        }
        throw new ArgumentException($"Type must be one of int, uint, float or its nullable counterpart but was {type.Name}");
    }

    /// <summary>
    /// Grabs N (consecutive) values from a dictionary of UpdateFields
    /// </summary>
    /// <typeparam name="T">The type of UpdateField (ObjectField, UnitField, ...)</typeparam>
    /// <typeparam name="TK">The type of the value (int, uint or float and their nullable counterparts)</typeparam>
    /// <param name="dict">The dictionary</param>
    /// <param name="firstUpdateField">The first update field of the sequence</param>
    /// <param name="count">Number of values to retrieve</param>
    /// <returns></returns>
    public static TK[] GetArray<T, TK>(this Dictionary<int, UpdateField> dict, T firstUpdateField, int count) where T: System.Enum // C# 7.3
    {
        return GetArray<TK>(dict, LegacyVersion.GetUpdateField(firstUpdateField), count);
    }
    public static TK[] GetArray<TK>(this Dictionary<int, UpdateField> dict, int firstUpdateField, int count)
    {
        var result = new TK[count];
        var type = GetTypeCodeOfReturnValue<TK>();
        for (var i = 0; i < count; i++)
        {
            UpdateField uf;
            if (dict != null && dict.TryGetValue(firstUpdateField + i, out uf))
            {
                switch (type)
                {
                    case TypeCode.UInt32:
                        result[i] = (TK)(object)uf.UInt32Value;
                        break;
                    case TypeCode.Int32:
                        result[i] = (TK)(object)(int)uf.UInt32Value;
                        break;
                    case TypeCode.Single:
                        result[i] = (TK)(object)uf.FloatValue;
                        break;
                    case TypeCode.Double:
                        result[i] = (TK)(object)(double)uf.FloatValue;
                        break;
                    default:
                        break;
                }
            }
        }

        return result;
    }
    public static WowGuid64 GetGuidValue64(this Dictionary<int, UpdateField> UpdateFields, int field)
    {
        var parts = UpdateFields.GetArray<uint>(field, 2);
        return new WowGuid64(MathFunctions.MakePair64(parts[0], parts[1]));
    }

    public static WowGuid128 GetGuidValue128(this Dictionary<int, UpdateField> UpdateFields, int field)
    {
        var parts = UpdateFields.GetArray<uint>(field, 4);
        return new WowGuid128(MathFunctions.MakePair64(parts[2], parts[3]), MathFunctions.MakePair64(parts[0], parts[1]));
    }

    // Keep for backward compatibility - returns WowGuid64 since the only caller needs that
    public static WowGuid64 GetGuidValue(this Dictionary<int, UpdateField> UpdateFields, int field)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V6_0_2_19033))
            return GetGuidValue64(UpdateFields, field);
        else
            return GetGuidValue128(UpdateFields, field).To64();
    }
}
