﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class AvatarFlame : IAvatarElement
    {
        public IAvatarElement Clone()
        {
            return new AvatarFlame();
        }
    }
}
