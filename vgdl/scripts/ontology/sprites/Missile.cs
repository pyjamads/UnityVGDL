public class Missile : VGDLSprite
{
    public Missile()
    {
        speed = 1;
        is_oriented = true;
    }	
    
    public Missile(Missile from) : base(from) { }
}