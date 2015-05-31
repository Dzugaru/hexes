using System;
using System.Collections.Generic;
using System.IO;
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

        public static string levelName;

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
            E.levelName = levelName;
            var levelDataAsset = (TextAsset)Resources.Load("Levels/" + levelName);
            string savePath = Application.persistentDataPath + "/Save/save_";            

            using (var reader = new BinaryReader(new MemoryStream(levelDataAsset.bytes)))
            {
                Level.Load(reader);               
                if (File.Exists(savePath + levelName))
                {
                    using (var saveReader = new BinaryReader(File.OpenRead(savePath + levelName)))
                    {
                        Level.S.LoadDynamicPart(saveReader);
                    }
                }
                else
                {
                    Level.S.LoadDynamicPart(reader);
                }
            }

            //TODO: save/load level script too
            levelScript = (LevelScript)Activator.CreateInstance(Assembly.GetExecutingAssembly().GetTypes().First(t => t.Namespace == "Engine.LevelScripts" && t.Name == levelName));
            levelScript.Start();

            if (File.Exists(savePath + "player"))
            {
                using (var saveReader = new BinaryReader(File.OpenRead(savePath + "player")))
                {
                    player = Player.Load(saveReader);
                    player.Spawn(player.pos);
                }
            }
            else
            {
                player = new Player();
                player.InitForNewGame();
                player.Spawn(levelScript.PlayerSpawnPos);
            } 
        }

        public static void Update(float dt)
        {
            foreach (var ent in Level.S.entityList)
                ent.Update(dt);

            Spell.Update(dt);
            levelScript.Update(dt);            
        }

        public static void SaveGame()
        {            
            string savePath = Application.persistentDataPath + "/Save/";
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            savePath += "save_";            

            using (var writer = new BinaryWriter(File.OpenWrite(savePath + levelName)))
            {
                Level.S.SaveDynamicPart(writer);
            }

            using (var writer = new BinaryWriter(File.OpenWrite(savePath + "player")))
            {
                player.Save(writer);
            }
        }
    }
}
