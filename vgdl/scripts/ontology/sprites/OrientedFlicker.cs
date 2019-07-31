public class OrientedFlicker : Flicker
{
    public OrientedFlicker()
    {
        draw_arrow = true;
        speed = 0;
        is_oriented = true;
    }

    public OrientedFlicker(OrientedFlicker from) : base(from) { }
}