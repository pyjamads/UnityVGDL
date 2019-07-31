
//Player controlled sprites are of type Avatar

using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class VGDLAvatar : VGDLSprite
{
    /**
     * Milliseconds allowed per controller action.
     */
    public const int ACTION_TIME = 40;
    
    /**
    * Milliseconds for controller disqualification, if it returns an action after this time.
    */
    public const int ACTION_TIME_DISQ = 50;
    
    /**
 * Milliseconds allowed for controller tear down.
 */
    public const int TEAR_DOWN_TIME = 100;
    
    /**
    * Indicates if the overspend should be taken into account or not.
    *  Time limits are WALL TIME on Windows, because CPU TIME is not accurate enough
    *  at the level of milliseconds on this OS.
    */
    public const bool TIME_CONSTRAINED = false;
    
    public List<VGDLAvatarActions> actions = new List<VGDLAvatarActions>();
    public List<VGDLAvatarActions> actionsNIL = new List<VGDLAvatarActions>();
    //public bool alternate_keys;
    
    public VGDLPlayerInterface player;
    public int playerID;
    public VGDLPlayerOutcomes winState = VGDLPlayerOutcomes.NO_WINNER;

    public float score = 0.0f;
    
    /**
    * Disqualified flag, moved from Game class to individual players,
    * as there may be more than 1 in a game; variable still in Game
    * class for single player games to keep back-compatibility
    */
    public bool is_disqualified;

    //Avatar can have any KeyHandler system. We use KeyInput by default.
    protected VGDLInputHandler inputHandler;

    public VGDLMovementTypes lastMovementType = VGDLMovementTypes.STILL;

    protected VGDLAvatar()
    {
        inputHandler = new VGDLInputHandler();
    }

    public VGDLAvatar(VGDLAvatar from) : base(from)
    {
        //Insert fields to be copied by copy constructor.
        actions.AddRange(from.actions);
        actionsNIL.AddRange(from.actionsNIL);

        playerID = from.playerID;
        winState = from.winState;
        score = from.score;
        is_disqualified = from.is_disqualified;
        
        //Don't know about this one
        //inputHandler = from.inputHandler;
        setKeyHandler(from.getKeyHandler());
        lastMovementType = from.lastMovementType;
    }
    
    public override void init(Vector2 position, Vector2 size)
    {
        //Define actions here first.
        if(actions.Count==0)
        {
            actions.Add(VGDLAvatarActions.ACTION_UP);
            actions.Add(VGDLAvatarActions.ACTION_DOWN);
            actions.Add(VGDLAvatarActions.ACTION_LEFT);
            actions.Add(VGDLAvatarActions.ACTION_RIGHT);
        }

        base.init(position, size);

        //A separate array with the same actions, plus NIL.
        actionsNIL.AddRange(actions);
        actionsNIL.Add(VGDLAvatarActions.ACTION_NIL);
    }

    //TODO: THINK ABOUT INPUTS SOME MORE!!!
    //Java version uses KeyMasks to store AvatarActions between updates, and classes like FlakAvatar exploits that.
    //Something about limiting the actions up front, instead of after the fact feels better to me.
    
    /**
     * This update call is for the game tick() loop.
     * @param game current state of the game.
     */
    public virtual void updateAvatar(VGDLGame game, bool requestInput, List<VGDLAvatarActions> actionMask = null)
    {
        lastMovementType = VGDLMovementTypes.STILL;

        var direction = VGDLUtils.VGDLDirections.NONE.getDirection();

        if (requestInput || actionMask == null)
        {
           //Get the input from the player.
            VGDLAvatarActions action;
            if (player != null && player.isHuman)
            {
                if (inputHandler.ProcessEscapeInput(playerID))
                {
                    game.Abort();
                    action = VGDLAvatarActions.ACTION_ESCAPE;
                }
                //TODO: CHECK IF actions INCLUDE USE KEY!
                else if (inputHandler.ProcessUseInput(playerID))
                {
                    //NOTE: technically we could allow directional input + use
                    action = VGDLAvatarActions.ACTION_USE;
                }
                else
                {
                    //TODO: CHECK IF actions INCLUDE DIRECTIONS!
                    direction = inputHandler.ProcessPlayerMovement(playerID, true);
                    //NOTE: For now we don't handle multiple actions at once, so this is okay.
                    //But if/when that happens, the AvatarActions need to deal with that.
                    action = AvatarAction.fromVector(direction);
                }
                
                player.logAction(action);
                game.avatarLastAction[playerID] = action;
            }
            else
            {
                if (player == null)
                {
                    Debug.LogError("No player attached to avatar: "+getType());
                    return;
                }
                
                action = requestAgentInput(game);
                direction = action.getDirection();
            }
        } 
        else
        {
            direction = inputHandler.ProcessPlayerInput(actionMask);
        }

        //Apply the physical movement.
        applyMovement(game, direction);
    }
    
    /**
     * Requests the controller's input, setting the game.ki.action mask with the processed data.
     * @param game
     */
    protected VGDLAvatarActions requestAgentInput(VGDLGame game) {
        var ect = new ElapsedCpuTimer();
        ect.setMaxTimeMilliseconds(ACTION_TIME);
        
        var action = VGDLAvatarActions.ACTION_NIL;
        if (game.no_players > 1) {
            action = player.act(game.getObservationMulti(playerID), ect.copy());
        } else {
            action = player.act(game.getObservation(), ect.copy());
        }
        
        if (TIME_CONSTRAINED && ect.exceededMaxTime()) {
            var exceeded = -ect.remainingTimeMilliseconds();

            if (ect.ElapsedMilliseconds > ACTION_TIME_DISQ) {
                //The agent took too long to replay. The game is over and the agent is disqualified
                Debug.Log("Too long: " + playerID + "(exceeding " + (exceeded) + "ms): controller disqualified.");
                game.disqualify(playerID);
            } else {
                Debug.Log("Overspent: " + playerID + "(exceeding " + (exceeded) + "ms): applying ACTION_NIL.");
            }

            action = VGDLAvatarActions.ACTION_NIL;
        }

        if (action == VGDLAvatarActions.ACTION_ESCAPE) {
            game.Abort();
        } else if (!actions.Contains(action)) {
            action = VGDLAvatarActions.ACTION_NIL;
        }

        player.logAction(action);
        game.avatarLastAction[playerID] = action;

        return action;
    }

    public virtual void applyMovement(VGDLGame game, Vector2 action)
    {
        if (!physicstype.CompareAndIgnoreCase("GRID"))
        {
            updatePassive();
        }
        lastMovementType = physics.activeMovement(this, action, speed);
    }

    public void passResult(VGDLGame game)
    {
        var ect = new ElapsedCpuTimer();
        ect.setMaxTimeMilliseconds(TEAR_DOWN_TIME);
        
        if (game.no_players > 1) {
            player.result(game.getObservationMulti(playerID), ect.copy());
        } else {
            player.result(game.getObservation(), ect.copy());
        }
    }
    
    /**
     * Sets the disqualified flag.
     */
    public void disqualify(bool is_disqualified)
    {
        this.is_disqualified = is_disqualified;
        this.winState = VGDLPlayerOutcomes.PLAYER_DISQ;
    }

    
    public void addScore(float scoreChange)
    {
        score += scoreChange;
    }

    public void setKeyHandler(VGDLInputHandler gameInputHandler)
    {
        inputHandler = gameInputHandler;
    }

    public VGDLInputHandler getKeyHandler()
    {
        return inputHandler;
    }
}

public enum VGDLAvatarActions
{
    ACTION_NIL,
    ACTION_UP,
    ACTION_DOWN,
    ACTION_LEFT,
    ACTION_RIGHT,
    ACTION_USE,
    ACTION_ESCAPE,
//    ACTION_UP_LEFT,
//    ACTION_UP_RIGHT,
//    ACTION_DOWN_LEFT,
//    ACTION_DOWN_RIGHT,
 //   ACTION_ANALOG,
}

public static class AvatarAction
{
    public static KeyCode[] getKeys(this VGDLAvatarActions action)
    {
        switch (action)
        {
            case VGDLAvatarActions.ACTION_NIL:
                return new[] {KeyCode.None, KeyCode.None};
            case VGDLAvatarActions.ACTION_UP:
                return new[] {KeyCode.UpArrow, KeyCode.W};
            case VGDLAvatarActions.ACTION_LEFT:
                return new[] {KeyCode.LeftArrow, KeyCode.A};
            case VGDLAvatarActions.ACTION_DOWN:
                return new[] {KeyCode.DownArrow, KeyCode.S};
            case VGDLAvatarActions.ACTION_RIGHT:
                return new[] {KeyCode.RightArrow, KeyCode.D};
            case VGDLAvatarActions.ACTION_USE:
                return new[] {KeyCode.Space, KeyCode.LeftShift};
            case VGDLAvatarActions.ACTION_ESCAPE:
                return new[] {KeyCode.Escape, KeyCode.Escape};
//            case AvatarActions.ACTION_UP_LEFT:
//            case AvatarActions.ACTION_UP_RIGHT:
//            case AvatarActions.ACTION_DOWN_LEFT:
//            case AvatarActions.ACTION_DOWN_RIGHT:
//                return new[] {KeyCode.None, KeyCode.None};
            default:
                throw new ArgumentOutOfRangeException("action", action, null);
        }
    }

    public static VGDLAvatarActions getRandomAction()
    {
        var rnd = UnityEngine.Random.Range(0, 5);

        switch (rnd)
        {
            case 0:
                return VGDLAvatarActions.ACTION_UP;
            case 1:
                return VGDLAvatarActions.ACTION_LEFT;
            case 2:
                return VGDLAvatarActions.ACTION_DOWN;
            case 3:
                return VGDLAvatarActions.ACTION_RIGHT;
            default:
                return VGDLAvatarActions.ACTION_USE;
        }
    }
    
    
    public static VGDLAvatarActions fromString(String strKey)
    {
        if (strKey.CompareAndIgnoreCase("ACTION_UP")) return VGDLAvatarActions.ACTION_UP;
        if (strKey.CompareAndIgnoreCase("ACTION_LEFT")) return VGDLAvatarActions.ACTION_LEFT;
        if (strKey.CompareAndIgnoreCase("ACTION_DOWN")) return VGDLAvatarActions.ACTION_DOWN;
        if (strKey.CompareAndIgnoreCase("ACTION_RIGHT")) return VGDLAvatarActions.ACTION_RIGHT;
        if (strKey.CompareAndIgnoreCase("ACTION_USE")) return VGDLAvatarActions.ACTION_USE;
        if (strKey.CompareAndIgnoreCase("ACTION_ESCAPE")) return VGDLAvatarActions.ACTION_ESCAPE;
//        if (strKey.CompareAndIgnoreCase("ACTION_UP_LEFT")) return AvatarActions.ACTION_UP_LEFT;
//        if (strKey.CompareAndIgnoreCase("ACTION_UP_RIGHT")) return AvatarActions.ACTION_UP_RIGHT;
//        if (strKey.CompareAndIgnoreCase("ACTION_DOWN_LEFT")) return AvatarActions.ACTION_DOWN_LEFT;
//        if (strKey.CompareAndIgnoreCase("ACTION_DOWN_RIGHT")) return AvatarActions.ACTION_DOWN_RIGHT;
        
        return VGDLAvatarActions.ACTION_NIL;
    }
    
    public static VGDLAvatarActions fromVector(Vector2 move)
    {
        // Probably better to use .equals() instead of == to test for equality,
        // but not necessary for the current call hierarchy of this method
        if (move.Equals(VGDLUtils.VGDLDirections.UP.getDirection())) return VGDLAvatarActions.ACTION_UP;
        if (move.Equals(VGDLUtils.VGDLDirections.DOWN.getDirection())) return VGDLAvatarActions.ACTION_DOWN;
        if (move.Equals(VGDLUtils.VGDLDirections.LEFT.getDirection())) return VGDLAvatarActions.ACTION_LEFT;
        if (move.Equals(VGDLUtils.VGDLDirections.RIGHT.getDirection())) return VGDLAvatarActions.ACTION_RIGHT;
        return VGDLAvatarActions.ACTION_NIL;
    }

    public static Vector2 getDirection(this VGDLAvatarActions action)
    {
        // Probably better to use .equals() instead of == to test for equality,
        // but not necessary for the current call hierarchy of this method
        if (action.Equals(VGDLAvatarActions.ACTION_UP)) return VGDLUtils.VGDLDirections.UP.getDirection();
        if (action.Equals(VGDLAvatarActions.ACTION_DOWN)) return VGDLUtils.VGDLDirections.DOWN.getDirection();
        if (action.Equals(VGDLAvatarActions.ACTION_LEFT)) return VGDLUtils.VGDLDirections.LEFT.getDirection();
        if (action.Equals(VGDLAvatarActions.ACTION_RIGHT)) return VGDLUtils.VGDLDirections.RIGHT.getDirection();
        if (action.Equals(VGDLAvatarActions.ACTION_NIL)) return VGDLUtils.VGDLDirections.NIL.getDirection();
        return VGDLUtils.VGDLDirections.NONE.getDirection();
    }

    public static bool isMoving(this VGDLAvatarActions value){
        return value == VGDLAvatarActions.ACTION_UP || value == VGDLAvatarActions.ACTION_DOWN ||
               value == VGDLAvatarActions.ACTION_LEFT || value == VGDLAvatarActions.ACTION_RIGHT;
    }

    public static VGDLAvatarActions ReverseAction(this VGDLAvatarActions value){
        switch (value)
        {
            case VGDLAvatarActions.ACTION_DOWN:
                return VGDLAvatarActions.ACTION_UP;
            case VGDLAvatarActions.ACTION_UP:
                return VGDLAvatarActions.ACTION_DOWN;
            case VGDLAvatarActions.ACTION_RIGHT:
                return VGDLAvatarActions.ACTION_LEFT;
            case VGDLAvatarActions.ACTION_LEFT:
                return VGDLAvatarActions.ACTION_RIGHT;
            default:
                return VGDLAvatarActions.ACTION_NIL;
        }
    }

}