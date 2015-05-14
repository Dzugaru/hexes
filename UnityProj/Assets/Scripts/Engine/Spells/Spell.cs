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
        public List<Avatar> avatars = new List<Avatar>();

        public bool isRunning = false;

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
            Avatar avatar = new Avatar(this, 0, ent.pos, dir, root);
            avatars.Add(avatar);
            isRunning = true;
        }

        public void Update(float dt)
        {
            for (int i = 0; i < avatars.Count; i++)
            {
                var av = avatars[i];
                av.timeLeft -= dt;
                if (av.timeLeft > 0) continue;
                av.Interpret();
                if (av.error != null)                
                    avatars.RemoveAt(i--);                
            }

            if (avatars.Count == 0)            
                isRunning = false;
        }

        static Queue<CompiledRune> compilationFront = new Queue<CompiledRune>();

        public static Spell CompileSpell(Rune compileRune, HexXY compileRunePos)
        {
            Spell spell = new Spell();

            HexXY refPos = compileRunePos;
            spell.root = new CompiledRune((RuneType)compileRune.entityType, new HexXY(0, 0), compileRune.dir, 0);
            spell.allRunes.Add(spell.root);

            compilationFront.Clear();
            compilationFront.Enqueue(spell.root);

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
                                                
                        var nrune = new CompiledRune((RuneType)rune.entityType, c.relPos + d, rune.dir, (uint)spell.allRunes.Count);
                        spell.allRunes.Add(nrune);
                        int reverseIdx = (i + 3) % 6;

                        c.neighs[i] = nrune;
                        c.neighsListIdxs[i] = (int)nrune.listIdx;                        
                        nrune.neighs[reverseIdx] = c;
                        nrune.neighsListIdxs[reverseIdx] = (int)c.listIdx;

                        compilationFront.Enqueue(nrune);                        
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
            if (WorldBlock.S.entityMap[pos.x, pos.y].Any(e => e is Rune)) return;

            Rune groundRune = Freelist<Rune>.Allocate();
            groundRune.Construct(rune.type, rune.dir);
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
