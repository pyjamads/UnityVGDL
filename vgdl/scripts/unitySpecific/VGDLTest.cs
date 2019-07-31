using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using vgdl.scripts.player;
using Random = System.Random;

public class VGDLTest : MonoBehaviour
{
	public bool verbose;

	//[Tooltip("Default value is 20fps")]
	//public int frameRateTarget = 20;

	//public VGDLUnityView view;
	
	public int StepsBetweenRendering = 1;
	public int maxSteps = 1500;
	
	
	//public int levelToLoad;

	//public bool setupCameraAndGameView;

	//public bool headless;

	//public bool saveFrames;

	//public MLAgentVGDLPlayer[] players;

	public RenderTexture renderTarget;
	
	private static int parserTestErrors = 0;

	private bool shouldRender;

	private BasicGame currentGame;
	
	[Tooltip("Feature to combine level and game description into one file.\n" +
	         "Levels\n" +
	         "  Level1 >\n" +
	         "   wwwwwwwwwwwwwwwwwwwww\n" +
	         "   w...................w\n" +
	         "   w5555555555555555555w\n" +
	         "   w4444444444444444444w\n" +
	         "   w3333333333333333333w\n" +
	         "   w2222222222222222222w\n" +
	         "   w1111111111111111111w\n" +
	         "   w.........o.........w\n" +
	         "   w...................w\n" +
	         "   w...................w\n" +
	         "   w.........A.........w\n" +
	         "   w-------------------w\n" +
	         "   wwwwwwwwwwwwwwwwwwwww\n" +
	         "  Level2 >\n" +
	         "   wwwwwwwwwwwwwwwwwwwww\n" +
	         "   w...................w\n" +
	         "   w5555555555555555555w\n" +
	         "   w..444444444444444..w\n" +
	         "   w....33333333333....w\n" +
	         "   w.......22222.......w\n" +
	         "   w........111........w\n" +
	         "   w.........o.........w\n" +
	         "   w...................w\n" +
	         "   w...................w\n" +
	         "   w.........A.........w\n" +
	         "   w-------------------w\n" +
	         "   wwwwwwwwwwwwwwwwwwwww\n")]
	public bool loadLevelsFromGameTree;
	
	public VGDLTestType type;
	
	public VGDLExampleTypes exampleTypesToLoad;
	
	[HideInInspector]
	public string exampleToLoad;

	[HideInInspector]
	public List<TextAsset> gamesToLoad;

	public enum VGDLTestType
	{
		LoadFromInputString,
		RunFromInputString,
		ParserTest,
		ParseAndRunTest,
	}
	
	// Use this for initialization
	IEnumerator Start ()
	{
		VGDLParser.verbose = verbose;

		//Make the system run as fast as possible, (Importantly set the VSync to 0/Don't Sync in QualitySettings)
		//Application.targetFrameRate = 1000; //Default frame rate limit on StandAlone platforms like PC and MAC is -1 (eg. as fast as possible)
		
		//BasicGame game;
		
		var examplesPath = Path.Combine("vgdl", "examples");

		switch (exampleTypesToLoad)
		{
			case VGDLExampleTypes.GridPhysics:
				examplesPath = Path.Combine(examplesPath, "gridphysics");
				break;
			case VGDLExampleTypes.ContinuousPhysics:
				examplesPath = Path.Combine(examplesPath, "contphysics");
				break;
			case VGDLExampleTypes.TwoPlayer:
				examplesPath = Path.Combine(examplesPath, "2player");
				break;
			default:
			case VGDLExampleTypes.Unspecified:
				//Do Nothing, this won't actually work, because the path is incomplete...
				break;
		}
		
		switch (type)
		{
			case VGDLTestType.LoadFromInputString:
			case VGDLTestType.RunFromInputString:

				if (gamesToLoad != null && gamesToLoad.Count > 0)
				{
					StartCoroutine(parserTest(type == VGDLTestType.RunFromInputString, gamesToLoad, examplesPath));
				}
				else
				{
					examplesPath = Path.Combine(examplesPath, exampleToLoad);

					var examples = Resources.LoadAll<TextAsset>(examplesPath);
					
					StartCoroutine(parserTest(type == VGDLTestType.RunFromInputString, examples, examplesPath));
				}
				break;
			
			case VGDLTestType.ParserTest:
			case VGDLTestType.ParseAndRunTest:
				if (gamesToLoad != null && gamesToLoad.Count > 0)
				{
					StartCoroutine(parserTest(type == VGDLTestType.ParseAndRunTest, gamesToLoad, examplesPath));
				}
				else
				{
					StartCoroutine(parserTest(type == VGDLTestType.ParseAndRunTest));	
				}

				break;
		}

		return null;
//		if (game != null)
//		{
//			if (loadLevelsFromGameTree)
//			{
//				if (game.levelList.Count > levelToLoad)
//				{
//					game.buildLevelFromLines(game.levelList[levelToLoad]);
//				}
//				else
//				{
//					Debug.LogError("Level "+levelToLoad+" not defined (levels are indexed from Zero)");
//				}
//			}
//			else
//			{
//				if (type == VGDLTestType.LoadFromInputString)
//				{
//					LoadLevelFromExampleString(game, exampleToLoad, levelToLoad);
//				}
//				
//				if (type == VGDLTestType.InternalCustomString)
//				{
//					var lines =  new[]{ "Awwwwwwwwwwwwwwwwwwww",
//										"w.....ww..wwwwww....w",
//										"w.....ww.hhh..hww...w",
//										"w..x..w..wwwwww..ww.w",
//										"w..whww.wwwwww.w....w",
//										"w..ww...wwwwwhww....w",
//										"w..ww.hwwwwwwwww....w",
//										"w.....hw..w..ww.....w",
//										"w..w..hw....wwww....w",
//										"w................e..w",
//										"wwwwwwwwwwwwwwwwwwwww"};
//					game.buildLevelFromLines(lines);
//				}
//			}
//
//			//game.setFrameRateTarget(frameRateTarget);
//			
//			//Do unity rendering
//			//var view = FindObjectOfType<VGDLUnityView>();
//			view.game = game;
//			
//			//Hack to generate the correct size, because the updated resolution doesn't kick in properly until the next time we run it.
//			if (setupCameraAndGameView)
//			{
//				view.SetupCameraAndResolution(game.name);
//				setupCameraAndGameView = false;
//				
//				Debug.Log("Remember to go to the VGDL menu and select VGDL/SetupCameraDynamic");
//			}
//			else
//			{
//				//init & play game
//				//view.RegenerateGameFromVGDL();
//
//				if (headless)
//				{
//					//game.setFrameRateTarget(-1);
//					view.headless = true;
//					
//				}
//
//				var agents = new VGDLPlayerInterface[game.no_players];
//
//				if (players != null && players.Length > 0)
//				{
//					for (int i = 0; i < agents.Length; i++)
//					{
//						agents[i] = players[i];
//					}	
//				}
//				else
//				{
//					for (int i = 0; i < agents.Length; i++)
//					{
//						agents[i] = new VGDLHumanAgent();
//						//TODO: figure out if we need to handle alt_keys here.
//					}	
//				}
//				
//				view.RunGame(agents, -1, saveFrames);
//				
//				//TODO show result
//				//Wait for view.ended
//			}
//			
//			//TODO: make a menu button, to generate and save the whole thing
//			//TODO: make a menu toggle button, to allow automatic updating, when the text file is changed.
//			//TODO: editor highlighting? => needs it's own file extension then. => "*.vgdl"
//		}
//		
//		return null;
	}

	private BasicGame loadThisString()
	{
		string gameStr = "\nBasicGame" +
		                 "\n    SpriteSet" +
		                 "\n        floor > Immovable img=newset/highway3 hidden=True"+
		                 "\n        goal > Immovable color=GREEN " +
		                 "\n            othergoal > " +
		                 "\n            mygoal    >" +
		                 "\n        racket > VerticalAvatar speed=0.25" +
		                 "\n            avatar      > alternate_keys=True" +
		                 "\n            otheravatar > color=BLUE " +
		                 "\n        ball > Missile orientation=LEFT speed=15 color=ORANGE physicstype=NoFrictionPhysics" +
		                 "\n            " +
		                 "\n    TerminationSet # from the perspective of player 1 (on the left)" +
		                 "\n        SpriteCounter stype=othergoal limit=6 win=False     " +
		                 "\n        SpriteCounter stype=mygoal    limit=6 win=True     " +
		                 "\n        MultiSpriteCounter stypes=[mygoal,othergoal]    limit=6 win=True     " +
		                 "\n        MultiSpriteCounter stype1=avatar stype2=otheravatar    limit=2 win=True     " +
		                 "\n           " +
		                 "\n    InteractionSet" +
		                 "\n        goal  ball   > killSprite" +
		                 "\n        ball racket > bounceDirection" +
		                 "\n        ball wall   > wallBounce" +
		                 "\n        racket wall > stepBack" +
		                 "\n        " +
		                 "\n    LevelMapping" +
		                 "\n        - > mygoal" +
		                 "\n        + > othergoal" +
		                 "\n        a > otheravatar" +
		                 "\n        o > ball" +
		                 "\n        . > floor"+
		                 "\n	Levels #test to see if levels would lex okay" +
		                 "\n        Level1 >" +
		                 "\n            wwwwwwwwwwwwwwwwwwwww" +
		                 "\n            w.....ww..wwwwww....w" +
		                 "\n            w.....ww.hhh..hww...w" +
		                 "\n            w..x..w..wwwwww..ww.w" +
		                 "\n            w..whww.wwwwww.w....w" +
		                 "\n            w..ww...wwwwwhww....w" +
		                 "\n            w..ww.hwwwwwwwww....w" +
		                 "\n            w.....hw..w..ww.....w" +
		                 "\n            w..w..hw....wwww....w" +
		                 "\n            w..A.............e..w" +
		                 "\n            wwwwwwwwwwwwwwwwwwwww";

		string gameStr2 = "\nBasicGame square_size=30"+
		                  "\n    SpriteSet"+
		                  "\n        floor > Immovable img=newset/highway3 hidden=True"+
		                  "\n        street > Immovable img=newset/street3 hidden=True"+
		                  "\n        moving >"+
		                  "\n"+
		                  "\n            motorbike >"+
		                  "\n                avatar  > OngoingShootAvatar speed=0.2 color=YELLOW img=newset/camel1 healthPoints=50 limitHealthPoints=50 stype=boost"+
		                  "\n                    avatarFast > MissileAvatar speed=0.5"+
		                  "\n                mbike   > Missile orientation=RIGHT  img=newset/camel2"+
		                  "\n                    mbikeSlow > speed=0.2"+
		                  "\n                    mbikeMed > speed=0.4"+
		                  "\n                    mbikeFast > speed=0.6"+
		                  "\n            cars >"+
		                  "\n                carSlow  > Missile orientation=LEFT speed=0.5 img=newset/car1"+
		                  "\n                carFast  > Missile orientation=LEFT speed=0.75 img=newset/car2"+
		                  "\n                truck    > Missile orientation=LEFT speed=0.9 img=newset/firetruckL"+
		                  "\n            statics >"+
		                  "\n                tree > Missile orientation=LEFT speed=0.8 img=newset/tree2"+
		                  "\n                goal > Missile orientation=LEFT speed=0.5 img=newset/exit2 portal=True"+
		                  "\n"+
		                  "\n        portal      > BomberRandomMissile invisible=True hidden=True stypeMissile=carSlow,carFast,truck cooldown=15 prob=0.5 total=80"+
		                  "\n        bikePortal  > BomberRandomMissile invisible=True hidden=True stypeMissile=mbikeSlow,mbikeMed,mbikeFast cooldown=20 prob=0.6  total=400"+
		                  "\n"+
		                  "\n        goalPortal > SpawnPoint invisible=True hidden=True stype=goal prob=1 total=1 cooldown=25"+
		                  "\n        treePortal   > SpawnPoint invisible=True hidden=True stype=tree cooldown=2 total=400"+
		                  "\n"+
		                  "\n        winnerNPC > Immovable color=RED img=oryx/sparkle2"+
		                  "\n        winnerPlayer > Immovable color=PINK img=oryx/sparkle1"+
		                  "\n"+
		                  "\n        boost > OrientedFlicker invisible=True hidden=True limit=1 singleton=True"+
		                  "\n"+
		                  "\n    InteractionSet"+
		                  "\n        avatar EOS  > stepBack"+
		                  "\n        avatar cars tree > subtractHealthPoints"+
		                  "\n        avatar TIME > transformToAll stype=portal stypeTo=goalPortal nextExecution=500 timer=500 repeating=False"+
		                  "\n        mbike cars tree > killSprite"+
		                  "\n        tree EOS    > killSprite"+
		                  "\n        cars EOS    > killSprite"+
		                  "\n        statics EOS > killSprite"+
		                  "\n"+
		                  "\n        mbike goal > transformTo stype=winnerNPC"+
		                  "\n        avatar goal > spawn stype=winnerPlayer"+
		                  "\n        goal avatar > killSprite"+
		                  "\n"+
		                  "\n        avatar boost > addTimer timer=10 ftype=transformToAll stype=avatarFast stypeTo=avatar forceOrientation=True"+
		                  "\n        avatar boost > transformTo stype=avatarFast killSecond=True forceOrientation=True"+
		                  "\n"+
		                  "\n    LevelMapping"+
		                  "\n        b > bikePortal floor"+
		                  "\n        A > avatar floor"+
		                  "\n        x > tree street"+
		                  "\n        p > portal floor"+
		                  "\n        t > treePortal street"+
		                  "\n        s > carSlow floor"+
		                  "\n        c > carFast floor"+
		                  "\n        . > floor"+
		                  "\n        + > street"+
		                  "\n"+
		                  "\n    TerminationSet"+
		                  "\n        MultiSpriteCounter stype1=winnerPlayer limit=1 win=True"+
		                  "\n        MultiSpriteCounter stype1=winnerNPC limit=1 win=False"+
		                  "\n        SpriteCounter stype=avatar limit=0 win=False";
		
		return parseGameFromString(gameStr);
	}

	private IEnumerator parserTest(bool run = false, IEnumerable<TextAsset> examples = null, string examplesPath = null)
	{
		if (examples == null || examplesPath == null)
		{
			examplesPath = Path.Combine("vgdl", "examples");

			switch (exampleTypesToLoad)
			{
				case VGDLExampleTypes.GridPhysics:
					examplesPath = Path.Combine(examplesPath, "gridphysics");
					break;
				case VGDLExampleTypes.ContinuousPhysics:
					examplesPath = Path.Combine(examplesPath, "contphysics");
					break;
				case VGDLExampleTypes.TwoPlayer:
					examplesPath = Path.Combine(examplesPath, "2player");
					break;
				default:
				case VGDLExampleTypes.Unspecified:
					//Do Nothing, this won't actually work, because the path is incomplete...
					break;
			}

			examples = Resources.LoadAll<TextAsset>(examplesPath);
		}

		yield return new WaitForEndOfFrame();

		foreach (var example in examples)
		{
			yield return ExampleParser(run, example, examplesPath);
		}


		if (parserTestErrors == 0)
		{
			Debug.LogWarning("PARSER TEST COMPLETED SUCCESSFULLY! ");
		}
		else
		{
			Debug.LogWarning("PARSER TEST COMPLETED WITH [" + parserTestErrors + "] ERRORS!");
		}

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif	
		yield return null;
	}

	private IEnumerator ExampleParser(bool run, TextAsset example, string examplesPath)
	{
		if (example.name.Contains("lvl"))
		{
			//Debug.Log(">>> SKIPPING: "+example.name);
			yield return null;
			yield break;
		}

		Debug.Log(">>> PARSING: " + example.name);
		yield return null;

		try
		{
			currentGame = parseGameFromString(example.text);
		}
		catch (Exception e)
		{
			parserTestErrors++;
			Debug.LogException(e);
			Debug.LogWarning("PARSER TESTS FAILED: " + parserTestErrors);

			//StopCoroutine("parserTest");
			//Debug.Break();

			yield break;
		}

		for (int i = 0; i < 5; i++)
		{
			var levelString = Path.Combine(examplesPath, example.name);
			levelString += "_lvl" + i;
			
			Debug.Log(">>> PARSING LEVEL: " + levelString);
			yield return null;

			try
			{
				currentGame.buildLevelFromFile(levelString);
			}
			catch (Exception e)
			{
				parserTestErrors++;
				Debug.LogException(e);
				Debug.LogWarning("PARSER TESTS FAILED: " + parserTestErrors);

				//StopCoroutine("parserTest");
				//Debug.Break();

				continue;
			}

			if (run)
			{
				Debug.Log(">>> RUNNING LEVEL: " + levelString);
				yield return RunGameRandomly(currentGame);
				currentGame.reset();
				currentGame = parseGameFromString(example.text);
			}
			else
			{
				yield return new WaitForEndOfFrame();
			}
		}

		yield return new WaitForEndOfFrame();
	}

	private IEnumerator RunGameRandomly(BasicGame game)
	{
		var agents = new VGDLPlayerInterface[game.no_players];

		for (int i = 0; i < agents.Length; i++)
		{
			agents[i] = new VGDLRandomAgent();
		}

		game.prepareGame(agents);
		
		
		var ended = false;

		var stepsUntilRendering = 1;
		
		while (!ended && game.getGameTick() < maxSteps)
		{
			try
			{
				game.updateGameState();
			}
			catch (Exception e)
			{
				parserTestErrors++;
				Debug.LogException(e);
				Debug.LogWarning("PARSER TESTS FAILED: "+parserTestErrors);
						
				//StopCoroutine("parserTest");
				//Debug.Break();
				break;
			}
			
			if (stepsUntilRendering <= 0)
			{
				shouldRender = true;	
				stepsUntilRendering = StepsBetweenRendering;
				yield return null;
			}
			
			stepsUntilRendering--;
			
			if (!game.isEnded) continue;
			
			ended = true;

			var scores = game.handleResult();
			
			Debug.Log("GAME OVER!\n"+(game.gameResult.playerOutcomes[0] == VGDLPlayerOutcomes.PLAYER_WINS ? "YOU WON!" : "YOU LOST!")+" Score: "+game.gameResult.playerScores[0]);
		}

		if (game.getGameTick() >= maxSteps)
		{	
			Debug.Log("TIMEOUT!\n");
		}
		
		yield return null;
	}

	private void OnPostRender()
	{
		if (shouldRender)
		{
			VGDLRenderHelper.RenderGameUsingDrawTexture(currentGame, renderTarget);

			shouldRender = false;
		}	
	}

	private BasicGame loadFromExampleString()
	{
		var exampleGameStr = File.ReadAllText(Path.Combine("Assets", Path.Combine("vgdl", Path.Combine("examples", exampleToLoad)))+".txt");
		return parseGameFromString(exampleGameStr);
	}

	private void loadLevelFromExampleString(BasicGame game, string example, int level)
	{
		var exampleLevelStr = Path.Combine("Assets", Path.Combine("vgdl", Path.Combine("examples", example)))+"_lvl"+level+".txt";
		game.buildLevelFromFile(exampleLevelStr);
	}

	private BasicGame parseGameFromString(string gamestr)
	{
		return VGDLParser.ParseGame(gamestr, loadLevelsFromGameTree);
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
	
}
