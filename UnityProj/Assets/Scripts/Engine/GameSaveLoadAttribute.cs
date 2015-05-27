using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{   
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class GameSaveLoadAttribute : Attribute
    {   
        readonly string guid;
     
        public GameSaveLoadAttribute(string guid)
        {
            this.guid = guid;
        }

        public string Guid
        {
            get { return guid; }
        }        
    }   
}
