using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class Avatar
    {
        const bool isLogging = true;

        public uint id;
        public Spell spell;

        public HexXY pos;
        public uint dir;
        public Spell.CompiledRune rune;
        public float timeLeft;
        public FinishedState? error;

        public enum FinishedState
        {
            DiedCauseTooWeak,
            RuneIsNull
        }

        public Avatar(Spell spell, uint id, HexXY pos, uint dir, Spell.CompiledRune startRune)
        {
            this.spell = spell;
            this.id = id;
            this.pos = pos;
            this.dir = dir;
            this.rune = startRune;
            this.timeLeft = 0;
        }

        public void Interpret()
        {
            if (rune == null)
            {
                error = FinishedState.RuneIsNull;
                return;
            }

            if (isLogging)            
                Logger.Log(id + "> interprets " + rune.type + " at " + rune.relPos);            

            switch (rune.type)
            {
                case RuneType.Compile:
                    InterpretSplit(); break;

                case RuneType.Arrow0:
                case RuneType.ArrowL120:
                case RuneType.ArrowL60:
                case RuneType.ArrowR120:
                case RuneType.ArrowR60:
                case RuneType.ArrowCross:
                    InterpretArrow(); break;

            }
        }

        void InterpretSplit()
        {
            rune = rune.neighs.FirstOrDefault(r => r != null);

            //TODO: split
        }

        void InterpretArrow()
        {
            uint fromDir = rune.dir;

            uint dirChange;
            switch (rune.type)
            {
                case RuneType.Arrow0:
                case RuneType.ArrowCross:
                    dirChange = 0;
                    break;

                case RuneType.ArrowR60: dirChange = 1; break;
                case RuneType.ArrowR120: dirChange = 2; break;
                case RuneType.ArrowL120: dirChange = 4; break;
                case RuneType.ArrowL60: dirChange = 5; break;
                default: throw new Tools.AssertException();
            }

            uint toDir = (fromDir + dirChange) % 6;

            rune = rune.neighs[toDir];

            //Checking runes that forbid entering from arbitrary direction
            if (rune != null && rune.type == RuneType.ArrowCross)
            {
                if (toDir != rune.dir &&
                    (toDir + 5) % 6 != rune.dir)
                {
                    rune = null;
                }
            }
        }
    }
}
