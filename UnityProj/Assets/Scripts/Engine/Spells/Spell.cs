using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Engine
{
    public class Spell
    {
        public HexXY realWorldStartRunePos;
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

        public SpellExecuting CastMelee(ICaster caster, uint dir, bool lockRealRunes)
        {
            var pos = ((Entity)caster).pos;
            var spEx = new SpellExecuting(caster, this, pos, dir, lockRealRunes);
            spEx.SpawnAvatar(root, null, pos, dir); //TODO: spawn at spell target? what to do with direction? make random if far?
            executingSpells.Add(spEx);           
            return spEx;
        }

        public SpellExecuting CastRanged(ICaster caster, HexXY target)
        {
            var pos = ((Entity)caster).pos;
            var dist = HexXY.Dist(pos, target);
            uint dir;
            if (dist == 0)
                dir = ((Entity)caster).dir;
            else
                dir = HexXY.GetApproximateDir(pos, target);
             

            var spEx = new SpellExecuting(caster, this, target, dir, false);
            spEx.SpawnAvatar(root, null, target, dir); 
            executingSpells.Add(spEx);
            return spEx;
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
        public void Compile(Rune compileRune, HexXY compileRunePos, List<Rune> visSeq = null)
        {
            HexXY refPos = realWorldStartRunePos = compileRunePos;
            root = new CompiledRune((RuneType)compileRune.entityType, new HexXY(0, 0), compileRune.dir, 0);
            allRunes.Add(root);

            compilationFront.Clear();
            compilationMap.Clear();
            compilationFront.Enqueue(root);
            compilationMap.Add(compileRunePos, root);

            //Nice visualization support included
           
            uint compileNextBatchCount = 0;
            uint compileBatchCount = 1;

            CompiledRune c;            

            do
            {                
                c = compilationFront.Dequeue();
                if (visSeq != null) visSeq.Add(GetRealRune(c));

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
                        nrune = new CompiledRune((RuneType)rune.entityType, c.relPos + d, rune.dir, (uint)allRunes.Count);
                        allRunes.Add(nrune);
                        compilationFront.Enqueue(nrune);                        
                        compilationMap.Add(np, nrune);
                        compileNextBatchCount++;
                    }
                }

                compileBatchCount--;
                if (compileBatchCount == 0)
                {
                    compileBatchCount = compileNextBatchCount;
                    compileNextBatchCount = 0;
                    if (visSeq != null) visSeq.Add(null); //Pause
                }

            } while (compilationFront.Count > 0);

            //foreach (var r in spell.allRunes)
            //  Logger.Log(r.relPos + " " + r.type);                        
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

            Rune groundRune = new Rune(rune.dir);
            groundRune.entityType = (uint)rune.type;
            groundRune.Spawn(pos);

            for (int i = 0; i < 6; i++)
            {
                var n = rune.neighs[i];
                if (n != null)                
                    RedrawOnGround(n, refPos);                
            }
        }

        public Rune GetRealRune(CompiledRune compRune)
        {
            return Level.S.GetEntities(realWorldStartRunePos + compRune.relPos).OfType<Rune>().First();
        }      
    }
}
