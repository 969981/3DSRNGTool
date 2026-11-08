using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Pk3DSRNGTool;
using Pk3DSRNGTool.RNG;
using Xunit;

namespace Pk3DSRNGTool.Tests;

public class Gen6EggSeedTests
{
    // ResultE6 captures a main RNG buffer snapshot for the metadata columns.
    // This setup is not used for the actual egg PID when MM + charm are enabled.
    private static void InitBuffer(uint mainSeed)
    {
        var pool = typeof(Egg6).Assembly.GetType("Pk3DSRNGTool.Core.RNGPool", true)!;
        var method = pool.GetMethod("CreateBuffer", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
        method.Invoke(null, new object[] { new MersenneTwister(mainSeed), 150, true });
    }

    private static Egg6 CreateEgg()
    {
        var egg = new Egg6
        {
            MaleIVs = Enumerable.Repeat(31, 6).ToArray(),
            FemaleIVs = Enumerable.Repeat(31, 6).ToArray(),
            MaleItem = 2, // Destiny Knot
            FemaleItem = 1, // Everstone
            Gender = 127,
            InheritAbility = 0,
            MMethod = true,
            ShinyCharm = true,
            TSV = 131,
            TRV = 0,
            IsMainRNGEgg = false,
            OtherTSVs = Array.Empty<int>()
        };
        egg.MarkItem();
        return egg;
    }

    private static ResultE6 Generate(uint key0, uint key1, uint mainSeed)
    {
        InitBuffer(mainSeed);
        return (ResultE6)CreateEgg().Generate(null, new uint[] { key0, key1 });
    }

    // Exercises production RNGPool.Generate6 / Egg6.Generate rather than
    // an invented "next egg" function that advances the current key.
    private static (ulong Key, uint PID, uint EC)[] FutureEggs(
        uint mainSeed, uint key0, uint key1, int firstFrame = 418, int count = 4)
    {
        var rng = new MersenneTwister(mainSeed);
        for (int i = 0; i < firstFrame; i++) rng.Next();
        var pool = typeof(Egg6).Assembly.GetType("Pk3DSRNGTool.Core.RNGPool", true)!;
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        var egg = CreateEgg();
        pool.GetField("igenerator", flags)!.SetValue(null, egg);
        pool.GetField("Considerdelay", flags)!.SetValue(null, false);
        pool.GetMethod("CreateBuffer", flags)!.Invoke(null, new object[] { rng, 150, true });
        var results = new List<(ulong Key, uint PID, uint EC)>();
        try
        {
            _ = egg.Generate(null, new uint[] { key0, key1 });
            for (int i = 0; i < count; i++)
            {
                var next = (ResultE6)pool.GetMethod("Generate6", flags)!.Invoke(null, null)!;
                Assert.True(next.EggSeed.HasValue);
                results.Add((next.EggSeed.Value, next.PID, next.EC));
                pool.GetMethod("AddNext", flags)!.Invoke(null, new object[] { rng, true });
            }
            return results.ToArray();
        }
        finally
        {
            pool.GetMethod("Clear", flags)!.Invoke(null, null);
        }
    }

    [Fact]
    public void Screenshot_case_future_eggs_follow_main_MT_not_current_egg_key()
    {
        // XY/ORAS: current egg Seed determines -1, future eggs use the
        // independent main MT stream at the chosen accept/reject frame.
        var a = FutureEggs(0, 0xF5D266F8, 0xFECED4DB);
        var b = FutureEggs(0, 0x44444444, 0x33333333);
        Assert.Equal(a, b);
        var c = FutureEggs(0xDEADBEEF, 0xF5D266F8, 0xFECED4DB);
        Assert.NotEqual(a[0].Key, c[0].Key);
        Assert.NotEqual(a[0].EC, c[0].EC);
    }

    [Fact]
    public void Changing_egg_key_changes_PID_and_EC_in_MM_plus_charm_mode()
    {
        var a = Generate(0x22222222, 0x11111111, 0);
        var b = Generate(0x44444444, 0x33333333, 0);
        Assert.Equal(0x1111111122222222UL, a.EggSeed);
        Assert.Equal(0x3333333344444444UL, b.EggSeed);
        Assert.Equal(0x0AD2D21Au, a.EC);
        Assert.Equal(0xCAAC136Au, b.EC);
        Assert.Equal(0xD8CDD888u, a.PID);
        Assert.Equal(0xBCE5CFB2u, b.PID);
        Assert.NotEqual(a.EC, b.EC);
        Assert.NotEqual(a.PID, b.PID);
    }

    [Fact]
    public void Screenshot_egg_key_is_used_as_high_low_words_and_changes_with_key0()
    {
        var original = Generate(0xF5D266F8, 0xFECED4DB, 0);
        var edited = Generate(0xF5D266F9, 0xFECED4DB, 0);
        Assert.Equal(0xFECED4DBF5D266F8UL, original.EggSeed);
        Assert.Equal(0xFECED4DBF5D266F9UL, edited.EggSeed);
        Assert.NotEqual(original.EC, edited.EC);
        Assert.NotEqual(original.PID, edited.PID);
    }

    [Fact]
    public void Current_egg_PID_is_independent_of_main_MT_seed_in_MM_plus_charm_mode()
    {
        var a = Generate(0x22222222, 0x11111111, 0);
        var b = Generate(0x22222222, 0x11111111, 0xDEADBEEF);
        Assert.Equal(a.EggSeed, b.EggSeed);
        Assert.Equal(a.PID, b.PID);
        Assert.Equal(a.EC, b.EC);
        Assert.Equal(a.IVs, b.IVs);
        Assert.Equal(a.Nature, b.Nature);
    }
}
