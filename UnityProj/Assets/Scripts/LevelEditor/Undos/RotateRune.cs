using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Undos
{
    public class RotateRune : IUndo
    {
        HexXY p;

        public RotateRune(HexXY p)
        {
            this.p = p;
        }

        public void Rotate()
        {
            var rune = (Rune)Level.S.GetEntities(p).First(e => e is Rune);
            rune.dir = (rune.dir + 1) % 6;
            rune.UpdateInterface();        
        }

        public void Redo()
        {
            Rotate();
        }

        public void Undo()
        {
            var rune = (Rune)Level.S.GetEntities(p).First(e => e is Rune);
            rune.dir = (rune.dir - 1) % 6;
            rune.UpdateInterface();
        }
    }
}
