using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class TerrainTypeStickyButton : StickyButton
{
    public TerrainCellType type;

    public override void SetPressed(bool pressed)
    {
        base.SetPressed(pressed);

        LevelEditor.S.brushCellType = type;
    }

    
}

