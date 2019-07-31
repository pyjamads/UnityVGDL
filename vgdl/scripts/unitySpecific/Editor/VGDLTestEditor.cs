
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VGDLTest))]
public class VGDLTestEditor : Editor
{
    private int choice;
    
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        var vgdlTest = (VGDLTest) target;

        var examplesPath = Path.Combine("vgdl", "examples");

        switch (vgdlTest.exampleTypesToLoad)
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
                //Do Nothing
                break;
        }

        var examples = Resources.LoadAll<TextAsset>(examplesPath);
        var possibleGames = examples.Where(item => !item.name.Contains("lvl")).ToList();

        if (vgdlTest.type == VGDLTest.VGDLTestType.ParserTest || vgdlTest.type == VGDLTest.VGDLTestType.ParseAndRunTest)
        {
            //vgdlTest.exampleToLoad = "";
            vgdlTest.gamesToLoad = possibleGames;

            EditorUtility.SetDirty(target);
        }
        else
        {
            // Set the choice index to the previously selected index
            choice = possibleGames.FindIndex(item => item.name.Equals(vgdlTest.exampleToLoad));

            // If the choice is not in the array then the _choiceIndex will be -1 so set back to 0
            if (choice < 0)
            {
                choice = 0;
            }

            EditorGUILayout.HelpBox("Select a game from the list", MessageType.Info);
            choice = EditorGUILayout.Popup(choice, possibleGames.Select(item => item.name).ToArray());


            
            vgdlTest.exampleToLoad = possibleGames[choice].name;
            vgdlTest.gamesToLoad = new List<TextAsset>{possibleGames[choice]};

            // Save the changes back to the object
            EditorUtility.SetDirty(target);
        }
    }
}
