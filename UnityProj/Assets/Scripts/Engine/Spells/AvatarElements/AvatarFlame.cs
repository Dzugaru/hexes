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

        public AvatarFlame(Avatar avatar, uint elementRuneIdx)
        {
            this.avatar = avatar;
            this.elementRuneIdx = elementRuneIdx;
        }

        public bool CanAvatarFork()
        {          
            return avatar.spell.GetElementalPower(elementRuneIdx) > 0;
        }

        public void ForkTo(Avatar to)
        {
            avatar.spell.UseElementalPower(elementRuneIdx, 0.1f);
            to.avatarElement = new AvatarFlame(to, elementRuneIdx);            
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {
            float powerLeft = avatar.spell.GetElementalPower(elementRuneIdx);
            if (powerLeft == 0)
            {
                avatar.finishState = Avatar.FinishedState.DiedCauseTooWeak;
                return;
            }

            if (!isDrawing || Level.S.GetPFBlockedMap(to) == WorldBlock.PFBlockType.StaticBlocked)
            {
                avatar.spell.UseElementalPower(elementRuneIdx, 0.01f);
            }
            else
            {               
                float powerMax = 0.1f;
                float powerLowThresh = 0.2f;
                float powerUse = powerLeft > powerLowThresh ?
                    powerMax :
                    Mathf.Lerp(powerMax, powerMax * 0.1f, (powerLowThresh - powerLeft) / powerLowThresh);

                avatar.spell.UseElementalPower(elementRuneIdx, powerUse);

                var spellEffect = new SpellEffects.GroundFlame(powerUse / powerMax);                
                spellEffect.StackOn(to);
            }
        }

        public void OnSpawn()
        {
            
        }

        public void OnDie()
        {
            
        }

        public float OnInterpret(Spell.CompiledRune rune)
        {
            return 0;
        }

        public void OnRotate(uint dir)
        {
            
        }
    }
}
