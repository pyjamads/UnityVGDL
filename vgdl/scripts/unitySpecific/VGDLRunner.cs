using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using UnityEngine;
using UnityEngine.Serialization;

public class VGDLRunner : MonoBehaviour
{
    public bool verbose;

    [Tooltip("Default value is 20fps")] public int frameRateTarget = 20;

    public VGDLGame game;

    [Tooltip("Determines whether game updates are handled by MonoBehavior or onDemand.")]
    public bool onDemandUpdates;

    public int skipFrames;

    public VGDLPlayerInterface[] players;
    
    [HideInInspector]
    public bool started;
    [HideInInspector]
    public bool ended;

    [Tooltip("RenderTarget for !onDemandUpdates instead of BackBuffer")]
    public RenderTexture renderTarget;

    private ElapsedCpuTimer StartTime;
    private int ticks;
    private int renderTicks;
    
    public float avgTicksPerSecond;
    public float avgRenderingsPerSecond;
    
    public int globalActionRequestCounter;
    public int globalObservationRequestCounter;
    
    public static int globalActionRequests;
    public static int globalObservationRequests;
    
    //Read Only
    public float smoothedScore;
    
    public int numberOfRunsToEvaluate = -1;
    public int runs;
    
    public float avgScore;
    private float sumScore;
    

    public float avgWins;
    private int sumWins;

    public void InitializePlayers(VGDLPlayerInterface[] players)
    {
        this.players = players;
    }
    
    public void InitializeGame(VGDLGameAndLevel gameToRun, int randomSeed = -1)
    {
        VGDLParser.verbose = verbose;
     
        //Try to remove all the old reference stuff...
        game?.reset();
        game = null;
        
        game = LoadGame(gameToRun);

        
        if (players == null || players.Length < game.no_players)
        {
            var playerArr = new VGDLPlayerInterface[game.no_players];

            for (int i = 0; i < playerArr.Length; i++)
            {
                if (players.Length > i)
                {
                    playerArr[i] = players[i];
                }
                else
                {
                    var agent = new VGDLHumanAgent();
                    agent.PlayerID = i;
                    playerArr[i] = agent;    
                }
            }
        }
        
        game.prepareGame(players, randomSeed);

        started = false;
        ended = false;

        Time.captureFramerate = frameRateTarget;
    }
    
    public void RunGame(VGDLGameAndLevel gameToRun, int randomSeed = -1)
    {
        if (numberOfRunsToEvaluate > 0 && runs >= numberOfRunsToEvaluate) return;
        
        InitializeGame(gameToRun, randomSeed);
        
        started = true;
        StartTime = new ElapsedCpuTimer();
        ticks = 0;
        renderTicks = 0;
    }

    public void ExecuteRendering(RenderTexture renderTarget, bool OnGUI = false)
    {   
        if (game == null || StartTime == null) return;
        
        VGDLRenderHelper.RenderGameUsingDrawTexture(game, renderTarget, OnGUI);
        
        renderTicks++;

        avgRenderingsPerSecond = renderTicks / (float)StartTime.Elapsed.TotalSeconds;
    }

    public void ExecuteUpdate()
    {
        if (numberOfRunsToEvaluate > 0 && runs >= numberOfRunsToEvaluate) return;
        
        if (ended || game == null || StartTime == null) return;
        
        
//        //Set Framerate to -1 for unlimited fps
//        if (frameRateTarget > 0)
//        {
//            var fixedStepTime = 1.0f / frameRateTarget;
//            var nextUpdateStep = (game.gameTick) * fixedStepTime;
//            
//            if (((float)StartTime.Elapsed.TotalSeconds) < nextUpdateStep) return;	
//        }
        
        game.updateGameState();

        ticks++;

        avgTicksPerSecond = ticks / (float)StartTime.Elapsed.TotalSeconds;
        
        if (!game.isEnded) return;

        ended = true;

        LogResult();
    }

    public void LogResult(bool timeout = false)
    {
        var scores = game.handleResult();
        smoothedScore = smoothedScore * 0.95f + scores[0] * 0.05f;

        sumScore += scores[0];
        sumWins += game.getWinner() == VGDLPlayerOutcomes.PLAYER_WINS ? 1 : 0;
        runs++;

        avgScore = sumScore / (runs * 1.0f);
        avgWins = sumWins / (runs * 1.0f);
        
        if (!timeout)
        {
            Debug.Log(name + "["+transform.GetInstanceID()+"]: Game Over! score: "+scores[0]);    
        }
        else
        {
            Debug.Log(name + "["+transform.GetInstanceID()+"]: Timeout! score: "+scores[0]);
        }
        
        
        //TODO: show result better.
//        Debug.Log("GAME OVER!\n" +
//                  (game.gameResult.playerOutcomes[0] == VGDLPlayerOutcomes.PLAYER_WINS ? "YOU WON!" : "YOU LOST!") +
//                  " Score: " + game.gameResult.playerScores[0]);
    }
    
    #region MonoBehavior Driven Updates

    //private void OnPostRender() //NOTE: On post render only happens on camera's
    private void OnGUI()
    {
        if (!started || onDemandUpdates) return;
        
        //if ((skipFrames > 0) && ((ticks % skipFrames) != 0)) return;
        
        //NOTE: use this line if using OnGUI
        if (Event.current.type.Equals(EventType.Repaint))
        {
            if (renderTarget != null)
            {
                ExecuteRendering(renderTarget, true);
            }
            //NOTE: Renders to BackBuffer.
            ExecuteRendering(null, true);
        }
    
    }

    /// <summary>
    /// Updates the game state, unless onDemandUpdates is true.
    /// </summary>
    void Update()
    {
        globalActionRequestCounter = globalActionRequests;
        globalObservationRequestCounter = globalObservationRequests;
        
        if (!started || onDemandUpdates || ended) return;
        
        ExecuteUpdate();
    }
    #endregion
    
    #region VGDL Loading
    public static VGDLGame LoadGame(VGDLGameAndLevel gameToLoad, bool loadLevelsFromGameTree = false)
    {
        var exampleToLoad = "";
        BasicGame game = null;
        switch (gameToLoad.type)
        {
            case VGDLExampleTypes.Unspecified:
                exampleToLoad += gameToLoad.filename;
                //File.ReadAllText
                break;
            case VGDLExampleTypes.GridPhysics:
                exampleToLoad += Path.Combine("gridphysics", gameToLoad.filename);
                game = LoadFromExampleString(exampleToLoad, loadLevelsFromGameTree);
                break;
            case VGDLExampleTypes.ContinuousPhysics:
                exampleToLoad += Path.Combine("contphysics", gameToLoad.filename);
                game = LoadFromExampleString(exampleToLoad, loadLevelsFromGameTree);
                break;
            case VGDLExampleTypes.TwoPlayer:
                exampleToLoad += Path.Combine("2player", gameToLoad.filename);
                game = LoadFromExampleString(exampleToLoad, loadLevelsFromGameTree);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        //NOTE: Something went wrong
        if (game == null)
        {
            return null;
        } 
        
        game.name = exampleToLoad;
        
        if (loadLevelsFromGameTree)
        {
            if (game.levelList.Count > gameToLoad.level)
            {
                game.buildLevelFromLines(game.levelList[gameToLoad.level]);
            }
            else
            {
                Debug.LogError("Level "+gameToLoad.level+" not defined (levels are indexed from Zero)");
            }
        }
        else
        {
            if (gameToLoad.type == VGDLExampleTypes.Unspecified)
            {
                //NOTE: we expect the levels to be located next to the game specification and named filename_lvl[#level].txt
                LoadLevelFromFile(game, gameToLoad.filename+"_lvl"+gameToLoad.level+".txt");
            }
            else
            {
                LoadLevelFromExampleString(game, exampleToLoad, gameToLoad.level);
            }
        }

        return game;
    }
    
    private static BasicGame LoadFromExampleString(string exampleToLoad, bool loadLevelsFromGameTree)
    {
        var filename = Path.Combine("vgdl", Path.Combine("examples", exampleToLoad));
        return ParseGameFromFile(filename, loadLevelsFromGameTree);
    }

    private static void LoadLevelFromExampleString(BasicGame game, string example, int level)
    {
        var exampleLevelStr = Path.Combine("vgdl", Path.Combine("examples", example))+"_lvl"+level;
        //NOTE: we expect the levels to be located in the same folder and called [gamename]_lvl[levelnum].txt 
        LoadLevelFromFile(game, exampleLevelStr);
    }

    private static BasicGame ParseGameFromFile(string filename, bool loadLevelsFromGameTree)
    {
        var gamedef = Resources.Load<TextAsset>(filename);

        if (gamedef == null)
        {
            Debug.LogError("Failed to load game: "+filename);
            return null;
        }
        
        return VGDLParser.ParseGame(gamedef.text, loadLevelsFromGameTree);
    }

    private static void LoadLevelFromFile(BasicGame game, string filename)
    {
        game.buildLevelFromFile(filename);
    }
    #endregion
}