using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Sim.Util;

// *Really* minimal PCG32 code / (c) 2014 M.E. O'Neill / pcg-random.org
// Licensed under Apache License 2.0 (NO WARRANTY, etc. see website)
// Modified by Kruno

public struct Rng
{
    private ulong state, inc;

    public Rng(ulong state, ulong inc)
    {
        this.state = state;
        this.inc = inc;
    }

    public int Between(int min, int max)
    {
        var c = this.NextFloat();
        var res = (int)(min + ((max - min) * c));
        return res;
    }

    public float NextFloat()
    {
        return (float)(this.IncrementState() * System.MathF.Pow(2.0f, -32.0f));
    }

    public int Next(int max)
    {
        return (int)this.Next((uint)max);
    }

    public uint Next(uint range)
    {
        uint x = this.IncrementState();
        ulong m = (ulong)x * (ulong)range;
        uint l = (uint)m;
        if (l < range)
        {
            uint t = (uint)(-range);
            if (t >= range)
            {
                t -= range;
                if (t >= range)
                    t %= range;
            }
            while (l < t)
            {
                x = this.IncrementState();
                m = (ulong)x * (ulong)range;
                l = (uint)m;
            }
        }
        return (uint)(m >> 32);
    }

    public uint IncrementState()
    {
        ulong oldstate = this.state;

        this.state = oldstate * 6364136223846793005UL + (this.inc | 1);

        uint xorshifted = (uint)(((oldstate >> 18) ^ oldstate) >> 27);
        uint rot = (uint)(oldstate >> 59);

        uint a = xorshifted >> (int)rot;
        uint b = xorshifted << (int)((-rot) & 31);

        return a | b;
    }

    public uint IncrementStateAvx()
    {
        var oldstate = Vector128.Create<ulong>(this.state);

        this.state = this.state * 6364136223846793005UL + (this.inc | 1);

        var xorShifted = Avx2.ShiftRightLogical(
            Avx2.Xor(Avx2.ShiftRightLogical(oldstate, 18), oldstate),
            27
        ).AsUInt32();

        var rot = Avx2.ShiftRightLogical(xorShifted, 59).AsUInt32();

        var a = Avx2.ShiftRightLogical(xorShifted, rot.AsByte()[0]);
        var amt = Avx2.And(-rot, Vector128.Create<uint>(31)).AsByte()[0];
        var b = Avx2.ShiftLeftLogical(xorShifted, amt);

        return Avx2.Or(a, b).AsUInt32()[0];
    }

}
