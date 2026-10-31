using Framework.IO;
using Framework.Logging;
using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects;

[StructLayout(LayoutKind.Explicit)]
public struct UpdateValues
{
    [FieldOffset(0)]
    public uint UnsignedValue;

    [FieldOffset(0)]
    public int SignedValue;

    [FieldOffset(0)]
    public float FloatValue;
}

public sealed class UpdateFieldsArray
{
    public UpdateFieldsArray(uint size)
    {
        ValuesCount = size;
        m_updateValues = new UpdateValues[size];
        m_updateMask = new UpdateMask(size);
    }
    public uint ValuesCount;
    public UpdateValues[] m_updateValues;
    public UpdateMask m_updateMask;

    public void WriteToPacket(ByteBuffer buffer)
    {
        m_updateMask.AppendToPacket(buffer);

        // Values follow the mask in ascending field order; walking set bits skips clean blocks,
        // which is most of a 4674-field ActivePlayer array on a typical Values update.
        var blocks = m_updateMask.Blocks;
        for (var block = 0; block < blocks.Length; ++block)
        {
            for (var bits = blocks[block]; bits != 0; bits &= bits - 1)
                buffer.WriteUInt32(m_updateValues[(block << 5) + BitOperations.TrailingZeroCount(bits)].UnsignedValue);
        }
    }

    // Fields are addressed by int. These took object once, which boxed every computed index (the
    // array loops of the V1_14 and V2_5 builders) at 24 B a field, and sent an enum through a
    // runtime helper at each unbox. Cast the enum at the call: it costs nothing there.
    public void SetUpdateField<T>(int index, T value, byte offset = 0) where T : new()
    {
        if (value is byte byteValue)
        {
            if (offset > 3)
            {
                Log.Print(LogType.Error, $"SetUpdateField<UInt8>: Wrong offset: {offset}");
                return;
            }

            if ((byte)(m_updateValues[index].UnsignedValue >> (offset * 8)) != byteValue)
            {
                m_updateValues[index].UnsignedValue &= ~(uint)(0xFF << (offset * 8));
                m_updateValues[index].UnsignedValue |= (uint)byteValue << (offset * 8);
                m_updateMask.SetBit(index);
            }
        }
        else if (value is ushort ushortValue)
        {
            if (offset > 1)
            {
                Log.Print(LogType.Error, $"SetUpdateField<UInt16>: Wrong offset: {offset}");
                return;
            }

            if ((ushort)(GetUpdateField<uint>(index) >> (offset * 16)) != ushortValue)
            {
                m_updateValues[index].UnsignedValue &= ~((uint)0xFFFF << (offset * 16));
                m_updateValues[index].UnsignedValue |= (uint)ushortValue << (offset * 16);
                m_updateMask.SetBit(index);
            }
        }
        else if (value is int intValue)
        {
            if (m_updateValues[index].SignedValue != intValue)
            {
                m_updateValues[index].SignedValue = intValue;
                m_updateMask.SetBit(index);
            }
        }
        else if (value is uint uintValue)
        {
            if (m_updateValues[index].UnsignedValue != uintValue)
            {
                m_updateValues[index].UnsignedValue = uintValue;
                m_updateMask.SetBit(index);
            }
        }
        else if (value is float floatValue)
        {
            if (m_updateValues[index].FloatValue != floatValue)
            {
                m_updateValues[index].FloatValue = floatValue;
                m_updateMask.SetBit(index);
            }
        }
        else if (value is ulong ulongValue)
        {
            if (GetUpdateField<ulong>(index) != ulongValue)
            {
                m_updateValues[index].UnsignedValue = MathFunctions.Pair64_LoPart(ulongValue);
                m_updateValues[index + 1].UnsignedValue = MathFunctions.Pair64_HiPart(ulongValue);
                m_updateMask.SetBit(index);
                m_updateMask.SetBit(index + 1);
            }
        }
        else if (value is WowGuid128 guid)
        {
            SetUpdateField(index, guid);
        }
        else
            throw new Exception($"Unhandled type {typeof(T).Name} in SetUpdateField!");
    }

    public T GetUpdateField<T>(int index, byte offset = 0)
    {
        return default(T) switch
        {
            byte => (T)(object)(byte)((m_updateValues[index].UnsignedValue >> (offset * 8)) & 0xFF),
            ushort => (T)(object)(ushort)((m_updateValues[index].UnsignedValue >> (offset * 16)) & 0xFFFF),
            int => (T)(object)m_updateValues[index].SignedValue,
            uint => (T)(object)m_updateValues[index].UnsignedValue,
            float => (T)(object)m_updateValues[index].FloatValue,
            ulong => (T)(object)((ulong)m_updateValues[index + 1].UnsignedValue << 32 | m_updateValues[index].UnsignedValue),
            WowGuid128 => (T)(object)GetUpdateFieldGuid(index),
            _ => throw new Exception($"{typeof(T).Name} is not implemented in GetUpdateField<T>"),
        };
    }

    public WowGuid128 GetUpdateFieldGuid(int index)
    {
        ulong low = (ulong)m_updateValues[index + 1].UnsignedValue << 32 | m_updateValues[index].UnsignedValue;
        ulong high = (ulong)m_updateValues[index + 3].UnsignedValue << 32 | m_updateValues[index + 2].UnsignedValue;
        return new WowGuid128(low, high);
    }

    public void SetUpdateField(int index, WowGuid128 guid)
    {
        // Low 8 bytes → indices [index, index+1]
        ulong low = guid.GetLowValue();
        uint loLo = MathFunctions.Pair64_LoPart(low);
        uint loHi = MathFunctions.Pair64_HiPart(low);
        if (m_updateValues[index].UnsignedValue != loLo ||
            m_updateValues[index + 1].UnsignedValue != loHi)
        {
            m_updateValues[index].UnsignedValue = loLo;
            m_updateValues[index + 1].UnsignedValue = loHi;
            m_updateMask.SetBit(index);
            m_updateMask.SetBit(index + 1);
        }

        // High 8 bytes → indices [index+2, index+3]
        ulong high = guid.GetHighValue();
        uint hiLo = MathFunctions.Pair64_LoPart(high);
        uint hiHi = MathFunctions.Pair64_HiPart(high);
        if (m_updateValues[index + 2].UnsignedValue != hiLo ||
            m_updateValues[index + 3].UnsignedValue != hiHi)
        {
            m_updateValues[index + 2].UnsignedValue = hiLo;
            m_updateValues[index + 3].UnsignedValue = hiHi;
            m_updateMask.SetBit(index + 2);
            m_updateMask.SetBit(index + 3);
        }
    }

    public bool HasFlag(int index, uint flag)
    {
        if (index >= ValuesCount)
            return false;

        return (GetUpdateField<uint>(index) & flag) != 0;
    }

    public void AddFlag(int index, uint newFlag)
    {
        var oldValue = m_updateValues[index].UnsignedValue;
        var newValue = oldValue | newFlag;

        if (oldValue != newValue)
            SetUpdateField<uint>(index, newValue);
    }

    public void RemoveFlag(int index, uint newFlag)
    {
        var oldValue = m_updateValues[index].UnsignedValue;
        var newValue = oldValue & ~newFlag;

        if (oldValue != newValue)
        {
            SetUpdateField<uint>(index, newValue);
        }
    }

    public void ApplyFlag(int index, uint flag, bool apply)
    {
        if (apply)
            AddFlag(index, flag);
        else
            RemoveFlag(index, flag);
    }
}

public sealed class DynamicUpdateFieldsArray
{
    public DynamicUpdateFieldsArray(uint size, UpdateTypeModern updateType)
    {
        m_updateType = updateType;
        m_updateMask = new UpdateMask(size);
    }
    UpdateTypeModern m_updateType;
    UpdateMask m_updateMask;
    // Every builder constructs one of these, but most updates set no dynamic field — only rent
    // the pooled buffer once one does.
    ByteBuffer? m_fieldBuffer;

    public void WriteToPacket(ByteBuffer buffer)
    {
        m_updateMask.AppendToPacket(buffer);
        if (m_fieldBuffer != null)
            buffer.WriteBytes(m_fieldBuffer.GetDataSpan());
    }

    public void SetUpdateField(int index, ReadOnlySpan<uint> values, DynamicFieldChangeType changeType)
    {
        m_updateMask.SetBit(index);
        var data = m_fieldBuffer ??= new ByteBuffer();

        // The size-changed flag and explicit count only exist on Values updates; creates always
        // carry the full array.
        var blockCount = UpdateMask.BlockCount(values.Length);
        var sizeChanged = m_updateType == UpdateTypeModern.Values && changeType == DynamicFieldChangeType.ValueAndSizeChanged;
        data.WriteUInt16((ushort)(sizeChanged ? blockCount | (int)DynamicFieldChangeType.ValueAndSizeChanged : blockCount));
        if (sizeChanged)
            data.WriteInt32(values.Length);

        // Every element is sent, so the element mask is all ones up to values.Length.
        for (var remaining = values.Length; remaining > 0; remaining -= 32)
            data.WriteUInt32(remaining >= 32 ? uint.MaxValue : (1u << remaining) - 1);

        UpdateMask.WriteUInt32s(data, values);
    }

    public void SetUpdateField<T>(int index, T value, DynamicFieldChangeType changeType) where T : new()
    {
        Span<uint> values = stackalloc uint[4];
        int count;
        if (value is int intValue)
        {
            values[0] = new UpdateValues { SignedValue = intValue }.UnsignedValue;
            count = 1;
        }
        else if (value is uint uintValue)
        {
            values[0] = uintValue;
            count = 1;
        }
        else if (value is float floatValue)
        {
            values[0] = new UpdateValues { FloatValue = floatValue }.UnsignedValue;
            count = 1;
        }
        else if (value is ulong ulongValue)
        {
            values[0] = MathFunctions.Pair64_LoPart(ulongValue);
            values[1] = MathFunctions.Pair64_HiPart(ulongValue);
            count = 2;
        }
        else if (value is WowGuid128 guid)
        {
            values[0] = MathFunctions.Pair64_LoPart(guid.GetLowValue());
            values[1] = MathFunctions.Pair64_HiPart(guid.GetLowValue());
            values[2] = MathFunctions.Pair64_LoPart(guid.GetHighValue());
            values[3] = MathFunctions.Pair64_HiPart(guid.GetHighValue());
            count = 4;
        }
        else
            throw new Exception($"Unhandled type {typeof(T).Name} in SetUpdateField!");

        SetUpdateField(index, values[..count], changeType);
    }
}
