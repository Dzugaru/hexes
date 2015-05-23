using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine
{
    public class Door : Entity
    {
        public ScriptObjectID id;
        public bool isOpen;        

        public Door() : base(EntityClass.Mech, (uint)MechType.Door)
        {

        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write((byte)DerivedTypes.Door);

            writer.Write(id);
            writer.Write(isOpen);
        }

        public new static Door Load(BinaryReader reader)
        {
            return new Door()
            {
                id = (ScriptObjectID)reader.ReadInt32(),
                isOpen = reader.ReadBoolean()
            };
        }

        public override void Update(float dt)
        {            
            
        }
    }
}

