using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class TerrainTypeStickyButton : StickyButton
{
    public TerrainCellType type;

    public override void SetPressed(bool val)
    {
        base.SetPressed(val);

        if (isPressed)
            LevelEditor.S.brushCellType = type;
        else
            LevelEditor.S.brushCellType = null;
        
    }
}

