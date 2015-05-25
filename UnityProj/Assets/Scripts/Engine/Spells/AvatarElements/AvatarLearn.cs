using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine
{
    public class AvatarLearn : Entity, IAvatarElement, IRotatable
    {
        List<Rune> litRunes = new List<Rune>();
        float movementTimeLeft;

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
            return avatar.spell.avatars.Count < 3;
            //return false;
        }

        public void ForkTo(Avatar to)
        {            
            to.avatarElement = new AvatarLearn(to, elementRuneIdx, speed);
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {
            if (!WorldBlock.CanTryToMoveToBlockType(Level.S.GetPFBlockedMap(to)))
            {
                avatar.finishState = Avatar.FinishedState.CantMoveThere;
            }
            else
            {
                float time = 1 / speed;
                Interfacing.PerformInterfaceMove(graphicsHandle, to, time);
                movementTimeLeft = time * 0.75f; //TODO: this is point in time when avatar changes pos and pressplates etc. react
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

        Rune GetRealRune(Spell.CompiledRune compRune)
        {
            return Level.S.GetEntities(avatar.spell.compiledSpell.realWorldStartRunePos + compRune.relPos).OfType<Rune>().First();
        }

        public float OnInterpret(Spell.CompiledRune rune, List<Spell.CompiledRune> additionalRunes)
        {            
            foreach(var litRune in litRunes)
                litRune.isLit = false;
            litRunes.Clear();

            var mainLitRune = GetRealRune(rune);
            litRunes.Add(mainLitRune);
            foreach (var arune in additionalRunes)
                litRunes.Add(GetRealRune(arune));

            foreach (var litRune in litRunes)
                litRune.isLit = true;

            float interTime;       

            if (rune.type == RuneType.Wind)
                interTime = 2;
            else if (Avatar.IsArrowRune(rune.type))
                interTime = 0.25f;
            else
                interTime = 1;

            return interTime * 0.1f;
        }

        public override void Update(float dt)
        {
            if (movementTimeLeft > 0)
            {
                movementTimeLeft = Mathf.Max(0, movementTimeLeft - dt);
                if (movementTimeLeft == 0)
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
