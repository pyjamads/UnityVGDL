public class waterPhysics : VGDLEffect
{
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        sprite1.gravity = 0.2f;
    }
}