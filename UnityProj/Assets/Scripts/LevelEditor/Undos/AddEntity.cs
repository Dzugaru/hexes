using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Undos
{
    public class AddEntity : IUndo
    {
        Entity ent;
        HexXY p;

        public AddEntity(Entity ent, HexXY p)
        {
            this.ent = ent;
            this.p = p;
        }

        public void Add()
        {
            ent.Spawn(p);
        }

        public void Redo()
        {
            Add();
        }

        public void Undo()
        {
            ent.Die();
        }
    }
}
