using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    //Set ground it passes aflame
    public class AvatarFlame : IAvatarElement
    {
        public IAvatarElement Clone()
        {
            return new AvatarFlame();
        }
    }
}
