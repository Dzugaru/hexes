using UnityEngine;
using System.Collections;
using System;

namespace Engine
{
    public class World
    {
        public const int BlockSize = 32;
        public static readonly int TerrainCellTypesCount = Enum.GetValues(typeof(TerrainCellType)).Length;
    }
}
