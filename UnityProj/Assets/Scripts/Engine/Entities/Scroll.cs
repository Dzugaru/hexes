using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine
{
    [GameSaveLoad("00000006")]
    public class Scroll : Entity, IClickable
    {
        public string text;

        public bool CanBeClicked { get { return true; } }

        public Scroll() : base(EntityClass.Collectible, (uint)CollectibleType.Scroll)
        {

        }

        public Scroll(string text) : base(EntityClass.Collectible, (uint)CollectibleType.Scroll)
        {
            this.text = text;
        }
      
        public void Click()
        {
            //Die();
            Interfacing.ShowScrollWindow(this);            
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);            

            writer.Write(text);
        }

        public override void LoadDerived(BinaryReader reader)
        {
            this.text = reader.ReadString();
        }
    }
}
