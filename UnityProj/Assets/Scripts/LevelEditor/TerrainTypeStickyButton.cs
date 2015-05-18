using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class TerrainTypeStickyButton : StickyButton
{
    public TerrainCellType type;

    public override void OnClick()
    {
        base.OnClick();

        if (isPressed)
            LevelEditor.S.brushCellType = type;
        else
            LevelEditor.S.brushCellType = null;
        
    }
}

