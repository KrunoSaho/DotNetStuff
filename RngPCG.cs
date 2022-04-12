using System;

namespace Util;

// *Really* minimal PCG32 code / (c) 2014 M.E. O'Neill / pcg-random.org
// Licensed under Apache License 2.0 (NO WARRANTY, etc. see website)

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
        var c = this.NextFloat(max);
        var res = (int)(min + ((max - min) * c));
        return res;
    }

    public float NextFloat(int max)
    {
        return (float)((double)this.IncrementState() * Math.Pow(2.0, -32.0));
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
        var rot = (int)(oldstate >> 59);
        return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
    }

}
