using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public interface ICaster
    {
        float Mana { get; }
        bool SpendMana(float amount);
    }
}
