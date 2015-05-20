using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Undos
{
    public class ChangePassability : IUndo
    {        
        HexXY p;

        public ChangePassability(HexXY p)
        {   
            this.p = p;
        }

        public bool Change()
        {
            if (Level.S.GetCellType(p) == TerrainCellType.Empty ||
                Level.S.GetPFBlockedMap(p) == WorldBlock.PFBlockType.DynamicBlocked) return false;


            if (Level.S.GetPFBlockedMap(p) == WorldBlock.PFBlockType.Unblocked)
            {
                Level.S.SetPFBlockedMap(p, WorldBlock.PFBlockType.StaticBlocked);
                var tileVis = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Editor/SBTileVis"));
                tileVis.GetComponent<StaticBlockedTileVisual>().p = p;
                tileVis.transform.SetParent(LevelEditor.S.sbvisParent.transform, false);
                Vector2 pp = p.ToPlaneCoordinates();
                tileVis.transform.localPosition = new Vector3(pp.x, 0, pp.y);
            }
            else
            {
                Level.S.SetPFBlockedMap(p, WorldBlock.PFBlockType.Unblocked);

                foreach (Transform t in LevelEditor.S.sbvisParent.transform)
                {
                    if (t.GetComponent<StaticBlockedTileVisual>().p == p)
                    {
                        GameObject.Destroy(t.gameObject);
                        break;
                    }
                }            
            }

            return true;
        }

        public void Redo()
        {
            Change();
        }

        public void Undo()
        {
            Change();
        }
    }
}
