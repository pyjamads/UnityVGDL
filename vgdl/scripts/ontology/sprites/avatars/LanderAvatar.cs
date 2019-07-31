using System.Collections.Generic;
using UnityEngine;

public class LanderAvatar : OrientedAvatar
{
    public float angle_diff;

    public LanderAvatar()
    {
        angle_diff = 0.3f;
        speed = 5;
        orientation = VGDLUtils.VGDLDirections.NONE.getDirection();
    }

    public LanderAvatar(LanderAvatar from) : base (from)
    {
        angle_diff = from.angle_diff;
    }

/**
     * This update call is for the game tick() loop.
     * @param game current state of the game.
     */
    public override void updateAvatar(VGDLGame game, bool requestInput, List<VGDLAvatarActions> actionMask = null)
    {
        base.updateAvatar(game, requestInput, actionMask);
        aim(game);
        move(game);
    }
    
    public override void applyMovement(VGDLGame game, Vector2 action)
    {
        if (!physicstype.CompareAndIgnoreCase(VGDLPhysics.GRID))
        {
            base.updatePassive();
        }
    }

    
    public void aim(VGDLGame game)
    {
        var angle = rotation;

        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_LEFT)
        {
            angle -= angle_diff;
        }
        else if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_RIGHT) 
        {
            angle += angle_diff;
        }
        updateRotation(angle);
    }
    
    public void move(VGDLGame game)
    {
        var facing = new Vector2(0,0);

        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_UP) 
        {
            facing = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
            physics.activeMovement(this, facing, speed);
        }
    }
}