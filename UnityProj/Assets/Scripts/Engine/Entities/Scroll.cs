using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine
{
    public class Scroll : Entity, IClickable
    {
        public string text;

        public Scroll(string text) : base(EntityClass.Collectible, (uint)CollectibleType.Scroll)
        {
            this.text = text;
        }
      
        public void Click()
        {
            Logger.Log(text);
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write((byte)DerivedTypes.Scroll);

            writer.Write(text);
        }

        public new static Scroll Load(BinaryReader reader)
        {
            return new Scroll(reader.ReadString());
        }
    }
}
