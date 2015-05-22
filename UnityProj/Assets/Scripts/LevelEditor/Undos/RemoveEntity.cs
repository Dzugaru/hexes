#if UNITY_EDITOR
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Undos
{
    public class RemoveEntity : IUndo
    {
        Entity ent;
        HexXY p;

        public RemoveEntity(Entity ent, HexXY p)
        {
            this.ent = ent;
            this.p = p;
        }

        public void Remove()
        {
            ent.Die();
            IStaticBlocker blocker = ent as IStaticBlocker;
            if (blocker != null)
                LevelEditor.S.ChangeStaticPassability(p, false);
        }

        public void Redo()
        {
            Remove();
        }

        public void Undo()
        {
            ent.Spawn(p);
            IStaticBlocker blocker = ent as IStaticBlocker;
            if (blocker != null)
                LevelEditor.S.ChangeStaticPassability(p, false);
        }
    }
}
#endif
