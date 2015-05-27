using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    [GameSaveLoad("02FB581A")]
    public class StatueTeach : Statue, IFibered
    {       
        public StatueTeach() : base()
        {
            
        }       

        public override void Click()
        {            
            fibered.StartFiber(TeachFib());
        }

        IEnumerator<float> TeachFib()
        {
            isCasting = true;
            E.player.cantMove = true;
            yield return 2;            
            isCasting = false;
            var compileRune = Level.S.GetEntities(sourceSpellPos).OfType<Rune>().First();
            var spell = Spell.CompileSpell(compileRune, compileRune.pos);
            E.player.abilitySpell = spell;
            E.player.cantMove = false;
        }
    }
}
