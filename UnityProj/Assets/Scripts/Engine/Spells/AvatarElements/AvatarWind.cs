using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class AvatarWind : IAvatarElement
    {
        public Avatar avatar;

        public AvatarWind(Avatar avatar)
        {
            this.avatar = avatar;
        }

        public void CopyTo(Avatar avatar)
        {
            avatar.avatarElement = new AvatarWind(avatar);
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {
            throw new NotImplementedException();
        }
    }
}
