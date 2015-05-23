using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Engine;

public class InanimateGraphics : EntityGraphics
{
    public override void Die()
    {
        Destroy(gameObject);       
    }
}

