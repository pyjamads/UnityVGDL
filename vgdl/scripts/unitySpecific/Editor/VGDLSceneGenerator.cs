using UnityEngine;
using UnityEditor;

public class VGDLSceneGenerator : EditorWindow
{
    #region Menu and Static reference

    private static EditorWindow _window;
    
    [MenuItem ("VGDL/Scene Generator")]
    //[Tooltip("Generate a Unity scene from a VGDL game and level description.")]
    public static void  ShowWindow () {
        _window = EditorWindow.GetWindow(typeof(VGDLSceneGenerator));
    }
    
    #endregion
    
    VGDLGameAndLevel vgdlToGenerate = new VGDLGameAndLevel();
    
    bool UseVGDLBackend = true;
    bool UseUnityPlayground = false;
    
    void OnGUI()
    {
        _window.titleContent = new GUIContent("VGDL Generator");
        
        //NOTE: Before we can create VGDLScene we need to make these first:
        //TODO: Create special MonoBehavior, that can be initialized as any stype from spriteConstructor with tag=stype, and handles the interactions described in VGDL.
        //TODO: Create Termination MonoBehavior that handles terminations

        vgdlToGenerate.type = (VGDLExampleTypes)EditorGUILayout.EnumPopup("Type", vgdlToGenerate.type);
        vgdlToGenerate.filename = EditorGUILayout.TextField ("Filename", vgdlToGenerate.filename);
        vgdlToGenerate.level = EditorGUILayout.IntField("Level", vgdlToGenerate.level);
        
        GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
        if (UseVGDLBackend)
        {
            EditorGUILayout.HelpBox("Scene hierarchy will be created with the VGDLGame backend controlling them.", MessageType.Info, true);
        }
        else if (!UseUnityPlayground)
        {
            EditorGUILayout.HelpBox("Scene hierarchy will be created with custom MonoBehaviors controlling them.", MessageType.Info, true);
        }
        var wasToggledOn = EditorGUILayout.Toggle ("Use VGDL Backend", UseVGDLBackend);
        if (UseUnityPlayground && wasToggledOn)
        {
            UseVGDLBackend = true;
            UseUnityPlayground = false;
        }
        if (UseUnityPlayground)
        {
            EditorGUILayout.HelpBox("Scene hierarchy will be created with the UnityPlayground framework.", MessageType.Info, true);
        }
        wasToggledOn = EditorGUILayout.Toggle ("Use Unity Playground", UseUnityPlayground);
        if (UseVGDLBackend && wasToggledOn)
        {
            UseUnityPlayground = true;
            UseVGDLBackend = false;
        }
        
        if (GUILayout.Button("Generate Unity GameObjects"))
        {
            var game = VGDLRunner.LoadGame(vgdlToGenerate);
            
            VGDLParser.verbose = false;
            if (UseUnityPlayground)
            {
                //TODO: try implementing this for the continuous physics games.
            }
            else
            {
                //TODO: Load up VGDL Game, and initialize level
                //TODO: Get list of sprite constructors and stypes 
                //TODO: Generate scene from initial game state in game.
                
                if (UseVGDLBackend)
                {
                    //TODO: use common VGDLSpriteBehavior that just queries VGDLGame for updates/collisions.
                }
                else
                {
                    //TODO: create custom VGDLBehaviors that implement collision detection, physics and a VGDLGameManager to manage game state/termination
                }
            }
            
            //NOTE: instead of referencing VGDLUnityView, make a VGDLUnityLoader            
            //NOTE: make a function that does what VGDLTest does now and call that.
            //NOTE: make another function that uses unityView.game to generate everything in the scene. 
        }
    }
}