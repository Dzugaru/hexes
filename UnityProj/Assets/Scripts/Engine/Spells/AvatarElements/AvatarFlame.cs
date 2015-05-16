using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    //Set ground it passes aflame
    public class AvatarFlame : IAvatarElement
    {
        public Avatar avatar;

        public AvatarFlame(Avatar avatar)
        {
            this.avatar = avatar;
        }

        public void CopyTo(Avatar avatar)
        {
            avatar.avatarElement = new AvatarFlame(avatar);
        }

        public void OnMove(HexXY from, HexXY to, bool isDrawing)
        {
            //Logger.Log("Flame moved from " + from + " to " + to + " isDrawing " + isDrawing);
            if (isDrawing)
            {
                SpawnFlameIfNotPresent(from);
                SpawnFlameIfNotPresent(to);
            }
        }

        //TODO: manage it by stack logic in SpellEffect!
        void SpawnFlameIfNotPresent(HexXY pos)
        {
            if (pos == avatar.spell.refPos) return; //Don't kill the caster, lol

            bool isSomeEffectPresent = WorldBlock.S.entityMap[pos.x, pos.y].Any(e => e is SpellEffect);
            if (isSomeEffectPresent) return;

            var spellEffect = Freelist<SpellEffects.GroundFlame>.Allocate();
            spellEffect.Construct();
            spellEffect.Spawn(pos);
        }
    }
}
