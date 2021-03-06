﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public interface IAvatarElement
    {        
        void OnMove(HexXY from, HexXY to, bool isDrawing); //TODO: make this virtual with default "power" implementation?
        void OnRotate(uint dir);
        void OnSpawn();
        void OnDie();
        bool CanAvatarFork();
        void ForkTo(Avatar to);
        float OnInterpret(Spell.CompiledRune rune, List<Spell.CompiledRune> additionalRunes);
        float ForkManaCost { get; }
    }
}
