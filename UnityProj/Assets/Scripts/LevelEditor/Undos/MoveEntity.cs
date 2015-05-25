#if UNITY_EDITOR
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Undos
{
    public class MoveEntity : IUndo
    {
        Entity ent;
        HexXY from, to;

        public MoveEntity(Entity ent, HexXY from, HexXY to)
        {
            this.ent = ent;
            this.from = from;
            this.to = to;
        }

        public void Move()
        {          
            Level.S.RemoveEntity(ent.pos, ent);
            ent.pos = to;           
            Level.S.AddEntity(ent.pos, ent);           

            Interfacing.PerformInterfaceTeleport(ent.graphicsHandle, to);        
        }

        public void Redo()
        {
            Move();
        }

        public void Undo()
        {
            Level.S.RemoveEntity(ent.pos, ent);
            ent.pos = from;          
            Level.S.AddEntity(ent.pos, ent);

            Interfacing.PerformInterfaceTeleport(ent.graphicsHandle, from);
        }
    }
}
#endif
