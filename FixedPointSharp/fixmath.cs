﻿using BEPUutilities;
using FixMath.NET;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace Deterministic.FixedPoint
{
  public partial struct fixmath
  {
    private static readonly fp _atan2Number1 = new(-883);
    private static readonly fp _atan2Number2 = new(3767);
    private static readonly fp _atan2Number3 = new(7945);
    private static readonly fp _atan2Number4 = new(12821);
    private static readonly fp _atan2Number5 = new(21822);
    private static readonly fp _atan2Number6 = new(65536);
    private static readonly fp _atan2Number7 = new(102943);
    private static readonly fp _atan2Number8 = new(205887);
    private static readonly fp _atanApproximatedNumber1 = new(16036);
    private static readonly fp _atanApproximatedNumber2 = new(4345);
    private static readonly byte[] _bsrLookup = { 0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30, 8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31 };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BitScanReverse(uint num)
    {
      num |= num >> 1;
      num |= num >> 2;
      num |= num >> 4;
      num |= num >> 8;
      num |= num >> 16;
      return _bsrLookup[(num * 0x07C4ACDDU) >> 27];
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static int CountLeadingZeroes(uint num) { return num == 0 ? 32 : BitScanReverse(num) ^ 31; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountLeadingZeroes(ulong x)
    {
      int result = 0;
      while ((x & 0xF000000000000000) == 0) { result += 4; x <<= 4; }
      while ((x & 0x8000000000000000) == 0) { result += 1; x <<= 1; }
      return result;
    }

    /// <param name="num">Angle in radians</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Sin(fp num)
    {
      num.value %= fp.pi2.value;
      num *= fp.one_div_pi2;
      var raw = fixlut.sin(num.value);
      fp result;
      result.value = raw;
      return result;
    }

    /// <param name="num">Angle in radians</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Cos(fp num)
    {
      Godot.GD.Print("\nCOS PROCESSING: \ninput: " + num.ToString());

      num.value %= fp.pi2.value;
      Godot.GD.Print("input.value %= pi2: " + num.ToString());

      num *= fp.one_div_pi2;
      Godot.GD.Print("input *= 1/pi2: " + num.ToString());

      return new fp(fixlut.cos(num.value));
    }

    /// <param name="num">Angle in radians</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Tan(fp num)
    {
      num.value %= fp.pi2.value;
      num *= fp.one_div_pi2;
      return new fp(fixlut.tan(num.value));
    }

    /// <param name="num">Cos [-1, 1]</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Acos(fp num) { return new fp(fixlut.acos(num.value)); }


    /// <param name="num">Sin [-1, 1]</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Asin(fp num) { return new fp(fixlut.asin(num.value)); }


    /// <param name="num">Tan</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Atan(fp num) { return Atan2(num, F64.C1); }


    /// <param name="num">Tan [-1, 1]</param>
    /// Max error ~0.0015
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp AtanApproximated(fp num)
    {
      fp absX = Fix64.Abs(num);
      return fp.pi_quarter * num - num * (absX - F64.C1) * (_atanApproximatedNumber1 + _atanApproximatedNumber2 * absX);
    }

    /// <param name="x">Denominator</param>
    /// <param name="y">Numerator</param>
    public static fp Atan2(fp y, fp x)
    {
      var absX = Fix64.Abs(x);
      var absY = Fix64.Abs(y);
      var t3 = absX;
      var t1 = absY;
      var t0 = Max(t3, t1);
      t1 = Min(t3, t1);
      t3 = F64.C1 / t0;
      t3 = t1 * t3;
      var t4 = t3 * t3;
      t0 = _atan2Number1;
      t0 = t0 * t4 + _atan2Number2;
      t0 = t0 * t4 - _atan2Number3;
      t0 = t0 * t4 + _atan2Number4;
      t0 = t0 * t4 - _atan2Number5;
      t0 = t0 * t4 + _atan2Number6;
      t3 = t0 * t3;
      t3 = absY > absX ? _atan2Number7 - t3 : t3;
      t3 = x < F64.C0 ? _atan2Number8 - t3 : t3;
      t3 = y < F64.C0 ? -t3 : t3;
      return t3;
    }

    /// <param name="num">Angle in radians</param>
    public static void SinCos(fp num, out fp sin, out fp cos)
    {
      num.value %= fp.pi2.value;
      num *= fp.one_div_pi2;
      fixlut.sin_cos(num.value, out var sinVal, out var cosVal);
      sin.value = sinVal;
      cos.value = cosVal;
    }

    public static fp Rcp(fp num)   { return new fp(4294967296 / num.value); }

    public static fp Rsqrt(fp num) { return new fp(4294967296 / Sqrt(num).value); }

    public static fp Sqrt(fp num)
    {
      fp r;

      if (num.value == 0) r.value = 0; 
      else
      {
        var b = (num.value >> 1) + 1L;
        var c = (b + (num.value / b)) >> 1;

        while (c < b) { b = c; c = (b + (num.value / b)) >> 1; }

        r.value = b << (fixlut.PRECISION >> 1);
      }

      return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Ceil(fp num)
    {
      var fractions = num.value & 0x000000000000FFFFL;

      if (fractions == 0) return num; 

      num.value = num.value >> fixlut.PRECISION << fixlut.PRECISION;
      num.value += fixlut.ONE;
      return num;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Fractions(fp num) { return new fp(num.value & 0x000000000000FFFFL); }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundToInt(fp num)
    {
      var fraction = num.value & 0x000000000000FFFFL; 

      if (fraction >= fixlut.HALF)  return num.AsInt + 1;  
      return num.AsInt;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Min(fp a, fp b) { return a.value < b.value ? a : b; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Min(int a, int b) { return a < b ? a : b; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Min(long a, long b) { return a < b ? a : b; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Max(fp a, fp b) { return a.value > b.value ? a : b; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Max(int a, int b) { return a > b ? a : b; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Max(long a, long b) { return a > b ? a : b; } 


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Clamp(fp num, fp min, fp max)
    {
      if (num.value < min.value) return min;  
      if (num.value > max.value) return max;  
      return num;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(int num, int min, int max)
    {
      return num < min ? min : num > max ? max : num;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Clamp(long num, long min, long max)
    {
      return num < min ? min : num > max ? max : num;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Clamp01(fp num)
    {
      if (num.value < 0) return F64.C0;  
      return num.value > F64.C1.value ? F64.C1 : num;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Lerp(fp from, fp to, fp t) { t = Clamp01(t); return from + (to - from) * t; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fbool Lerp(fbool from, fbool to, fp t) { return t.value > F64.C0p5.value ? to : from; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Repeat(fp value, fp length) { return Clamp(value - Fix64.Floor(value / length) * length, 0, length); }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp LerpAngle(fp from, fp to, fp t)
    {
      var num = Repeat(to - from, fp.pi2);
      return Lerp(from, from + (num > fp.pi ? num - fp.pi2 : num), t); 
    } 

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp NormalizeRadians(fp angle) { angle.value %= fixlut.PI; return angle; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp LerpUnclamped(fp from, fp to, fp t) { return from + (to - from) * t; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Sign(fp num) { return num.value < fixlut.ZERO ? fp.minus_one : F64.C1; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOppositeSign(fp a, fp b) { return ((a.value ^ b.value) < 0); }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp SetSameSign(fp target, fp reference)
    { return IsOppositeSign(target, reference) ? target * fp.minus_one : target; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Pow2(int power) { return new fp(fixlut.ONE << power); }


    /// <summary>
    /// Returns true if raw value of <paramref name="powerLevel"/> is greater than <see cref="F64.OverNineThousand"/> - 1. 
    /// </summary>  
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CheckScouter(fp powerLevel) { return powerLevel > F64.OverNineThousand.value - Fix64.One.value; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static fp Exp(fp num)
    {
      if (num == F64.C0) return F64.C1;
      if (num == F64.C1) return fp.e;
      if (num.value >= 2097152) return Fix64.MaxValue;
      if (num.value <= -786432) return F64.C0;

      var neg = num.value < 0;
      if (neg) num = -num;

      var result = num + F64.C1;
      var term = num;

      for (var i = 2; i < 30; i++)
      {
        term *= num / (fp)i;
        result += term;

        if (term.value < 500 && ((i > 15) || (term.value < 20))) break;
      }

      if (neg) result = F64.C1 / result;

      return result;
    }
  }
}