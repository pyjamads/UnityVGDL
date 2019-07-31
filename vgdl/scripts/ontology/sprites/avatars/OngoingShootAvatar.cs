using System.Collections.Generic;
using UnityEngine;

public class OngoingShootAvatar : ShootAvatar
{
    public OngoingShootAvatar()
    {
        speed = 1;
        is_oriented = true;
    }
    
    public OngoingShootAvatar(OngoingShootAvatar from) : base(from)
    {
        //Insert fields to be copied by copy constructor.   
    }
    
    public override void updateAvatar(VGDLGame game, bool requestInput, List<VGDLAvatarActions> actionMask = null)
    {
        lastMovementType = VGDLMovementTypes.MOVE;

        Vector2 action;

        if (requestInput || actionMask == null) {
            //Get the input from the player.
            requestAgentInput(game);
            //Map from the action mask to a Vector2D action.
            action = game.avatarLastAction[playerID].getDirection();
        } else {
            action = inputHandler.ProcessPlayerInput(actionMask);
        }

        //Update the orientation for this cycle's movement,
        // but only if there was a direction indicated.
        var canShoot = true;
        if (!(action.Equals(VGDLUtils.VGDLDirections.NONE.getDirection())))
        {
            updateOrientation(action);
            canShoot = false;
        }

        //Update movement.
        base.updatePassive();
        
        if(canShoot || lastMovementType == VGDLMovementTypes.STILL)
            updateUse(game);
    }
}