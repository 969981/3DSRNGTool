using System;
using System.Linq;
using System.Reflection;
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

    private static ResultE6 Generate(uint key0, uint key1, uint mainSeed)
    {
        InitBuffer(mainSeed);
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
        return (ResultE6)egg.Generate(null, new uint[] { key0, key1 });
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
