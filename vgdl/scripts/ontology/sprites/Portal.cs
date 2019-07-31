using System.Collections.Generic;
using UnityEngine;

public class Portal : SpriteProducer 
{   
    public Portal()
    {
        is_static = true;
        portal = true;
        color = VGDLColors.Blue;
    }

    public Portal(Portal from) : base(from) { }
}