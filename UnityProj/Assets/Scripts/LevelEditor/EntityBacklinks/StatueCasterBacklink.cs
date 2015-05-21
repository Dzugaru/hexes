using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace EntityPutters
{
    class StatueCasterBacklink : EditorEntityBacklink
    {
        public HexXY sourceSpellPos;

        public override Entity CreateEntity()
        {
            return new StatueCaster(0, sourceSpellPos);            
        }
    }
}
