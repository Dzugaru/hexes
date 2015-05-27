using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine
{
    //Set ground it passes aflame
    public class AvatarFlame : IAvatarElement
    {
        public Avatar avatar;
        public uint elementRuneIdx;
        public float ForkManaCost { get; private set; }

        public AvatarFlame(Avatar avatar, uint elementRuneIdx)
        {
            this.avatar = avatar;
            this.elementRuneIdx = elementRuneIdx;
            this.ForkManaCost = 10;
        }

        public bool CanAvatarFork()
        {          
            return true;
        }

        public void ForkTo(Avatar to)
        {
            to.avatarElement = new AvatarFlame(to, elementRuneIdx);            
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {   
            if (!isDrawing || !WorldBlock.CanTryToMoveToBlockType(Level.S.GetPFBlockedMap(to)))
            {
                if (!avatar.spell.caster.SpendMana(1))
                {
                    avatar.finishState = Avatar.FinishedState.NoManaLeft;
                    return;
                }
            }
            else
            {
                if (!avatar.spell.caster.SpendMana(5))
                {
                    avatar.finishState = Avatar.FinishedState.NoManaLeft;
                    return;
                }

                var spellEffect = new SpellEffects.GroundFlame(1);
                spellEffect.StackOn(to);
            }
        }

        public void OnSpawn()
        {
            
        }

        public void OnDie()
        {
            
        }

        public void OnRotate(uint dir)
        {
            
        }

        public float OnInterpret(Spell.CompiledRune rune, List<Spell.CompiledRune> additionalRunes)
        {
            return 0;
        }
    }
}
