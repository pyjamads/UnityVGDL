
using System.Collections.Generic;

public class VGDLReplayer : VGDLPlayer
{
    /**
     * List of actions to execute. They must be loaded using loadActions().
     */
    private List<VGDLAvatarActions> actions;

    /**
     * Current index of the action to be executed.
     */
    private int actionIdx;


    /**
     * Public constructor with state observation and time due.
     * @param so state observation of the current game.
     * @param elapsedTimer Timer for the controller creation.
     * @param playerID ID of this player.
     */
    public VGDLReplayer()
    {
        actions = new List<VGDLAvatarActions>();
    }

    /**
     * Loads the action from the contents of the object received as parameter.
     * @param actionsToLoad ArrayList of actions to execute.
     */
    public void setActions(List<VGDLAvatarActions> actionsToLoad)
    {
        actionIdx = 0;
        this.actions = actionsToLoad;
    }

    /**
     * Picks an action. This function is called every game step to request an
     * action from the player.
     * @param stateObs Observation of the current state.
     * @param elapsedTimer Timer when the action returned is due.
     * @return An action for the current state
     */
    public override VGDLAvatarActions act(StateObservation stateObs, ElapsedCpuTimer elapsedTimer)
    {
        var action = actions[actionIdx];
        actionIdx++;

        var remaining = elapsedTimer.remainingTimeMilliseconds();
        while(remaining > 1)
        {
            //This allows visualization of the replay.
            remaining = elapsedTimer.remainingTimeMilliseconds();
        }

        return action;
    }

    /**
     * Picks an action. This function is called every game step to request an
     * action from the player.
     * @param stateObs Observation of the current state.
     * @param elapsedTimer Timer when the action returned is due.
     * @return An action for the current state
     */
    public override VGDLAvatarActions act(StateObservationMulti stateObs, ElapsedCpuTimer elapsedTimer)
    {
        var action = actions[actionIdx];
        actionIdx++;

        var remaining = elapsedTimer.remainingTimeMilliseconds();
        while(remaining > 1)
        {
            //This allows visualization of the replay.
            remaining = elapsedTimer.remainingTimeMilliseconds();
        }

        return action;
    }
}
