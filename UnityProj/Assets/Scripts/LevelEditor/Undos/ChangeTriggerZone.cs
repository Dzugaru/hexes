#if UNITY_EDITOR
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Undos
{
    public class ChangeTriggerZone : IUndo
    {        
        HexXY p;
        uint zone;

        public ChangeTriggerZone(HexXY p, uint zone)
        {   
            this.p = p;
            this.zone = zone;
        }

        public static void CreateVis(HexXY p)
        {
            var tileVis = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Editor/TriggerTileVis"));
            tileVis.GetComponent<TileColorVisual>().p = p;
            tileVis.transform.SetParent(LevelEditor.S.trigVisParent.transform, false);
            var pp = p.ToPlaneCoordinates();
            tileVis.transform.localPosition = new Vector3(pp.x, 0, pp.y);
        }

        public bool Change()
        {
            if (LevelEditor.S.levelScript.GetTriggerZonesAt(p).Contains(zone))
            {
                LevelEditor.S.levelScript.RemoveTriggerZoneFrom(p, zone);
                foreach (Transform t in LevelEditor.S.trigVisParent.transform)                
                    if (t.GetComponent<TileColorVisual>().p == p)
                    {
                        GameObject.Destroy(t.gameObject);
                        break;
                    }                
            }
            else
            {
                LevelEditor.S.levelScript.AddTriggerZoneTo(p, zone);
                CreateVis(p);
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
#endif
