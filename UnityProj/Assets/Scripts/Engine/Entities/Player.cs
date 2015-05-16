using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class Player : Entity, IWalker
    {
        public Spell currentSpell;

        //Spell castingSpell;
        //void spellFinishedCasting()
        //{
        //   castingSpell = null;
        //}

        //uint[EnumMembers!CollectibleType.length] gatheredCollectibles;

	    public Player() : base(EntityClass.Character, (uint)CharacterType.Player)
        {		    
            walker.SetSpeed(2);
        }

        public override void Update(float dt)
        {           
            //Collectible gather
            if (walker.isWalkBlocked)
            {
       //         foreach (ent; worldBlock.entityMap[path[0].x][path[0].y].els())
			    //{
       //             Collectible coll = cast(Collectible)ent;
       //             if (coll !is null)
				   // {
       //                 coll.die();
       //                 gatheredCollectibles[coll.entityType] += coll.amount;
       //             }
       //         }
            }

            //Update collectibles number in GUI
            //guiData.fGemsCount = gatheredCollectibles[CollectibleType.FireGem];

            base.Update(dt);
        }

        public bool DrawRune(RuneType type, HexXY p)
        {
            bool isSuccess = !walker.isWalking && Rune.CanDraw(this, type, p);
            if (isSuccess) Rune.DrawRune(this, type, p);
            return isSuccess;             
        }

        public bool EraseRune(HexXY p)
        {
            bool isSuccess = !walker.isWalking && Rune.CanErase(this, p);
            if (isSuccess) Rune.EraseRune(p);
            return isSuccess;            
        }

        public bool CompileSpell(HexXY p)
        {
            Rune compileRune =                                
                (Rune)WorldBlock.S.entityMap[p.x, p.y].FirstOrDefault(e => e is Rune && Avatar.IsAvatarElementRune((RuneType)e.entityType));

            bool isSuccess = compileRune != null;
            if (isSuccess) currentSpell = Spell.CompileSpell(compileRune, p);            
            return isSuccess;
        }

        public bool CastCurrentSpell(HexXY p)
        {
            bool isSuccess = currentSpell != null && HexXY.Dist(pos, p) == 1;
            if (isSuccess) currentSpell.Cast(this, (uint)HexXY.neighbours.IndexOf(p - pos));
            return isSuccess;            
        }       
    } 
}


