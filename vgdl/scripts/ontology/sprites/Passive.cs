using System.Collections.Generic;
using UnityEngine;

public class Passive : VGDLSprite {

    public Passive()
    {
        color = VGDLColors.Red;
    }
    
    public Passive(Passive from) : base(from) { }
}