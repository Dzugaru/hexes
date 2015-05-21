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

        public AvatarLearn(Avatar avatar, uint elementRuneIdx) : base(EntityClass.Character, (uint)CharacterType.AvatarLearn)
        {
            this.avatar = avatar;
            this.elementRuneIdx = elementRuneIdx;
        }

        public bool CanAvatarFork()
        {
            return false;
        }

        public void ForkTo(Avatar to)
        {
            throw new NotImplementedException();
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {
            
        }
    }
}
