using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;


namespace Engine
{
    public static class E
    {
        public static Dictionary<uint, Type> saveLoadGuidToType = new Dictionary<uint, Type>();
        public static Dictionary<Type, uint> saveLoadTypeToGuid = new Dictionary<Type, uint>();

        public static Player player;
        public static LevelScript levelScript;

        static E()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                var saveLoadAttr = type.GetCustomAttributes(typeof(GameSaveLoadAttribute), false).OfType<GameSaveLoadAttribute>().FirstOrDefault();
                if (saveLoadAttr == null) continue;
                uint guidUInt = uint.Parse(saveLoadAttr.Guid, System.Globalization.NumberStyles.HexNumber);
                saveLoadGuidToType.Add(guidUInt, type);
                saveLoadTypeToGuid.Add(type, guidUInt);
            }
        }
        

        public static void LoadLevel(string levelName)
        {
            var levelDataAsset = (TextAsset)Resources.Load("Levels/" + levelName);
            using (var reader = new System.IO.BinaryReader(new System.IO.MemoryStream(levelDataAsset.bytes)))
            {
                Level.Load(reader);
                Level.S.LoadDynamicPart(reader);
            }

            levelScript = (LevelScript)Activator.CreateInstance(Assembly.GetExecutingAssembly().GetTypes().First(t => t.Namespace == "Engine.LevelScripts" && t.Name == levelName));
            levelScript.Start();

            //TODO: save/load player
            player.InitForNewGame();         
        }

        public static void Update(float dt)
        {
            foreach (var ent in Level.S.entityList)
                ent.Update(dt);

            Spell.Update(dt);
            levelScript.Update(dt);            
        }          
    }
}
