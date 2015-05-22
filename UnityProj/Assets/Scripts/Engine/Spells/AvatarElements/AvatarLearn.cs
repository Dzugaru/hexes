using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class AvatarLearn : Entity, IAvatarElement
    {
        public Avatar avatar;
        public uint elementRuneIdx;

        float speed;

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

        public void OnSpawn()
        {
            base.Spawn(avatar.pos);
        }

        public void OnDie()
        {
            Interfacing.PerformInterfaceLightOrQuenchRune(GetRealRune(avatar.rune).graphicsHandle, false);
            base.Die();
        }

        Rune GetRealRune(Spell.CompiledRune compRune)
        {
            return Level.S.GetEntities(avatar.spell.compiledSpell.realWorldStartRunePos + compRune.relPos).OfType<Rune>().First();
        }

        public float OnInterpret(Spell.CompiledRune rune)
        {            
            if (avatar.prevRune != null)
                Interfacing.PerformInterfaceLightOrQuenchRune(GetRealRune(avatar.prevRune).graphicsHandle, false);            
            
            Interfacing.PerformInterfaceLightOrQuenchRune(GetRealRune(rune).graphicsHandle, true);

            if (rune.type == RuneType.Flame)
                return 2;
            else
                return 1;
        }
    }
}
