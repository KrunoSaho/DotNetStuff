public struct Rng
{
    private ulong state;
    private readonly ulong inc;

    public Rng(ulong state, ulong inc)
    {
        this.state = state;
        this.inc = inc;
    }

    public int Between(int min, int max)
    {
        var c = this.NextFloat();
        var res = MathF.Round(min + ((max - min) * c));
        return (int)res;
    }

    public float NextFloat()
    {
        return (float)(this.AdvanceAvx() * System.MathF.Pow(2.0f, -32.0f));
    }

    public int Next(int max)
    {
        return (int)this.Next((uint)max);
    }


    public uint Next(uint range)
    {
        uint x = this.AdvanceAvx();
        ulong m = (ulong)x * (ulong)range;
        uint l = (uint)m;
        uint t = (uint)(-range);

        if (l < range)
        {
            if (t >= range)
            {
                t -= range;
                if (t >= range)
                    t %= range;
            }

            while (l < t)
            {
                x = this.AdvanceAvx();
                m = (ulong)x * (ulong)range;
                l = (uint)m;
            }
        }

        return (uint)(m >> 32);
    }

    public uint AdvanceAvx()
    {
        var oldState = Vector128.Create<ulong>(this.state);

        this.state = oldState[0] * 6364136223846793005UL + (this.inc | 1);

        var xorShifted = Avx2.ShiftRightLogical(
            Avx2.Xor(Avx2.ShiftRightLogical(oldState, 18), oldState),
            27
        ).AsUInt32();

        var rot = Avx2.ShiftRightLogical(xorShifted, 59).AsUInt32();

        var a = Avx2.ShiftRightLogical(xorShifted, rot.AsByte()[0]);
        var amt = Avx2.And(-rot, Vector128.Create<uint>(31)).AsByte()[0];
        var b = Avx2.ShiftLeftLogical(xorShifted, amt);

        return Avx2.Or(a, b).AsUInt32()[0];
    }


    // For reference
    // public uint Advance()
    // {
    //     ulong oldState = this.state;

    //     this.state = oldState * 6364136223846793005UL + (this.inc | 1);

    //     uint xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
    //     uint rot = (uint)(oldState >> 59);

    //     uint a = xorShifted >> (int)rot;
    //     uint b = xorShifted << (int)((-rot) & 31);

    //     return a | b;
    // }
}
