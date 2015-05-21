using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//This, added to graphics object, allows
//creation of entities in editor, linking
//graphics obj back to entity and changing entity 
//from graphics frontend in editor
public abstract class EditorEntityBacklink : MonoBehaviour
{
    public Entity ent;

    public abstract Entity CreateEntity();    
}