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
            IStaticBlocker blocker = ent as IStaticBlocker;

            if (blocker != null && blocker.IsBlocking)
                LevelEditor.S.ChangeStaticPassability(ent.pos, false);
            Level.S.RemoveEntity(ent.pos, ent);

            ent.pos = to;            

            Level.S.AddEntity(ent.pos, ent);
            if (blocker != null && blocker.IsBlocking)
                LevelEditor.S.ChangeStaticPassability(ent.pos, false);

            Interfacing.PerformInterfaceTeleport(ent.graphicsHandle, to);        
        }

        public void Redo()
        {
            Move();
        }

        public void Undo()
        {
            IStaticBlocker blocker = ent as IStaticBlocker;

            if (blocker != null && blocker.IsBlocking)
                LevelEditor.S.ChangeStaticPassability(ent.pos, false);
            Level.S.RemoveEntity(ent.pos, ent);

            ent.pos = from;

            if (blocker != null && blocker.IsBlocking)
                LevelEditor.S.ChangeStaticPassability(ent.pos, false);
            Level.S.AddEntity(ent.pos, ent);

            Interfacing.PerformInterfaceTeleport(ent.graphicsHandle, from);
        }
    }
}
#endif
