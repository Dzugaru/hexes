using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine
{
    public class AvatarLearn : Entity, IAvatarElement, IRotatable, INonSavable
    {
        List<Rune> litRunes = new List<Rune>();        

        public Avatar avatar;
        public uint elementRuneIdx;

        float movTime, movTimeLeft;

        public bool CanRotate { get { return true; } }

        public float ForkManaCost
        {
            get
            {
                return 0;
            }
        }

        public AvatarLearn(Avatar avatar, uint elementRuneIdx, float movTime) : base(EntityClass.Character, (uint)CharacterType.AvatarLearn)
        {
            this.avatar = avatar;
            this.elementRuneIdx = elementRuneIdx;
            this.movTime = movTime;
        }

        public bool CanAvatarFork()
        {
            return avatar.spell.avatars.Count < 3;
            //return false;
        }

        public void ForkTo(Avatar to)
        {            
            to.avatarElement = new AvatarLearn(to, elementRuneIdx, movTime);
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {
            if (!WorldBlock.CanTryToMoveToBlockType(Level.S.GetPFBlockedMap(to)))
            {
                avatar.finishState = Avatar.FinishedState.CantMoveThere;
            }
            else
            {                
                Interfacing.PerformInterfaceMove(graphicsHandle, to, movTime);
                movTimeLeft = movTime * 0.75f; //TODO: this is point in time when avatar changes pos
            }
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
            foreach(var litRune in litRunes)    
                litRune.isLit = false;            
            base.Die();
        }

        

        public float OnInterpret(Spell.CompiledRune rune, List<Spell.CompiledRune> additionalRunes)
        {            
            foreach(var litRune in litRunes)
                litRune.isLit = false;
            litRunes.Clear();

            var mainLitRune = avatar.spell.compiledSpell.GetRealRune(rune);
            litRunes.Add(mainLitRune);
            foreach (var arune in additionalRunes)
                litRunes.Add(avatar.spell.compiledSpell.GetRealRune(arune));

            foreach (var litRune in litRunes)
                litRune.isLit = true;

            float interTime;

            if (rune.type == RuneType.Wind)
                interTime = 2;
            else if (Avatar.IsArrowRune(rune.type))
                interTime = 0.25f;
            else if (Avatar.IsMovementCommandRune(rune.type))
                interTime = movTime;
            else
                interTime = 1;

            return interTime;// * 0.1f;
        }

        public override void Update(float dt)
        {
            if (movTimeLeft > 0)
            {
                movTimeLeft = Mathf.Max(0, movTimeLeft - dt);
                if (movTimeLeft == 0)
                {
                    Level.S.RemoveEntity(pos, this);                    
                    pos = avatar.pos;                    
                    Level.S.AddEntity(pos, this);
                }
            }
            base.Update(dt);
        }
    }
}
