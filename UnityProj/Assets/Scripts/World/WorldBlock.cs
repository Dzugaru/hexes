using System.Runtime.InteropServices;
using UnityEngine;

namespace Engine
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct WorldBlock
    {
        public const int size = 10;

        public HexXY position;
        public fixed int cellTypes[size * size];
        public fixed int cellTypeCounts[World.TerrainCellTypesCount];
        public int nonEmptyCellsCount;    
    }
}

