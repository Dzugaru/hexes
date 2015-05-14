using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public interface IEntityComponent
    {
        void OnSpawn(HexXY pos);
        bool OnUpdate(float dt);
        void OnDie();       
    }
}
