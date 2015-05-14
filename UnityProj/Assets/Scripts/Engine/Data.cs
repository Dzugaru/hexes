using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public struct MobData
    {
        public readonly CharacterType characterType;
        public readonly float maxHP;
        public readonly float speed;
        public readonly float attackDmgAppDelay;
        public readonly float attackDur;
        public readonly float attackDamage;

        public MobData(CharacterType characterType, float maxHP, float speed, float attackDmgAppDelay, float attackDur, float attackDamage)
        {
            this.characterType = characterType;
            this.maxHP = maxHP;
            this.speed = speed;
            this.attackDamage = attackDamage;
            this.attackDmgAppDelay = attackDmgAppDelay;
            this.attackDur = attackDur;
        }
    }

    public struct SpellData
    {
        public readonly float launchTime;

        public SpellData(float launchTime)
        {
            this.launchTime = launchTime;
        }
    }

    public struct RuneData
    {
        public readonly bool isDirectional;

        public RuneData(bool isDirectional)
        {
            this.isDirectional = isDirectional;
        }
    }

    public static class Data
    {
        public static readonly Dictionary<string, MobData> mobDatas = new Dictionary<string, MobData>();

        //TODO: make a complex key (SpellType, Level)?
        public static readonly Dictionary<SpellType, SpellData> spellDatas = new Dictionary<SpellType, SpellData>();
        public static readonly Dictionary<RuneType, RuneData> runeDatas = new Dictionary<RuneType, RuneData>();

        static Data()
        {
            //											Graphics				HP		Speed	DmgDelay	AtkDuration		AtkDmg
            mobDatas["spider"] = new MobData(CharacterType.Spider, 10, 1, 0.5f, 1.0f, 1);

            //
            spellDatas[SpellType.LineOfFire] = new SpellData(0.25f);
            spellDatas[SpellType.ColdCircle] = new SpellData(1.0f);
            spellDatas[SpellType.FireTurret] = new SpellData(2.0f);

            //													        Directional?
            runeDatas[RuneType.Compile] =                   new RuneData(false);

            runeDatas[RuneType.Arrow0] =                    new RuneData(true);
            runeDatas[RuneType.ArrowL60] =                  new RuneData(true);
            runeDatas[RuneType.ArrowL120] =                 new RuneData(true);
            runeDatas[RuneType.ArrowR60] =                  new RuneData(true);
            runeDatas[RuneType.ArrowR120] =                 new RuneData(true);
            runeDatas[RuneType.ArrowCross] =                new RuneData(true);

            runeDatas[RuneType.Avatar] =                    new RuneData(false);
            runeDatas[RuneType.AvatarWalkDir] =             new RuneData(true);
            runeDatas[RuneType.AvatarWalkDirDraw] =         new RuneData(true);
            runeDatas[RuneType.AvatarForward] =             new RuneData(false);
            runeDatas[RuneType.AvatarForwardDraw] =         new RuneData(false);
            runeDatas[RuneType.AvatarForwardDupDraw] =      new RuneData(false);
            runeDatas[RuneType.AvatarLeft] =                new RuneData(false);
            runeDatas[RuneType.AvatarRight] =               new RuneData(false);

            runeDatas[RuneType.If] =                        new RuneData(true);
            runeDatas[RuneType.PredicateAvatarRef] =        new RuneData(false);
            runeDatas[RuneType.PredicateTileEmpty] =        new RuneData(false);
            runeDatas[RuneType.PredicateTileWall] =         new RuneData(false);
            runeDatas[RuneType.PredicateTileMonster] =      new RuneData(false);

            runeDatas[RuneType.Flame] =                     new RuneData(false);
            runeDatas[RuneType.Stone] =                     new RuneData(false);
            runeDatas[RuneType.Wind] =                      new RuneData(false);

            runeDatas[RuneType.Number2] =                   new RuneData(false);
            runeDatas[RuneType.Number3] =                   new RuneData(false);
            runeDatas[RuneType.Number4] =                   new RuneData(false);
            runeDatas[RuneType.Number5] =                   new RuneData(false);
            runeDatas[RuneType.Number6] =                   new RuneData(false);
            runeDatas[RuneType.Number7] =                   new RuneData(false);
        }
    }
}
