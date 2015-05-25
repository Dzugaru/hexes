using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine
{
    public class Door : Entity, IFibered, IClickable, IHasScriptID
    {
        Fibered.Fiber changingState;

        public ScriptObjectID id;
        public bool isOpen;      

        public ScriptObjectID ID
        {
            get
            {
                return id;
            }
        }

        public Door() : base(EntityClass.Mech, (uint)MechType.Door)
        {

        }

        public override void Spawn(HexXY p)
        {
            base.Spawn(p);
            if(!G.S.isEditMode) if (!isOpen) Level.S.SetPFBlockedMap(p, WorldBlock.PFBlockType.DoorBlocked);
        }

        public override void Die()
        {
            if(!G.S.isEditMode) if (!isOpen) Level.S.SetPFBlockedMap(pos, WorldBlock.PFBlockType.Unblocked);
            base.Die();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }

        public void OpenOrClose(bool open)
        {
            if (isOpen == open) return;
            
            if (changingState != null && changingState.isDead)
                fibered.StopFiber(changingState);

            isOpen = open;
            changingState = fibered.StartFiber(ChangeStateFib());
        }

        IEnumerator<float> ChangeStateFib()
        {
            yield return (isOpen ? 0.5f : 0.25f);
            if (!isOpen && Level.S.GetPFBlockedMap(pos) == WorldBlock.PFBlockType.DynamicBlocked)
            {
                //Someone blocked the door while it was closing
                isOpen = true;
            }
            else
            {
                Level.S.SetPFBlockedMap(pos, isOpen ? WorldBlock.PFBlockType.Unblocked : WorldBlock.PFBlockType.DoorBlocked);
            }
            
            yield break;    
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

        public void Click()
        {
            OpenOrClose(!isOpen);
        }
    }
}

