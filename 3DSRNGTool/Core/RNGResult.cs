namespace Pk3DSRNGTool.Core
{
    public abstract class RNGResult : IPKM
    {
        public static bool IsPokemon = true;

        public uint PID, EC;
        public uint PSV => ((PID >> 16) ^ (PID & 0xFFFF)) >> 4;
        public uint PRV => ((PID >> 16) ^ (PID & 0xFFFF)) & 0xF;
        public byte hiddenpower;
        public int[] Stats;
        public bool Shiny;
        public bool SquareShiny;
        // True only for shinies evaluated against an available trainer TRV.
        // TSV-only secondary-trainer matches cannot be classified as star/square.
        public bool ShinyTypeKnown;
        public bool Synchronize;
    }
}
