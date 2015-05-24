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
        public static Player player;
        public static LevelScript levelScript;  
        

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
