using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.LevelScripts
{
    public class L001_Tutor : LevelScript
    {
        public enum ID
        {
            PressurePlate01,
            Door01
        }

        public override HexXY PlayerSpawnPos
        {
            get
            {
                return new HexXY(0, -3);
            }
        }
    }
}
