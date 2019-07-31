using System.Collections.Generic;
using UnityEngine;

public class PlatformerAvatar : MovingAvatar
{
    public float ground_speedup_factor;
    public float air_slowdown_factor;
    
    public PlatformerAvatar()
    {
        draw_arrow = false;
        jump_strength = 10f;
        on_ground = false;
        ground_speedup_factor = 1.0f;
        air_slowdown_factor = 2.0f;
        max_speed = 30.0f;
    }

    public PlatformerAvatar(PlatformerAvatar from) : base(from)
    {
        //Insert fields to be copied by copy constructor.
        air_slowdown_factor = from.air_slowdown_factor;
        ground_speedup_factor = from.ground_speedup_factor;
    }

    public override void init(Vector2 position, Vector2 size)
    {
        //Define actions here first.
        if(actions.Count==0)
        {
            actions.Add(VGDLAvatarActions.ACTION_LEFT);
            actions.Add(VGDLAvatarActions.ACTION_RIGHT);
            actions.Add(VGDLAvatarActions.ACTION_USE);
        }
        
        base.init(position, size);
    }

    public override bool intersects(VGDLSprite s2)
    {
        return groundIntersects(s2);
    }
    
    /**
     * This update call is for the game tick() loop.
     * @param game current state of the game.
     */
    public override void updateAvatar(VGDLGame game, bool requestInput, List<VGDLAvatarActions> actionMask = null)
    {
        base.updateAvatar(game, requestInput, actionMask);

        //Managing jumps
        if(game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_USE && on_ground) {
            var action = new Vector2 (0,-jump_strength);
            orientation = new Vector2 (orientation.x,0.0f);
            physics.activeMovement(this, action, speed);
            var temp = new Vector2 (0,-1);
            lastmove = cooldown; //need this to force this movement.
            updatePos(temp, 5);
        }


        //This at the end, needed for check on ground status in the next cycle.
        on_ground = false;
    }
    
    public override void applyMovement(VGDLGame game, Vector2 action)
    {
        //this.physics.passiveMovement(this);
        if (!physicstype.CompareAndIgnoreCase(VGDLPhysics.GRID))
        {
            base.updatePassive();
        }

        if (action.x != 0.0f || action.y != 0.0f)
        {
            var new_action = new Vector2(action.x * ground_speedup_factor, action.y);
            if (!on_ground)
            {
                new_action = new Vector2(action.x / air_slowdown_factor, action.y);
            }

            lastMovementType = physics.activeMovement(this, new_action, speed);
        }
    }
}