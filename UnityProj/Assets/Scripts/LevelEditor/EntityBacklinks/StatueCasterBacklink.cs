using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace EntityPutters
{
    class StatueCasterBacklink : EditorEntityBacklink
    {
        StatueCaster ent;
        public HexXY sourceSpellPos;

        public override Entity CreateEntity()
        {
            return new StatueCaster(0, sourceSpellPos);            
        }

#if UNITY_EDITOR
        void Start()
        {
            if (LevelEditor.S != null)
            {
                ent = (StatueCaster)GetComponent<StatueCasterGraphics>().entity;
                sourceSpellPos = ent.sourceSpellPos;
            }
        }

        void Update()
        {
            if (LevelEditor.S != null)
            {
                ent.sourceSpellPos = sourceSpellPos;
            }
        }
#endif
    }
}
