using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    [GameSaveLoad("02FB581A")]
    public class StatueTeach : Statue, IFibered
    {
        Spell compilingSpell;
        List<Rune> runeLightSeq = new List<Rune>();

        public StatueTeach() : base()
        {
            
        }

        public override bool CanBeClicked { get { return E.player.selectedAbilitySpell != -1 && !isCasting; } }

        public override void Click()
        {            
            isCasting = true;
            E.player.cantMove = true;

            var compileRune = Level.S.GetEntities(sourceSpellPos).OfType<Rune>().First();
            var spell = new Spell();
            runeLightSeq.Clear();
           
            spell.Compile(compileRune, compileRune.pos, runeLightSeq);            
            E.player.abilitySpells[E.player.selectedAbilitySpell] = spell;
            E.player.cantMove = true;
            E.player.selectedAbilitySpell = -1;

            compilingSpell = spell;
            fibered.StartFiber(RuneLightFib());
        }        

        IEnumerator<float> RuneLightFib()
        {
            foreach (var rune in runeLightSeq)
            {
                if (rune == null) yield return 0.025f;
                else
                {
                    rune.isLit = true;
                    fibered.StartFiber(RuneQuenchFib(rune));
                }
            }

            yield return 0.1f;
            isCasting = false;
            E.player.cantMove = false;
        }

        IEnumerator<float> RuneQuenchFib(Rune rune)
        {
            yield return UnityEngine.Random.Range(0.1f, 0.15f);
            rune.isLit = false;
        }
    }
}
