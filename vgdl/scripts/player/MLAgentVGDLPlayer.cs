
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MLAgents;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// This class is a hybrid actor in the Unity MLAgent & VGDL setup,
/// it acts both as an agent (MLAgent) collecting information and relaying the decided action,
/// as well as driving the updateGameState in the current VGDLGame listed in the MLAgentVGDLAcademy
/// and loaded into the VGDLRunner.
/// </summary>
public class MLAgentVGDLPlayer : Agent, VGDLPlayerInterface
{
    public VGDLRunner runner;
    public RawImage image;
    
    [Header("Specific to MLAgent implementation")]
    public float[] lastVectorAction;

    public StateObservation lastGameObservation;

    [Tooltip("Reward added at each step, used to encourage movement.")]
    public float stepReward;

    private float lastScore;
    
    public override void InitializeAgent()
    {
        if (runner == null)
        {
            throw new UnityAgentsException(
                "No Runner Component attached.");
        }

        if (agentParameters.agentRenderTextures.Count > 0 && agentParameters.agentRenderTextures[0] == null)
        {
            agentParameters.agentRenderTextures[0] = 
                new RenderTexture(brain.brainParameters.cameraResolutions[0].width, brain.brainParameters.cameraResolutions[0].height, 0);
            agentParameters.agentRenderTextures[0].filterMode = FilterMode.Point;
            
            if (image != null && image.gameObject.activeSelf)
            {
                image.texture = agentParameters.agentRenderTextures[0];
            }
        }

        lastScore = 0;
        
        //agentParameters.resetOnDone = false;
        //runner.onDemandUpdates = true;
        
        //NOTE: 2player games won't work in this setup, because only one player is passed.
        runner.InitializePlayers(transform.GetComponents<VGDLPlayerInterface>());
        runner.skipFrames = agentParameters.numberOfActionsBetweenDecisions;

        var academy = FindObjectOfType<MLAgentVGDLAcademy>();
        runner.RunGame(academy.GetNextGameAndLevel());
    }

    public override void CollectObservations()
    {
        VGDLRunner.globalObservationRequests += 1;
        
        //NOTE: first version of MLAgent for VGDL will use visual observations only and no vector observations
        if (runner.onDemandUpdates)
        {
            if (agentParameters.agentRenderTextures.Count > 0)
            {
                runner.ExecuteRendering(agentParameters.agentRenderTextures[0]);
            }
            else if (image != null)
            {
                runner.ExecuteRendering(image.texture as RenderTexture);
            }
            else
            {
                //Just render to back buffer
                runner.ExecuteRendering(null);
            }
        }
        
        //Set Rewards before collecting observations
        if (lastGameObservation != null && !lastGameObservation.isGameOver())
        {
            //TODO: set action mask based on available actions
            //SetActionMask();

            var latestObsScore = lastGameObservation.getGameScore();

            var multiObs = lastGameObservation as StateObservationMulti;
            if (multiObs != null)
            {
                latestObsScore = multiObs.getGameScore(PlayerID);    
            }
            
            var diff = latestObsScore - lastScore;
            AddReward(diff);
            
            lastScore = latestObsScore;
        }
    }

    
    /// <summary>
    /// Note vectorAction will be length 1 for discrete actions
    /// </summary>
    /// <param name="vectorAction"></param>
    /// <param name="textAction"></param>
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        VGDLRunner.globalActionRequests += 1;
        
        AddReward(stepReward);
        
        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            //TODO: implement continuous/analogue input in VGDL framework.
            throw new NotImplementedException("SpaceType.continuous not implemented");
        }
        
        

        //Store latest action
        lastVectorAction = vectorAction;

        if (runner.onDemandUpdates)
        {
            //NOTE: this updates the VGDL game and makes a callback to our act function.
            runner.ExecuteUpdate();    
        }
    }

    public override void AgentReset()
    {
        //NOTE: with the ResetOnDone = false, this is probably not needed.
        //TODO: implement agent reset... eg. reset game?

        if (IsMaxStepReached())
        {
            //Log the result as a timeout.
            runner.LogResult(true);   
        }
        
        lastScore = 0;
        lastGameObservation = null;
        
        var academy = FindObjectOfType<MLAgentVGDLAcademy>();
        runner.RunGame(academy.GetNextGameAndLevel());
    }

    public override void AgentOnDone()
    {
        
    }
    
    
    [Header("Specific to VGDLPlayer implementation")]
    /**
    * File where the actions played in a given game are stored.
    */
    public string actionFile;

    /**
     * Set this variable to FALSE to avoid core.logging the actions to a file.
     */
    private const bool SHOULD_LOG = true;

    /**
   * playerID
   */
    public int PlayerID
    {
        get { return _playerId; }
        set { _playerId = value; }
    }
    
    [SerializeField]
    private int _playerId;

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
    public VGDLAvatarActions act(StateObservation stateObs, ElapsedCpuTimer elapsedTimer)
    {
        lastGameObservation = stateObs;

        if (lastVectorAction == null || lastVectorAction.Length < 1) return VGDLAvatarActions.ACTION_NIL;

        return  (VGDLAvatarActions) Mathf.RoundToInt(lastVectorAction[0]);
    }


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
    public VGDLAvatarActions act(StateObservationMulti stateObs, ElapsedCpuTimer elapsedTimer)
    {
        lastGameObservation = stateObs;

        if (lastVectorAction == null || lastVectorAction.Length < 1) return VGDLAvatarActions.ACTION_NIL;

        return  (VGDLAvatarActions) Mathf.RoundToInt(lastVectorAction[0]);
    }


    /**
     * Function called when the game is over. This method must finish before CompetitionParameters.TEAR_DOWN_TIME,
     *  or the agent will be DISQUALIFIED
     * @param stateObs the game state at the end of the game
     * @param elapsedCpuTimer timer when this method is meant to finish.
     */
    public void result(StateObservation stateObs, ElapsedCpuTimer elapsedCpuTimer)
    {
        Done();
        
        var diff = stateObs.getGameScore() - lastScore;
        AddReward(diff);
        lastScore = stateObs.getGameScore();
        lastGameObservation = stateObs;
        
        //SetReward(stateObs.getGameScore());
    }

    /**
     * Function called when the game is over. This method must finish before CompetitionParameters.TEAR_DOWN_TIME,
     *  or the agent will be DISQUALIFIED
     * @param stateObs the game state at the end of the game
     * @param elapsedCpuTimer timer when this method is meant to finish.
     */
    public void result(StateObservationMulti stateObs, ElapsedCpuTimer elapsedCpuTimer)
    {
        Done();
        
        var diff = stateObs.getGameScore(PlayerID) - lastScore;
        AddReward(diff);
        lastScore = stateObs.getGameScore(PlayerID);
        lastGameObservation = stateObs;
        
        //SetReward(stateObs.getGameScore(PlayerID));
    }

    /**
     * This function sets up the controller to save the actions executed in a given game.
     * @param actionFile file to save the actions to.
     * @param randomSeed Seed for the sampleRandom generator of the game to be played.
     * @param isHuman Indicates if the player is a human or not.
     */
    public void setup(string actionFile, int randomSeed, bool isHuman)
    {
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
    public void teardown(VGDLGame played)
    {
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
    public void logAction(VGDLAvatarActions action)
    {
        lastAction = action;
        
        if(!string.IsNullOrEmpty(this.actionFile) && SHOULD_LOG)
        {
            allActions.Add(action);
        }
    }
}
