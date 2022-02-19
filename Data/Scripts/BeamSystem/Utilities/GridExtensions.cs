using System;
using VRage.Game.ModAPI;
using VRageMath;

namespace BeamSystem
{
    static class GridExtensions
    {
        internal static IMySlimBlock FirstBlock(this IMyCubeGrid grid, Vector3D worldStart, Vector3D worldEnd,
               Func<IMySlimBlock, bool> pred = null) //, Vector3I? gridSizeInflate = null)
        {
            for (var itr = CellEnumerator.EnumerateGridCells(grid, worldStart, worldEnd); //, gridSizeInflate);
                itr.IsValid;
                itr.MoveNext())
            {
                var block = grid.GetCubeBlock(itr.Current);
                if (block != null && (pred == null || pred.Invoke(block)))
                    return block;
            }
            return null;
        }

        internal static bool IsNeighborhood(this IMyCubeBlock a, IMyCubeBlock b)
        {
            var am = a.Max + Vector3I.One;
            var an = a.Min - Vector3I.One;
            var bm = b.Max;
            var bn = b.Min;

            am -= an - Vector3I.One;
            bm -= bn - Vector3I.One;

            var d = an - bn;
            d.X = Math.Abs(d.X) * 2;
            d.Y = Math.Abs(d.Y) * 2;
            d.Z = Math.Abs(d.Z) * 2;

            bm += am;
            return d.X <= bm.X && d.Y <= bm.Y && d.Z <= bm.Z;
        }
    }
}
