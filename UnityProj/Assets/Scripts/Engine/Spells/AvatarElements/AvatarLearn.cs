using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine
{
    public class AvatarLearn : Entity, IAvatarElement, IRotatable
    {
        Rune litRune;

        public Avatar avatar;
        public uint elementRuneIdx;

        float speed;

        public bool CanRotate { get { return true; } }

        public AvatarLearn(Avatar avatar, uint elementRuneIdx, float speed) : base(EntityClass.Character, (uint)CharacterType.AvatarLearn)
        {
            this.avatar = avatar;
            this.elementRuneIdx = elementRuneIdx;
            this.speed = speed;
        }

        public bool CanAvatarFork()
        {
            return true;
        }

        public void ForkTo(Avatar to)
        {            
            to.avatarElement = new AvatarLearn(to, elementRuneIdx, speed);
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {
            if (Level.S.GetPFBlockedMap(to) == WorldBlock.PFBlockType.StaticBlocked)            
                avatar.finishState = Avatar.FinishedState.CantMoveThere;            
            else            
                Interfacing.PerformInterfaceMove(graphicsHandle, to, 1 / speed);            
        }

        public void OnRotate(uint dir)
        {
            Interfacing.PerformInterfaceUpdateRotation(graphicsHandle, dir);
        }

        public void OnSpawn()
        {
            base.Spawn(avatar.pos);
        }

        public void OnDie()
        {           
            litRune.isLit = false;            
            base.Die();
        }

        Rune GetRealRune(Spell.CompiledRune compRune)
        {
            return Level.S.GetEntities(avatar.spell.compiledSpell.realWorldStartRunePos + compRune.relPos).OfType<Rune>().First();
        }

        public float OnInterpret(Spell.CompiledRune rune)
        {            
            if (litRune != null)
                litRune.isLit = false;

            litRune = GetRealRune(rune);
            litRune.isLit = true;             

            if (rune.type == RuneType.Flame)
                return 2;
            else
                return 1;
        }
    }
}
