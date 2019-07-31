
using UnityEngine;

public class VGDLHumanAgent : VGDLPlayer
{
    public VGDLHumanAgent()
    {
        isHuman = true;
    }
    
    public override VGDLAvatarActions act(StateObservation stateObs, ElapsedCpuTimer elapsedTimer)
    {
        var keyhandler = stateObs.getKeyHandler(0);

        var move = keyhandler.ProcessPlayerMovement(0);
        var useOn = keyhandler.ProcessUseInput(0);

        //In the keycontroller, move has preference.
        var action = AvatarAction.fromVector(move);

        //if(action == Types.ACTIONS.ACTION_NIL && useOn)
        if(useOn) //This allows switching to Use when moving.
            action = VGDLAvatarActions.ACTION_USE;


        return action;
    }

    public override VGDLAvatarActions act(StateObservationMulti stateObs, ElapsedCpuTimer elapsedTimer)
    {
        //int id = (getPlayerID() + 1) % stateObs.getNoPlayers();
        var keyhandler = stateObs.getKeyHandler(PlayerID);

        var move = keyhandler.ProcessPlayerMovement(PlayerID);
        var useOn = keyhandler.ProcessUseInput(PlayerID);


        //In the keycontroller, move has preference.
        VGDLAvatarActions action = AvatarAction.fromVector(move);
        
        if(action == VGDLAvatarActions.ACTION_NIL && useOn)
            action = VGDLAvatarActions.ACTION_USE;

        return action;
    }
}
