using System.Collections.Generic;

public class MissileAvatar : OrientedAvatar
{
    public MissileAvatar()
    {
        speed = 1;
        is_oriented = true;
    }
    
    public MissileAvatar(MissileAvatar from) : base(from)
    {
        //Insert fields to be copied by copy constructor.   
    }

    /**
     * This update call is for the game tick() loop.
     * @param game current state of the game.
     */
    public override void updateAvatar(VGDLGame game, bool requestInput, List<VGDLAvatarActions> actionMask = null)
    { 
        if (requestInput || actionMask == null) {
            //Get the input from the player.
            requestAgentInput(game);
        }

        //MissileAvatar has no actions available. Just update movement.
        base.updatePassive();
    }
}