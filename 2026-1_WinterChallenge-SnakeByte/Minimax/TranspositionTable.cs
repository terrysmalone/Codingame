using System;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class TranspositionTable
{
    private const int TableSize = 1 << 20;
    private const ulong TableMask = TableSize - 1;

    private readonly TranspositionEntry[] _entries = new TranspositionEntry[TableSize];

    internal void Clear()
    {
        Array.Clear(_entries);
    }

    internal bool TryGet(ulong hash, int depth, out TranspositionEntry entry)
    {
        ref var slot = ref _entries[(int)(hash & TableMask)];
        entry = slot;
        return slot.Flag != TransFlag.None && slot.Hash == hash && slot.Depth >= depth;
    }

    internal void Store(ulong hash, int score, int depth, TransFlag flag)
    {
        ref var slot = ref _entries[(int)(hash & TableMask)];

        if (slot.Flag == TransFlag.None || depth >= slot.Depth)
        {
            slot.Hash = hash;
            slot.Score = score;
            slot.Depth = depth;
            slot.Flag = flag;
        }
    }
}
