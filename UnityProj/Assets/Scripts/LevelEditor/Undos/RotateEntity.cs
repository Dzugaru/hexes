#if UNITY_EDITOR
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Undos
{
    public class RotateEntity : IUndo
    {
        Entity ent;
        HexXY p;

        public RotateEntity(Entity ent, HexXY p)
        {
            this.ent = ent;
            this.p = p;
        }

        public void Rotate()
        {            
            ent.dir = (ent.dir + 1) % 6;            
            ent.UpdateInterface();        
        }

        public void Redo()
        {
            Rotate();
        }

        public void Undo()
        {
            ent.dir = (ent.dir + 5) % 6;
            ent.UpdateInterface();
        }
    }
}
#endif
