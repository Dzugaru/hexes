using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    [GameSaveLoad("00000003")]
    public class StatueCaster : Statue
    {
        public SpellExecuting exSpell;

        public override bool CanBeClicked { get { return true; } }

        public StatueCaster() : base()
        {
            entityType = (uint)MechType.StatueCaster;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (exSpell != null && !exSpell.isExecuting)
                exSpell = null;

            isCasting = exSpell != null;
        }

        public override void Click()
        {
            if (!isCasting)
            {
                var compileRune = Level.S.GetEntities(sourceSpellPos).OfType<Rune>().First();

                var spell = new Spell();
                spell.Compile(compileRune, sourceSpellPos);
                exSpell = spell.CastMelee(this, dir, true);
            }
            else
            {
                exSpell.Die();
                exSpell = null;
            }
        }        
    }
}
