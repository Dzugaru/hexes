using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Engine
{
    public class Spell
    {
        public CompiledRune root;
        public List<CompiledRune> allRunes = new List<CompiledRune>();

        public class CompiledRune
        {            
            public RuneType type;
            public HexXY relPos;
            public uint dir;
            public CompiledRune[] neighs;

            public uint listIdx;
            public int[] neighsListIdxs;

            public CompiledRune(RuneType type, HexXY relPos, uint dir, uint listIdx)
            {
                this.type = type;
                this.relPos = relPos;
                this.dir = dir;
                this.listIdx = listIdx;
                this.neighs = new CompiledRune[6];
                this.neighsListIdxs = new int[6];
                for (int k = 0; k < 6; k++)                
                    this.neighsListIdxs[k] = -1;                
            }
        }

        public void Cast(Entity ent, uint dir)
        {
            var spEx = new SpellExecuting(ent, this, ent.pos, dir);
            spEx.SpawnAvatar(root, null, ent.pos, dir); //TODO: spawn at spell target? what to do with direction? make random if far?
            executingSpells.Add(spEx);
        }


        static List<SpellExecuting> executingSpells = new List<SpellExecuting>();
        public static void Update(float dt)
        {
            for (int i = 0; i < executingSpells.Count; i++)
            {
                var exSp = executingSpells[i];
                if (!exSp.isExecuting)
                    executingSpells.RemoveAt(i--);
                else
                    exSp.Update(dt);
            }            
        }
        

        static Queue<CompiledRune> compilationFront = new Queue<CompiledRune>();
        static Dictionary<HexXY, CompiledRune> compilationMap = new Dictionary<HexXY, CompiledRune>();
        public static Spell CompileSpell(Rune compileRune, HexXY compileRunePos)
        {
            Spell spell = new Spell();

            HexXY refPos = compileRunePos;
            spell.root = new CompiledRune((RuneType)compileRune.entityType, new HexXY(0, 0), compileRune.dir, 0);
            spell.allRunes.Add(spell.root);

            compilationFront.Clear();
            compilationMap.Clear();
            compilationFront.Enqueue(spell.root);
            compilationMap.Add(compileRunePos, spell.root);

            CompiledRune c;            

            do
            {
                c = compilationFront.Dequeue();

                for (int i = 0; i < 6; i++)
                {
                    var d = HexXY.neighbours[i];
                    var np = refPos + c.relPos + d;
                    Rune rune = (Rune)Level.S.GetEntities(np).FirstOrDefault(ent => ent is Rune);
                    if (rune == null) continue;

                    CompiledRune nrune;
                    if (compilationMap.TryGetValue(np, out nrune))
                    {                        
                        int reverseIdx = (i + 3) % 6;
                        c.neighs[i] = nrune;
                        c.neighsListIdxs[i] = (int)nrune.listIdx;
                        nrune.neighs[reverseIdx] = c;
                        nrune.neighsListIdxs[reverseIdx] = (int)c.listIdx;
                    }
                    else
                    {
                        nrune = new CompiledRune((RuneType)rune.entityType, c.relPos + d, rune.dir, (uint)spell.allRunes.Count);
                        spell.allRunes.Add(nrune);
                        compilationFront.Enqueue(nrune);
                        compilationMap.Add(np, nrune);
                    }
                }
            } while (compilationFront.Count > 0);

            //foreach (var r in spell.allRunes)
            //  Logger.Log(r.relPos + " " + r.type);            

            return spell;
        }

        public void Save(System.IO.BinaryWriter writer)
        {
            writer.Write((uint)allRunes.Count);
            for (int i = 0; i < allRunes.Count; i++)
            {
                var rune = allRunes[i];
                writer.Write((uint)rune.type);
                writer.Write(rune.relPos.x);
                writer.Write(rune.relPos.y);
                writer.Write(rune.dir);                
            }

            for (int i = 0; i < allRunes.Count; i++)
            {
                var rune = allRunes[i];
                for (int k = 0; k < 6; k++)
                    writer.Write(rune.neighsListIdxs[k]);
            }
        }

        public static Spell Load(System.IO.BinaryReader reader)
        {
            Spell spell = new Spell();
            uint runeCount = reader.ReadUInt32();
            for (uint i = 0; i < runeCount; i++)
            {
                var rune = new CompiledRune(
                    (RuneType)reader.ReadUInt32(),
                    new HexXY(reader.ReadInt32(), reader.ReadInt32()),
                    reader.ReadUInt32(), i);

                if (i == 0)
                    spell.root = rune;

                spell.allRunes.Add(rune);
            }

            for (int i = 0; i < runeCount; i++)
            {
                var rune = spell.allRunes[i];
                for (int k = 0; k < 6; k++)
                {
                    int idx = reader.ReadInt32();
                    if (idx != -1)
                    {
                        rune.neighsListIdxs[k] = idx;
                        rune.neighs[k] = spell.allRunes[idx];
                    } 
                }
            }

            return spell;
        }

        public void RedrawOnGround(CompiledRune rune, HexXY refPos)
        {
            HexXY pos = refPos + rune.relPos;
            if (Level.S.GetEntities(pos).Any(e => e is Rune)) return;

            Rune groundRune = new Rune(rune.type, rune.dir);            
            groundRune.Spawn(pos);

            for (int i = 0; i < 6; i++)
            {
                var n = rune.neighs[i];
                if (n != null)                
                    RedrawOnGround(n, refPos);                
            }
        }
    }
}
