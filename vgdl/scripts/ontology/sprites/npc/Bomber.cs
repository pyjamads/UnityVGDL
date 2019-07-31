using UnityEngine;

public class Bomber : SpawnPoint
{
    public Bomber()
    {
        color = VGDLColors.Orange;
        is_static = false;
        is_oriented = true;
        orientation = Vector2.right;
        is_npc = true;
    }	
    
    public Bomber(Bomber from) : base(from) { }
}