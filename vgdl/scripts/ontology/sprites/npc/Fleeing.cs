public class Fleeing : Chaser
{   
    public Fleeing()
    {
        fleeing = true;
    }

    public Fleeing(Fleeing from) : base(from)
    {
        
    }
}