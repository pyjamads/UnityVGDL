using System.Collections.Generic;
using UnityEngine;

public class VGDLPulseHandler : VGDLKeyHandler
{
    //TODO: consider if we need to track the input with a fifo queue, in the view or something.
    public virtual Vector2 ProcessPlayerMovementFixed(int playerID, bool allowGridDiagonal = false, bool allowAnalog = false)
    {
        var actions = new List<VGDLAvatarActions>();
        
        if (Input.GetKeyUp(VGDLAvatarActions.ACTION_UP.getKeys()[playerID])) actions.Add(VGDLAvatarActions.ACTION_UP);
        if (Input.GetKeyUp(VGDLAvatarActions.ACTION_DOWN.getKeys()[playerID])) actions.Add(VGDLAvatarActions.ACTION_DOWN);
        
        if (Input.GetKeyUp(VGDLAvatarActions.ACTION_LEFT.getKeys()[playerID])) actions.Add(VGDLAvatarActions.ACTION_LEFT);
        if (Input.GetKeyUp(VGDLAvatarActions.ACTION_RIGHT.getKeys()[playerID])) actions.Add(VGDLAvatarActions.ACTION_RIGHT);

        return ProcessPlayerInput(actions, allowGridDiagonal, allowAnalog);
    }

    public override Vector2 ProcessPlayerMovement(int playerID, bool fixedVersion = false)
    {
        //NOTE by Mads: Game Design wise, this implementation is bad,
        //so I created a fixedVersion that fixes the below issues.

        if (fixedVersion)
        {
            //NOTE: can be called with allowAnalog for continuous games, and allow grid diagonal.
            const bool allowGridDiagonal = false; //Returns horizontal+vertical movement
            const bool allowAnalog = false; //Return horizontal+vertical movement normalized 
            return ProcessPlayerMovementFixed(playerID, allowGridDiagonal, allowAnalog);
        }
        
        //NOTE: Pressing UP/DOWN + LEFT/RIGHT returns NONE
        //Should possibly return one or more directions

        //NOTE: Pressing LEFT + RIGHT returns RIGHT
        //NOTE: Pressing UP + DOWN returns DOWN
        //Should definitely return NONE

        //NOTE: Pressing UP/DOWN + LEFT + RIGHT returns NONE
        //Should return UP or DOWN

        //NOTE: Pressing UP + DOWN + LEFT/RIGHT returns NONE
        //Should return LEFT or RIGHT
        
        
        int vertical = 0;
        int horizontal = 0;

        if (Input.GetKeyUp(VGDLAvatarActions.ACTION_UP.getKeys()[playerID])) {
            vertical = -1;
        }
        if (Input.GetKeyUp(VGDLAvatarActions.ACTION_DOWN.getKeys()[playerID])) {
            vertical = 1;
        }


        if (Input.GetKeyUp(VGDLAvatarActions.ACTION_LEFT.getKeys()[playerID])) {
            horizontal = -1;
        }
        if (Input.GetKeyUp(VGDLAvatarActions.ACTION_RIGHT.getKeys()[playerID])) {
            horizontal = 1;
        }

        if (horizontal == 0) {
            if (vertical == 1)
                return VGDLUtils.VGDLDirections.DOWN.getDirection();
            else if (vertical == -1)
                return VGDLUtils.VGDLDirections.UP.getDirection();
        } else if (vertical == 0) {
            if (horizontal == 1) {
                return VGDLUtils.VGDLDirections.RIGHT.getDirection();
            }
            else if (horizontal == -1)
                return VGDLUtils.VGDLDirections.LEFT.getDirection();
        }
        return VGDLUtils.VGDLDirections.NONE.getDirection();
    }

    public override bool ProcessUseInput(int playerID)
    {
        return Input.GetKeyUp(VGDLAvatarActions.ACTION_USE.getKeys()[playerID]);
    }

    public override bool ProcessEscapeInput(int playerID)
    {
        return Input.GetKeyUp(VGDLAvatarActions.ACTION_ESCAPE.getKeys()[playerID]);;
    }
}