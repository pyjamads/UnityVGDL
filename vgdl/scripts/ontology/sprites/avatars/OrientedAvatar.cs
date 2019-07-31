using System.Collections.Generic;
using UnityEngine;

public class OrientedAvatar : MovingAvatar
{
    public OrientedAvatar()
    {
        orientation = VGDLUtils.VGDLDirections.RIGHT.getDirection();
        draw_arrow = true;
        is_oriented = true;
        rotateInPlace = true;
    }

    public OrientedAvatar(OrientedAvatar from) : base(from)
    {
        //Insert fields to be copied by copy constructor.
    }

    public override void updateAvatar(VGDLGame game, bool requestInput, List<VGDLAvatarActions> actionMask = null)
    {
        base.updateAvatar(game, requestInput, actionMask);

        //If the last thing the avatar did is to move (displacement), then update
        //the orientation in the direction of the move.
        if(lastMovementType == VGDLMovementTypes.MOVE)
        {
            if (physicstype == "GRID"){
                var dir = lastDirection();
                dir.Normalize();
                orientation = dir;
            }
        }
        //Otherwise, orientation is already updated, no need to change anything.
    }
}