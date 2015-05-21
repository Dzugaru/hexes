using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public static class Interfacing
    {
        public struct EntityHandle
        {           
	        public EntityClass objClass;
            public uint idx;
        }

        public static Func<EntityClass, uint, EntityHandle> CreateEntity;

        public static Action<EntityHandle, HexXY> PerformInterfaceSpawn;
        public static Action<EntityHandle, HexXY, float> PerformInterfaceMove;
        public static Action<EntityHandle, HexXY> PerformInterfaceStop;
        public static Action<EntityHandle, HexXY> PerformInterfaceAttack;
        public static Action<EntityHandle, float> PerformInterfaceDamage;        
        public static Action<EntityHandle> PerformInterfaceDie;

        public static Action<EntityHandle, float, float> PerformInterfaceUpdateHP;
        public static Action<EntityHandle, uint> PerformInterfaceUpdateRotation;
        public static Action<EntityHandle, float> PerformInterfaceUpdateSpellEffect;


    }
}
