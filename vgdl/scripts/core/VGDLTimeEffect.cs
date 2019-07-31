using System;

public class VGDLTimeEffect : VGDLEffect
{
    /**
   * timer for the effect, -1 by default.
   * Indicates every how many steps this effects is triggered.
   * Set in VGDL.
   */
    public int timer = -1;

    /**
     * Indicates the next time step when this effect will be automatically triggered without collisions.
     * It is set by planExecution().
     */
    public int nextExecution = -1;

    /**
     * Indicates if the effect should be repeated periodically ad infinitum
     */
    public bool repeating = false;

    /**
     * type of the sprite that suffers the effects of the delegated Effect.
     */
    public string targetType;

    /**
     * True if this is a time effect defined in VGDL using TIME.
     * False if this is defined by an AddTimer effect
     */
    public bool isNative = true;

    /**
     * The effect itself, that is triggered by this.
     * it's a unary effect (the second sprite is always TIME).
     */
    public VGDLEffect effectDelegate;

    public VGDLTimeEffect()
    {
        planExecution(null);
    }

    public VGDLTimeEffect(VGDLTimeEffect copyFrom) : this()
    {
        timer = copyFrom.timer;
        repeating = copyFrom.repeating;
        targetType = copyFrom.targetType;
        isNative = copyFrom.isNative;
        effectDelegate = copyFrom.effectDelegate;

        //nextExecution not copied, because this() runs planExecution
        //And we also run it from addTimer
    }

    public void planExecution(VGDLGame game)
    {
        int tick = game == null ? 0 : game.getGameTick();
        nextExecution = tick + timer;
    }
 
    public override void Validate(VGDLGame game)
    {
        base.Validate(game);

        if (string.Compare(targetType, "EOS", true) == 0)
        {
            throw new ArgumentException("Can't use EOS with TIME effect");
        }
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        effectDelegate.execute(sprite1, sprite2, game);
        if (repeating)
        {
            planExecution(game);
        }
    }
}