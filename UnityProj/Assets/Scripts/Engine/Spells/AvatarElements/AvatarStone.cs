using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    //Drops blocking stones from above
    public class AvatarStone : IAvatarElement
    {
        public Avatar avatar;
        public uint elementRuneIdx;

        public AvatarStone(Avatar avatar, uint elementRuneIdx)
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
            to.avatarElement = new AvatarStone(to, elementRuneIdx);
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {
            throw new NotImplementedException();
        }
    }
}
