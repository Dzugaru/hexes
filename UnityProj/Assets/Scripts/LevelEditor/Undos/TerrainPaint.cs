using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Undos
{
    public class TerrainPaint : IUndo
    {
        struct CellChange
        {
            public HexXY p;
            public TerrainCellType oldCellType, newCellType;
        }

        Stack<CellChange> changes = new Stack<CellChange>();

        void PaintOneCell(HexXY p, TerrainCellType type, HashSet<WorldBlock> changedBlocks, TerrainCellType oldCellType, bool writeChanges)
        {
            var changedWb = Level.S.SetCellType(p, type);
            if (Level.S.GetCellType(p) != type) return; //Set failure

            if(writeChanges)
                changes.Push(new CellChange() { oldCellType = oldCellType, newCellType = type, p = p });

            if (!changedBlocks.Contains(changedWb))
                changedBlocks.Add(changedWb);

            HexXY locp = Level.GetLocalCoords(p);

            //Other blocks possible walls fix
            if (locp.x == 0)
            {
                var nbl = Level.S.GetBlock(changedWb.position - new HexXY(1, 0));
                if (nbl != null && !changedBlocks.Contains(nbl))
                    changedBlocks.Add(nbl);
            }

            if (locp.y == 0)
            {
                var nbl = Level.S.GetBlock(changedWb.position - new HexXY(0, 1));
                if (nbl != null && !changedBlocks.Contains(nbl))
                    changedBlocks.Add(nbl);
            }

            if (locp.x == WorldBlock.sz - 1)
            {
                var nbl = Level.S.GetBlock(changedWb.position + new HexXY(1, 0));
                if (nbl != null && !changedBlocks.Contains(nbl))
                    changedBlocks.Add(nbl);
            }

            if (locp.y == WorldBlock.sz - 1)
            {
                var nbl = Level.S.GetBlock(changedWb.position + new HexXY(0, 1));
                if (nbl != null && !changedBlocks.Contains(nbl))
                    changedBlocks.Add(nbl);
            }
        }

        public void Paint(HexXY p, int brushSize, bool shouldPaintOnEmpty, TerrainCellType brushCellType)
        {
            var changedBlocks = new HashSet<WorldBlock>();

            for (int x = -(brushSize - 1); x <= brushSize - 1; x++)
            {
                for (int y = -(brushSize - 1); y <= brushSize - 1; y++)
                {
                    HexXY d = new HexXY(x, y);
                    if (HexXY.Dist(d) > brushSize - 1) continue;
                    HexXY pd = p + d;

                    var cellType = Level.S.GetCellType(pd);

                    if (cellType != brushCellType &&
                        (shouldPaintOnEmpty || cellType != TerrainCellType.Empty || brushCellType == TerrainCellType.Empty))
                    {
                        PaintOneCell(pd, brushCellType, changedBlocks, cellType, true);
                    }
                }
            }

            foreach (var wb in changedBlocks)
                TerrainController.S.RecreateHexTerrain(wb);
        }

        public void Undo()
        {
            var changedBlocks = new HashSet<WorldBlock>();
            foreach (var ch in changes)            
                PaintOneCell(ch.p, ch.oldCellType, changedBlocks, TerrainCellType.Empty, false);            
            foreach (var wb in changedBlocks)
                TerrainController.S.RecreateHexTerrain(wb);
        }

        public void Redo()
        {
            var changedBlocks = new HashSet<WorldBlock>();
            foreach (var ch in changes.Reverse())
                PaintOneCell(ch.p, ch.newCellType, changedBlocks, TerrainCellType.Empty, false);
            foreach (var wb in changedBlocks)
                TerrainController.S.RecreateHexTerrain(wb);
        }
    }
}
