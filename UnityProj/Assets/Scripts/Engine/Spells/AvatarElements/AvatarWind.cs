using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class AvatarWind : IAvatarElement
    {
        public Avatar avatar;
        public uint elementRuneIdx;

        public AvatarWind(Avatar avatar, uint elementRuneIdx)
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
            to.avatarElement = new AvatarWind(to, elementRuneIdx);
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {
            throw new NotImplementedException();
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
