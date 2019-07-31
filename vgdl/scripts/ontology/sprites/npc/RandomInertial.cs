public class RandomInertial : RandomNPC
{   
    public RandomInertial()
    {
        physicstype = VGDLPhysics.CONT;
        is_oriented = true;
    }

    public RandomInertial(RandomInertial from) : base(from)
    {
        
    }
}