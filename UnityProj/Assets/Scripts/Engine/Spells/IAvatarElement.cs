using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public interface IAvatarElement
    {        
        void OnMove(HexXY from, HexXY to, bool isDrawing);
        bool CanAvatarFork();
        void ForkTo(Avatar to);
    }
}
