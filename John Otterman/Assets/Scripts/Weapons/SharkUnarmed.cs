using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkUnarmed : ActiveWeapon
{
    public override void FlipSprite(bool flip)
    {
        if (flip) transform.localScale = new Vector3(1, -1, 1);
        else transform.localScale = Vector3.one;
    }
}
