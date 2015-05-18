using UnityEngine;
using System.Collections;
using Engine;

[ExecuteInEditMode]
public class Stone : SpellEffectGraphics
{
    float origYPos;

    public override void Spawn(HexXY pos)
    {
        //TODO: spawn effect
        base.Spawn(pos);
    }


    public override void Die()
    {
        //TODO: die effect
        base.Die();
    }

    void OnEnable()
    {
        origYPos = transform.position.y;

        transform.Rotate(new Vector3(0, 1, 0), Random.Range(0, 360));
    }

    void Update()
    {
        if (power.IsNew)
        {
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(origYPos, -0.3f, 1 - power.Value), transform.position.z);
        }
    }    
}
