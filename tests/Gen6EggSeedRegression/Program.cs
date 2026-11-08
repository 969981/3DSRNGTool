using System.Reflection;
using Pk3DSRNGTool;
using Pk3DSRNGTool.RNG;

namespace Gen6EggSeedRegression;

// Tests the real Egg6 code in the production assembly (not a rewritten RNG).
internal static class Program
{
    private static readonly Type Pool = typeof(Egg6).Assembly.GetType("Pk3DSRNGTool.Core.RNGPool", throwOnError: true)!;
    private static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

    private static void Invoke(string name, params object[] args) =>
        Pool.GetMethod(name, Flags)!.Invoke(null, args);

    private static T Call<T>(string name, params object[] args) =>
        (T)Pool.GetMethod(name, Flags)!.Invoke(null, args)!;

    private static Egg6 NewEgg(bool accept, bool masuda = false, bool charm = false)
    {
        var egg = new Egg6
        {
            MaleIVs = new[] { 31, 31, 31, 31, 31, 31 },
            FemaleIVs = new[] { 0, 0, 0, 0, 0, 0 },
            MaleItem = 2,
            FemaleItem = 0,
            InheritAbility = 0,
            Gender = 127,
            TSV = 131,
            TRV = 0,
            ShinyCharm = charm,
            MMethod = masuda,
            OtherTSVs = Array.Empty<int>(),
            IsMainRNGEgg = accept && !masuda && !charm
        };
        egg.MarkItem();
        return egg;
    }

    private sealed record Snapshot(ulong Seed, byte Nature, byte Gender, byte Ability,
        string Ivs, uint Ec, uint Pid, bool Shiny)
    {
        public static Snapshot From(ResultE6 egg) =>
            new(egg.EggSeed!.Value, egg.Nature, egg.Gender, egg.Ability,
                string.Join("/", egg.IVs), egg.EC, egg.PID, egg.Shiny);
        public override string ToString() =>
            $"Key={Seed:X16} Nature={Nature} Gender={Gender} Ability={Ability} " +
            $"IV={Ivs} EC={Ec:X8} PID={Pid:X8} Shiny={Shiny}";
    }

    private static (Snapshot current, Snapshot[] future) Simulate(
        uint key0, uint key1, bool accept, bool masuda = false, bool charm = false)
    {
        Egg6 egg = NewEgg(accept, masuda, charm);
        // Match Search6_Egg(): the same main MT instance supplies buffering
        // and the future-frame refill, independently of the entered egg key.
        var rng = new MersenneTwister(0u);
        Invoke("CreateBuffer", rng, 150, true);
        Pool.GetField("Considerdelay", Flags)!.SetValue(null, false);
        Pool.GetField("igenerator", Flags)!.SetValue(null, egg);

        var current = Snapshot.From((ResultE6)egg.Generate(null, new[] { key0, key1 }));
        var future = new Snapshot[8];
        for (int i = 0; i < future.Length; i++)
        {
            future[i] = Snapshot.From(Call<ResultE6>("Generate6"));
            Invoke("AddNext", rng, true);
        }
        return (current, future);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception("FAILED: " + message);
    }

    public static int Main()
    {
        try
        {
            var a = Simulate(0x22222222, 0x11111111, accept: true);
            var b = Simulate(0x44444444, 0x33333333, accept: true);
            var repeat = Simulate(0x22222222, 0x11111111, accept: true);

            Require(a.current.Seed == 0x1111111122222222UL, "A key word order");
            Require(b.current.Seed == 0x3333333344444444UL, "B key word order");
            Require(a.current.Ec != b.current.Ec, "changed key changes current egg EC");
            Require(a.current == repeat.current, "same key reproduces current egg");
            Require(a.future.SequenceEqual(b.future),
                "unchanged top-level MT seed reproduces future-frame results");
            Require(a.current.Pid == 0 && b.current.Pid == 0,
                "no Masuda/charm leaves current egg PID undetermined from key alone");
            Console.WriteLine("A accept/current: " + a.current);
            Console.WriteLine("B accept/current: " + b.current);
            Console.WriteLine("Future A/B first: " + a.future[0] + " (equal as expected)");

            var c = Simulate(0x22222222, 0x11111111,
                accept: false, masuda: true, charm: true);
            var d = Simulate(0x44444444, 0x33333333,
                accept: false, masuda: true, charm: true);
            Require(c.current.Pid != d.current.Pid,
                "with Masuda and charm egg-key-derived PID changes");
            Require(c.future.SequenceEqual(d.future),
                "future main MT stream still independent of current egg key");
            Console.WriteLine("A Masuda+Charm: " + c.current);
            Console.WriteLine("B Masuda+Charm: " + d.current);
            Console.WriteLine("PASS: actual Egg6 seed-A/B, acceptance, PID-source, future frames");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}
