using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine
{
    [Serializable]
    public class ScriptObjectID
    {
        [SerializeField]
        public int id;

        public static implicit operator int (ScriptObjectID id)
        {
            return id.id;
        }

        public static explicit operator ScriptObjectID (int id)
        {
            return new ScriptObjectID() { id = id };
        }
    }
}
