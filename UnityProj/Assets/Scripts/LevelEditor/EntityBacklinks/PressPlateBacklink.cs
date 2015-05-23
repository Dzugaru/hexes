using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace EntityPutters
{
    public class PressPlateBacklink : EditorEntityBacklink
    {
        PressPlate ent;

        public ScriptObjectID id;     

        public override Entity CreateEntity()
        {
            return new PressPlate();
        }

#if UNITY_EDITOR
        void Start()
        {
            if (LevelEditor.S != null)
            {
                ent = (PressPlate)GetComponent<PressPlateGraphics>().entity;
                id.id = ent.id.id;
            }
        }

        void Update()
        {
            if (LevelEditor.S != null)
            {
                ent.id.id = id.id;
            }
        }
#endif
    }
}
