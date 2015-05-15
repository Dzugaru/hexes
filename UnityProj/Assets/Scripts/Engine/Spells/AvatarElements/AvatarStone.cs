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

        public AvatarStone(Avatar avatar)
        {
            this.avatar = avatar;
        }

        public void CopyTo(Avatar avatar)
        {
            avatar.avatarElement = new AvatarStone(avatar);
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {
            throw new NotImplementedException();
        }
    }
}
