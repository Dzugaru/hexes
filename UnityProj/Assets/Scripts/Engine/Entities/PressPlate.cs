using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine
{
    [GameSaveLoad("00000004")]
    public class PressPlate : Entity, IHasScriptID
    {
        public bool isPressed;
        public ScriptObjectID id;

        public ScriptObjectID ID
        {
            get
            {
                return id;
            }
        }

        public PressPlate() : base(EntityClass.Mech, (uint)MechType.AvatarWalkPressPlate)
        {

        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);           

            writer.Write(id);         
        }

        public override void LoadDerived(BinaryReader reader)
        {
            id = (ScriptObjectID)reader.ReadInt32();
        }

        public override void Update(float dt)
        {            
            if (!isPressed)
            {                
                var avLearn = Level.S.GetEntities(pos).OfType<AvatarLearn>().FirstOrDefault();
                if (avLearn != null)
                    isPressed = true;
            }
        }
    }
}

