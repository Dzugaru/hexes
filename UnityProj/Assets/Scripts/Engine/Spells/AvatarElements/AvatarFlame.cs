using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine
{
    //Set ground it passes aflame
    public class AvatarFlame : Entity, IAvatarElement, IRotatable, INonSavable
    {
        public Avatar avatar;
        public uint elementRuneIdx;
        public float ForkManaCost { get; private set; }
        
        float movTime, movTimeLeft;
        HexXY movPos;
        bool isDrawing;        

        public bool CanRotate
        {
            get
            {
                return true;
            }
        }

        public AvatarFlame(Avatar avatar, uint elementRuneIdx, float movTime) : base(EntityClass.Character, (uint)CharacterType.AvatarFlame)
        {
            this.avatar = avatar;
            this.elementRuneIdx = elementRuneIdx;
            this.ForkManaCost = 10;
            this.movTime = movTime;
        }

        public bool CanAvatarFork()
        {          
            return true;
        }

        public void ForkTo(Avatar to)
        {
            to.avatarElement = new AvatarFlame(to, elementRuneIdx, movTime);
        }

        void EnsurePrevMoveFinished()
        {
            if (movTimeLeft > 0) //Didn't finish prev move cause of lag? fix it
            {
                OnMoveFinish(movPos);
                movTimeLeft = 0;
            }
        }

        void OnMoveFinish(HexXY to)
        {
            Level.S.RemoveEntity(pos, this);
            pos = to;
            Level.S.AddEntity(pos, this);

            if (isDrawing)
            {
                var spellEffect = new SpellEffects.GroundFlame(1);
                spellEffect.StackOn(pos);
            }
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {
            EnsurePrevMoveFinished();

            if (!WorldBlock.CanTryToMoveToBlockType(Level.S.GetPFBlockedMap(to)))
            {
                avatar.finishState = Avatar.FinishedState.CantMoveThere;
                return;
            }

            if (!isDrawing)
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
            }

            this.isDrawing = isDrawing;

            Interfacing.PerformInterfaceMove(graphicsHandle, to, movTime);
            movTimeLeft = movTime * 0.75f; //TODO: this is point in time when avatar element entity changes pos
            movPos = to;
        }

        public void OnSpawn()
        {
            base.Spawn(avatar.pos);
        }

        public void OnDie()
        {
            EnsurePrevMoveFinished();  
            base.Die();
        }

        public void OnRotate(uint dir)
        {
            Interfacing.PerformInterfaceUpdateRotation(graphicsHandle, dir);
        }

        public override void Update(float dt)
        {
            if (movTimeLeft > 0)
            {
                movTimeLeft = Mathf.Max(0, movTimeLeft - dt);
                if (movTimeLeft == 0)
                    OnMoveFinish(movPos);                
            }
            base.Update(dt);
        }       

        public float OnInterpret(Spell.CompiledRune rune, List<Spell.CompiledRune> additionalRunes)
        {
            if (Avatar.IsMovementCommandRune(rune.type))
                return movTime;
            else
                return 0;
        }
    }
}
