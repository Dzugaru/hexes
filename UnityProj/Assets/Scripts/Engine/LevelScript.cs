using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public abstract class LevelScript
    {
        public abstract HexXY PlayerSpawnPos { get; }

        public virtual void Update()
        {
        }
    }
}
