using System;
using System.Collections.Generic;
using UnityEngine;

public class WizardAvatar : MovingAvatar
{
    public float ground_speedup_factor;
    public float air_slowdown_factor;


    //This is the sprite I shoot
    public string stype;
    public string[] stypes;

    public Vector2 facing_dir;

    public int last_block_time;

    public WizardAvatar()
    {
        draw_arrow = false;
        jump_strength = 10;
        on_ground = false;
        speed = 0;
        stype = null;
        last_block_time = 0;
        ground_speedup_factor = 1.0f;
        air_slowdown_factor = 2.0f;
        max_speed = 30.0f;

        facing_dir = new Vector2(1,0);
    }

    public WizardAvatar(WizardAvatar from) : base(from)
    {
        ground_speedup_factor = from.ground_speedup_factor;
        air_slowdown_factor = from.air_slowdown_factor;
        stype = from.stype;
        stypes = from.stypes;
        facing_dir = from.facing_dir;
        last_block_time = from.last_block_time;
    }

    public override void init(Vector2 position, Vector2 size)
    {
        //Define actions here first.
        if(actions.Count == 0)
        {
            actions.Add(VGDLAvatarActions.ACTION_LEFT);
            actions.Add(VGDLAvatarActions.ACTION_RIGHT);
            actions.Add(VGDLAvatarActions.ACTION_USE);
            actions.Add(VGDLAvatarActions.ACTION_UP);
        }

        stypes = stype.Split(new []{","}, StringSplitOptions.RemoveEmptyEntries);

        base.init(position, size);
    }

    /**
     * Overwritting intersects to check if we are on ground.
     * @return true if it directly intersects with sp (as in the normal case), but additionally checks for on_ground condition.
     */
    public override bool intersects (VGDLSprite sp)
    {
        return groundIntersects(sp);
    }


    /**
     * This update call is for the game tick() loop.
     * @param game current state of the game.
     */
    public override void updateAvatar(VGDLGame game, bool requestInput, List<VGDLAvatarActions> actionMask = null)
    {
        base.updateAvatar(game, requestInput, actionMask);

        //Managing jumps
        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_USE && on_ground)
        {

            var action = new Vector2(0, -jump_strength);
            orientation = new Vector2(orientation.x, 0.0f);
            physics.activeMovement(this, action, speed);
            var temp = new Vector2(0, -1);
            lastmove = cooldown; //need this to force this movement.
            updatePos(temp, 5);
        }

        //Spawning blocks
        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_UP &&
            last_block_time + 5 <= game.getGameTick())
        {
            physics.activeMovement(this, new Vector2(0, 1), -1);

            for (int i = 0; i < stypes.Length; i++)
            {
                game.addSprite(stypes[i], new Vector2(rect.x + facing_dir.x * lastrect.width * 1.2f,
                    rect.y + facing_dir.y * lastrect.height));

            }

            last_block_time = game.getGameTick();
        }

        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_LEFT)
        {
            facing_dir = new Vector2(-1, 0);
        }

        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_RIGHT)
        {
            facing_dir = new Vector2(1, 0);
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

        if (action.x != 0.0 || action.y != 0.0)
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