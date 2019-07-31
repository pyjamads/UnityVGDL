using System.Collections.Generic;
using UnityEngine;

public class OngoingTurningAvatar : OrientedAvatar
{
    public string spawnBehind;

    public OngoingTurningAvatar()
    {
        speed = 1;
        is_oriented = true;
    }
    
    public OngoingTurningAvatar(OngoingTurningAvatar from) : base(from){
        //Insert fields to be copied by copy constructor.
        spawnBehind = from.spawnBehind;
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
        // but only if the movement is perpendicular to the current orientation.
        if(action != VGDLUtils.VGDLDirections.NONE.getDirection() && action.othogonal(orientation))
        {
            updateOrientation(action);
        }

        //Update movement.
        base.updatePassive();

        //Spawn behind:
        if (!rect.Overlaps(lastrect) && !string.IsNullOrEmpty(spawnBehind))
        {
            game.addSprite(spawnBehind, getLastPosition());
        }
    }
}