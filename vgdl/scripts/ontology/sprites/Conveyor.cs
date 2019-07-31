using UnityEngine;

public class Conveyor : VGDLSprite
{   
    public Conveyor()
    {
        is_static = true;
        color = Color.blue;
        jump_strength = 1;
        draw_arrow = true;
        is_oriented = true;
    }

    public Conveyor(Conveyor from) : base(from) { }
}