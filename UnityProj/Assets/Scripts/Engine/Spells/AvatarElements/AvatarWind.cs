using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class AvatarWind : IAvatarElement
    {
        public Avatar avatar;
        public uint elementRuneIdx;

        public AvatarWind(Avatar avatar, uint elementRuneIdx)
        {
            this.avatar = avatar;
            this.elementRuneIdx = elementRuneIdx;
        }

        public bool CanAvatarFork()
        {
            return avatar.spell.caster.Mana > 10;
        }

        public void ForkTo(Avatar to)
        {
            avatar.spell.caster.SpendMana(10);
            to.avatarElement = new AvatarWind(to, elementRuneIdx);
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {
            throw new NotImplementedException();
        }

        public void OnSpawn()
        {

        }

        public void OnDie()
        {

        }

        public void OnRotate(uint dir)
        {

        }

        public float OnInterpret(Spell.CompiledRune rune, List<Spell.CompiledRune> additionalRunes)
        {
            return 0;
        }
    }
}
