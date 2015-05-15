using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    //Drops blocking stones from above
    public class AvatarStone : IAvatarElement
    {
        public IAvatarElement Clone()
        {
            return new AvatarStone();
        }
    }
}
