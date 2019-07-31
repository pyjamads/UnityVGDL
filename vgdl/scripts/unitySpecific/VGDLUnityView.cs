using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;


public class VGDLUnityView : MonoBehaviour
{
	public VGDLGame game;

	public bool started;
	public bool ended;

	public bool headless;
	public bool saveFrames;

	public RenderTexture renderTarget;

	private ElapsedCpuTimer totalTime;
	
	// Update is called once per frame
	void Update () {
		if (started && !ended)
		{
			//var e = new ElapsedCpuTimer();
			game.updateGameState();

			
			//Debug.Log(""+1000/e.Elapsed.TotalMilliseconds);
			if (!game.isEnded) return;
			
			ended = true;

			var scores = game.handleResult();
			
			Debug.Log("TotalTime: "+totalTime.Elapsed.TotalSeconds+" avg. ticks/s: "+(game.getGameTick()/totalTime.Elapsed.TotalSeconds));
			
			//TODO: show result better.
			Debug.Log("GAME OVER!\n"+(game.gameResult.playerOutcomes[0] == VGDLPlayerOutcomes.PLAYER_WINS ? "YOU WON!" : "YOU LOST!")+" Score: "+game.gameResult.playerScores[0]);
		}
	}

	private bool firstFrame = true;

	void OnGUI()
	{	
		if (started)
		{
			if (!headless && !firstFrame)
			{
				if (Event.current.type.Equals(EventType.Repaint))
				{
//					var drawOrder = game.getSpriteOrder();
//					foreach (var stype in drawOrder)
//					{
//						foreach (var vgdlSprite in game.getSprites(stype))
//						{
//							if (vgdlSprite.image == null) continue;
//							if (vgdlSprite.invisible.CompareAndIgnoreCase("True")) continue;
//							
//							Graphics.DrawTexture(vgdlSprite.rect, vgdlSprite.image.texture);
//						}
//					}

					VGDLRenderHelper.RenderGameUsingDrawTexture(game, renderTarget);
				}
			}
			
			firstFrame = false;
		}
	}

	public void RunGame(VGDLPlayerInterface[] players, int randomSeed, bool saveFrames = false)
	{
		game.prepareGame(players, randomSeed);
		
		totalTime = new ElapsedCpuTimer();
		started = true;
		this.saveFrames = saveFrames;
		//TODO: think about headless vs saveFrames, should we be able to saveFrames with headless.
		if (this.saveFrames && !headless)
		{
			//TODO: select a better output location for frames.
			var path = Path.Combine(Application.dataPath, Path.Combine("Frames", "frame" + game.gameTick + ".png"));
			StartCoroutine(SavePNG(path));
		}
	}

	private IEnumerator SavePNG(string path)
	{
		var time = 0f;

		var lastTick = game.gameTick;

		while (true)
		{
			// We should only read the screen buffer after rendering is complete
			yield return new WaitForEndOfFrame();

			if (lastTick == game.gameTick) continue;
			lastTick = game.gameTick;
		
			// Create a texture the size of the screen, RGB24 format
			var width = (int)game.screenSize.x;
			var height = (int)game.screenSize.y;
			var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

			// Read screen contents into the texture
			tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			tex.Apply();

			// Encode texture into PNG
			byte[] bytes = tex.EncodeToPNG();
			Destroy(tex);
		
			File.WriteAllBytes(path, bytes);

			if (game.isEnded) break;
		}
		
	}


	public void SetupCameraAndResolution(string name = "VGDL")
	{
#if UNITY_EDITOR
		var sizeIndex = AddCustomSize(GameViewSizeType.FixedResolution, GameViewSizeGroupType.Standalone, (int)game.screenSize.x, (int)game.screenSize.y, name+" Viewer");
		SetSize(sizeIndex);
		SetupCameraOrthSize((int)game.screenSize.y);
//#else
//			Screen.SetResolution((int)game.screenSize.x, (int)game.screenSize.y, false);
//			VGDLUtils.SetupCameraOrthSize((int)game.screenSize.y);
#endif
	}

	 #region Game View size manipulation
#if UNITY_EDITOR
    public enum GameViewSizeType
    {
        AspectRatio,
        FixedResolution
    }

    /*[MenuItem("Test/AddSize")]
    public static void AddTestSize()
    {
        AddCustomSize(GameViewSizeType.AspectRatio, GameViewSizeGroupType.Standalone, 123, 456, "Test size");
    }
    */

    public static int AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width,
        int height, string text)
    {
        // goal:
        // var group = ScriptableSingleton<GameViewSizes>.instance.GetGroup(sizeGroupType);
        // group.AddCustomSize(new GameViewSize(viewSizeType, width, height, text);
        var asm = typeof(Editor).Assembly;
        var sizesType = asm.GetType("UnityEditor.GameViewSizes");
        var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instanceProp = singleType.GetProperty("instance");
        var getGroup = sizesType.GetMethod("GetGroup");
        var instance = instanceProp.GetValue(null, null);
        var group = getGroup.Invoke(instance, new object[] {(int) sizeGroupType});
        var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize"); // or group.GetType().
        var gvsType = asm.GetType("UnityEditor.GameViewSize");
        var ctor = gvsType.GetConstructor(new[] {typeof(int), typeof(int), typeof(int), typeof(string)});
        var newSize = ctor.Invoke(new object[] {(int) viewSizeType, width, height, text});

        //Remove any old versions of the custom view size
        if (SizeExists(sizeGroupType, text))
        {
            var index = FindSize(sizeGroupType, text, true);
            var removeCustomSize = getGroup.ReturnType.GetMethod("RemoveCustomSize");
            removeCustomSize.Invoke(group, new object[] {index});

        }
        //TODO: before adding, maybe check if it exists already, and then override it!


        addCustomSize.Invoke(group, new object[] {newSize});

        var groupType = group.GetType();
        var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
        var getCustomCount = groupType.GetMethod("GetCustomCount");
        int sizesCount = (int) getBuiltinCount.Invoke(group, null) + (int) getCustomCount.Invoke(group, null);

        //we return the last index, because that's where our new custom size ends up.
        return sizesCount;
    }

    public static void SetSize(int index)
    {
        var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var gvWnd = EditorWindow.GetWindow(gvWndType);
        selectedSizeIndexProp.SetValue(gvWnd, index, null);
    }


    public static bool SizeExists(GameViewSizeGroupType sizeGroupType, string text)
    {
        return FindSize(sizeGroupType, text) != -1;
    }

    public static int FindSize(GameViewSizeGroupType sizeGroupType, string text, bool onlyCustom = false)
    {
        // GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupType);
        // string[] texts = group.GetDisplayTexts();
        // for loop...

        var asm = typeof(Editor).Assembly;
        var sizesType = asm.GetType("UnityEditor.GameViewSizes");
        var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instanceProp = singleType.GetProperty("instance");
        var instance = instanceProp.GetValue(null, null);
        var getGroup = sizesType.GetMethod("GetGroup");
        var group = getGroup.Invoke(instance, new object[] {(int) sizeGroupType});
        var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
        var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
        
        var startValue = 0;
        if (onlyCustom)
        {
            var groupType = group.GetType();
            var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
            //var getCustomCount = groupType.GetMethod("GetCustomCount");
            int buildInCount = (int) getBuiltinCount.Invoke(group, null);
            startValue = buildInCount;
        }
        
        for (int i = startValue; i < displayTexts.Length; i++)
        {
            string display = displayTexts[i];
            // the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
            // so if we're querying a custom size text we substring to only get the name
            // You could see the outputs by just logging
            // Debug.Log(display);
            int pren = display.IndexOf('(');
            if (pren != -1)
                display = display.Substring(0,
                    pren - 1); // -1 to remove the space that's before the prens. This is very implementation-depdenent
            if (display == text)
                return i;
        }

        return -1;
    }

    public static bool SizeExists(GameViewSizeGroupType sizeGroupType, int width, int height)
    {
        return FindSize(sizeGroupType, width, height) != -1;
    }

    public static int FindSize(GameViewSizeGroupType sizeGroupType, int width, int height, bool onlyCustom = false)
    {
        // goal:
        // GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupType);
        // int sizesCount = group.GetBuiltinCount() + group.GetCustomCount();
        // iterate through the sizes via group.GetGameViewSize(int index)
        var asm = typeof(Editor).Assembly;
        var sizesType = asm.GetType("UnityEditor.GameViewSizes");
        var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instanceProp = singleType.GetProperty("instance");
        var instance = instanceProp.GetValue(null, null);
        var getGroup = sizesType.GetMethod("GetGroup");
        var group = getGroup.Invoke(instance, new object[] {(int) sizeGroupType});
        
        var groupType = group.GetType();
        var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
        var getCustomCount = groupType.GetMethod("GetCustomCount");
        int sizesCount = (int) getBuiltinCount.Invoke(group, null) + (int) getCustomCount.Invoke(group, null);
        var getGameViewSize = groupType.GetMethod("GetGameViewSize");
        var gvsType = getGameViewSize.ReturnType;
        var widthProp = gvsType.GetProperty("width");
        var heightProp = gvsType.GetProperty("height");
        var indexValue = new object[1];
        var startValue = 0;

        if (onlyCustom)
        {
            startValue = (int) getBuiltinCount.Invoke(group, null);
        }
        for (int i = startValue; i < sizesCount; i++)
        {
            indexValue[0] = i;
            var size = getGameViewSize.Invoke(group, indexValue);
            int sizeWidth = (int) widthProp.GetValue(size, null);
            int sizeHeight = (int) heightProp.GetValue(size, null);
            if (sizeWidth == width && sizeHeight == height)
                return i;
        }

        return -1;
    }

    public static void SetupCameraOrthSize(int height)
    {
        if (Camera.main == null) return;

        //Set Orthographic size to Rendering Resolution Height in Pixels / Pixels per Unit / 2
        Camera.main.orthographicSize = height / 24f / 2f;
    }

    [MenuItem("VGDL/SetupCamera 600")]
    public static void SetupCamera600()
    {
        if (Camera.main == null) return;

        //Set Orthographic size to Rendering Resolution Height in Pixels / Pixels per Unit / 2
        Camera.main.orthographicSize = 600 / 24f / 2f;
    }

    [MenuItem("VGDL/SetupCamera dynamic")]
    public static void SetupCameraDynamic()
    {
        if (Camera.main == null) return;

        //Set Orthographic size to Rendering Resolution Height in Pixels / Pixels per Unit / 2
        Camera.main.orthographicSize = Camera.main.pixelHeight / 24f / 2f;
    }
	
	#endif

    #endregion
}