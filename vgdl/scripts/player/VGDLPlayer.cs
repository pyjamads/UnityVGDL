using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using Debug = UnityEngine.Debug;

public abstract class VGDLPlayer : VGDLPlayerInterface
{
    /**
    * playerID
    */
    public int PlayerID { get; set; }

    /**
     * File where the actions played in a given game are stored.
     */
    private string actionFile;

    /**
     * Set this variable to FALSE to avoid core.logging the actions to a file.
     */
    protected const bool SHOULD_LOG = true;

    /**
     * Last action executed by this agent.
     */
    public VGDLAvatarActions lastAction { get; private set; }

    /**
     * List of actions to be dumped.
     */
    public List<VGDLAvatarActions> allActions;

    /**
     * Random seed of the game.
     */
    private int randomSeed;

    /**
     * Is this a human player?
     */
    public bool isHuman
    {
        get;
        protected set;
    }
    
    /**
     * Picks an action. This function is called every game step to request an
     * action from the player. The action returned must be contained in the
     * actions accessible from stateObs.getAvailableActions(), or no action
     * will be applied.
     * Single Player method.
     * @param stateObs Observation of the current state.
     * @param elapsedTimer Timer when the action returned is due.
     * @return An action for the current state
     */
    public abstract VGDLAvatarActions act(StateObservation stateObs, ElapsedCpuTimer elapsedTimer);


    /**
     * Picks an action. This function is called every game step to request an
     * action from the player. The action returned must be contained in the
     * actions accessible from stateObs.getAvailableActions(), or no action
     * will be applied.
     * Multi player method.
     * @param stateObs Observation of the current state.
     * @param elapsedTimer Timer when the action returned is due.
     * @return An action for the current state
     */
    public abstract VGDLAvatarActions act(StateObservationMulti stateObs, ElapsedCpuTimer elapsedTimer);


    /**
     * Function called when the game is over. This method must finish before CompetitionParameters.TEAR_DOWN_TIME,
     *  or the agent will be DISQUALIFIED
     * @param stateObs the game state at the end of the game
     * @param elapsedCpuTimer timer when this method is meant to finish.
     */
    public void result(StateObservation stateObs, ElapsedCpuTimer elapsedCpuTimer)
    {
    }

    public void result(StateObservationMulti stateObs, ElapsedCpuTimer elapsedCpuTimer)
    {
    }

    /**
     * This function sets up the controller to save the actions executed in a given game.
     * @param actionFile file to save the actions to.
     * @param randomSeed Seed for the sampleRandom generator of the game to be played.
     * @param isHuman Indicates if the player is a human or not.
     */
    public void setup(string actionFile, int randomSeed, bool isHuman) {
        this.actionFile = actionFile;
        this.randomSeed = randomSeed;
        this.isHuman = isHuman;

        if(!string.IsNullOrEmpty(this.actionFile) && SHOULD_LOG)
        {
            allActions = new List<VGDLAvatarActions>();
        }
    }

    /**
     * Closes the agent, writing actions to file.
     */
    public void teardown(VGDLGame played) {
        try
        {
            if (!string.IsNullOrEmpty(actionFile) && SHOULD_LOG)
            {
                var fileStream = File.OpenWrite(actionFile);
                var streamWriter = new StreamWriter(fileStream);
            
                //NOTE Mads 5/12-2018: this should probably use playerID to get winner/scores in case of >2 players
                streamWriter.WriteLine(randomSeed +
                                       " " + (played.getWinner() == VGDLPlayerOutcomes.PLAYER_WINS ? 1 : 0) +
                                       " " + played.getScore() + " " + played.getGameTick());

                foreach(var act in allActions)
                    streamWriter.WriteLine(Enum.GetName(typeof(VGDLAvatarActions), act));

                streamWriter.Close();
            }
        } catch (IOException e) {
            Debug.Log(e.StackTrace);
        }
    }

    /**
     * Logs a single action
     * @param action the action to log.
     */
    public void logAction(VGDLAvatarActions action) {

        lastAction = action;
        
        if(!string.IsNullOrEmpty(this.actionFile) && SHOULD_LOG)
        {
            allActions.Add(action);
        }
    }

    /// <summary>
    /// Loads an action list from an action file. Skips first line.
    /// </summary>
    /// <param name="file"></param>
    /// <returns>null if File doesn't exist or is empty, otherwise a list of all loaded actions</returns>
    public static List<VGDLAvatarActions> loadActionsFromFile(string file)
    {
        if (!File.Exists(file)) return null; 
        
        var lines = File.ReadAllLines(file);
        
        //First line contains randomSeed, winner, score, and gameTick
        //var initLine = lines[0]; //Init/Outcome line

        //Read all actions into result list;
        if (lines.Length > 0)
        {
            var result = new List<VGDLAvatarActions>();
            
            for (int i = 1; i < lines.Length; i++)
            {
                result.Add((VGDLAvatarActions)Enum.Parse(typeof(VGDLAvatarActions), lines[i], true));
            }

            return result;
        }

        return null;
    }
    
    //TODO: consider implementing a debug draw option.
    /**
     * Gets the player the control to draw something on the screen.
     * It can be used for debug purposes.
     * @param g Graphics device to draw to.
     */
//    public void draw(Graphics2D g)
//    {
//        //Overwrite this method in your controller to draw on the screen.
//        //This method should be left empty in this class.
//    }
}