using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Engine
{
    public static class SpellEngine
    {
        public class CompiledRune
        {
            public RuneType type;
            public HexXY relPos;
            public uint dir;
            public CompiledRune[] neighs;

            public CompiledRune(RuneType type, HexXY relPos, uint dir)
            {
                this.type = type;
                this.relPos = relPos;
                this.dir = dir;
            }
        }

        static Queue<CompiledRune> compilationFront = new Queue<CompiledRune>();

        public static CompiledRune CompileSpell(Rune compileRune, HexXY compileRunePos)
        {
            HexXY refPos = compileRunePos;
            CompiledRune root = new CompiledRune((RuneType)compileRune.entityType, new HexXY(0, 0), compileRune.dir);

            compilationFront.Clear();
            compilationFront.Enqueue(root);

            ++WorldBlock.S.pfExpandMarker;
            WorldBlock.S.pfExpandMap[compileRunePos.x, compileRunePos.y] = WorldBlock.S.pfExpandMarker;

            CompiledRune c;            

            do
            {
                c = compilationFront.Dequeue();                

                for(int i = 0; i < 6; i++)
                {
                    var d = HexXY.neighbours[i];
                    var np = refPos + c.relPos + d;
                    if (WorldBlock.S.pfExpandMap[np.x, np.y] < WorldBlock.S.pfExpandMarker)
                    {
                        Rune rune = (Rune)WorldBlock.S.entityMap[np.x, np.y].FirstOrDefault(ent => ent is Rune);
                        if (rune == null) continue;

                        WorldBlock.S.pfExpandMap[np.x, np.y] = WorldBlock.S.pfExpandMarker;

                        var nrune = new CompiledRune((RuneType)rune.entityType, c.relPos + d, rune.dir);
                        c.neighs[i] = nrune;
                        int reverseIdx = (i + 3) % 6;
                        nrune.neighs[reverseIdx] = c;

                        compilationFront.Enqueue(nrune);                        
                    }
                }
            } while (compilationFront.Count > 0);

            return root;
        }
    }
}
