using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public interface IAvatarElement
    {
       void CopyTo(Avatar avatar);
        void OnMove(HexXY from, HexXY to, bool isDrawing);
    }
}
