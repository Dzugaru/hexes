using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine
{
    //Drops blocking stones from above
    public class AvatarStone : IAvatarElement
    {
        public Avatar avatar;
        public uint elementRuneIdx;

        public float ForkManaCost
        {
            get
            {
                return 10;
            }
        }

        public AvatarStone(Avatar avatar, uint elementRuneIdx)
        {
            this.avatar = avatar;
            this.elementRuneIdx = elementRuneIdx;
        }

        public bool CanAvatarFork()
        {
            return true;
        }

        public void ForkTo(Avatar to)
        {            
            to.avatarElement = new AvatarStone(to, elementRuneIdx);
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
                if (!avatar.spell.caster.SpendMana(10))
                {
                    avatar.finishState = Avatar.FinishedState.NoManaLeft;
                    return;
                }

                var spellEffect = new SpellEffects.Stone(1);
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
