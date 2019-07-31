using System.Collections.Generic;
using UnityEngine;

public class AimedAvatar : ShootAvatar
{
    public float angle_diff;
    
    public AimedAvatar()
    {
        angle_diff = 0.15f;
        speed = 0;
        stationary = true;
        
        //Actions are different
        actions.Add(VGDLAvatarActions.ACTION_USE);
        actions.Add(VGDLAvatarActions.ACTION_DOWN);
        actions.Add(VGDLAvatarActions.ACTION_UP);
    }

    public AimedAvatar(AimedAvatar from) : base(from)
    {
        angle_diff = from.angle_diff;
    }

    public override void init(Vector2 position, Vector2 size)
    {
        //Define actions here first.
        if(actions.Count==0)
        {
            actions.Add(VGDLAvatarActions.ACTION_USE);
            actions.Add(VGDLAvatarActions.ACTION_DOWN);
            actions.Add(VGDLAvatarActions.ACTION_UP);
        }
        
        base.init(position, size);
    }

    /**
     * This update call is for the game tick() loop.
     * @param game current state of the game.
     */
    public override void updateAvatar(VGDLGame game, bool requestInput, List<VGDLAvatarActions> actionMask = null)
    {
        base.updateAvatar(game, requestInput, actionMask);
        //NOTE: updateUse is already run by base.updateAvatar() <-- in ShootAvatar
        updateUse(game);
        aim(game);
        move(game);
    }


    private void aim(VGDLGame game)
    {
        var angle = 0.0f;
        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_UP)
        {
            angle = -angle_diff;
        }
        else if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_DOWN)
        {
            angle = angle_diff;
        }

        var new_x_angle = orientation.x * Mathf.Cos(angle) - orientation.y * Mathf.Sin(angle);
        var new_y_angle = orientation.x * Mathf.Sin(angle) + orientation.y * Mathf.Cos(angle);
        orientation = new Vector2(new_x_angle, new_y_angle);

        updateRotation(Mathf.Atan2(orientation.y, orientation.x));
    }


    public void move(VGDLGame game)
    {
        var facing = new Vector2(0,0);

        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_UP)
        {
            facing = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
            physics.activeMovement(this, facing, speed);
        }
        else if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_DOWN)
        {
            facing = new Vector2(Mathf.Cos(rotation+(Mathf.Deg2Rad * 180)), Mathf.Sin(rotation+(Mathf.Deg2Rad * 180)));
            physics.activeMovement(this, facing, speed);
        }
    }
}