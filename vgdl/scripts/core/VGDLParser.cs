using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

public class VGDLParser
{
    public static bool verbose = true;

    /// <summary>
    /// Parser for the VGDL language.
    /// </summary>
    /// <param name="gameString"></param>
    /// <param name="loadLevelsFromGameTree"></param>
    /// <returns>BasicGame which contains all definitions from the loaded game string.</returns>
    public static BasicGame ParseGame(string gameString, bool loadLevelsFromGameTree = false)
    {
        if (verbose)
            Debug.Log("Lexing game tree");
        
        var gameTree = indentTreeLexer(gameString);

        if (verbose)
            Debug.Log("Parsing and Creating BasicGame instance");

        var typeAndArgs = parseArgs(gameTree.content);
        var game = CreateInstance<BasicGame>(typeAndArgs);
               
        game.init();
        
        foreach (var child in gameTree.children)
        {
            if (String.Compare(child.content, "SpriteSet", true) == 0)
            {
                if (verbose)
                    Debug.Log("Parsing and Creating VGDLSprite instances");
                parseSprites(game, child.children);
            }

            if (String.Compare(child.content, "InteractionSet", true) == 0)
            {
                if (verbose)
                    Debug.Log("Parsing and Creating VGDLInteraction instances");
                parseInteractions(game, child.children);
            }
            
            if (String.Compare(child.content, "LevelMapping", true) == 0)
            {
                if (verbose)
                    Debug.Log("Parsing and Creating VGDLMapping instances");
                
                parseLevelMappings(game, child.children);
            }
            
            if (String.Compare(child.content, "TerminationSet", true) == 0)
            {
                if (verbose)
                    Debug.Log("Parsing and Creating VGDLTermination instances");
                
                parseTerminations(game, child.children);
            }
            
            if (loadLevelsFromGameTree && string.Compare(child.content, "Levels", true) == 0)
            {
                if (verbose)
                    Debug.Log("Parsing levels from game tree");

                parseLevels(game, child.children);
            }
        }
        
        if(verbose)
            Debug.Log("Lexed Game Tree\n"+gameTree.reproduce());
        
        return game;
    }

    /// <summary>
    /// Parse sprite set
    /// </summary>
    /// <param name="game"></param>
    /// <param name="childChildren"></param>
    /// <param name="parentClass"></param>
    /// <param name="parentArgs"></param>
    /// <param name="parentKeys"></param>
    /// <exception cref="ArgumentException"></exception>
    private static void parseSprites(BasicGame game, IEnumerable<VGDLNode> childChildren, string parentClass = null, Dictionary<string, string> parentArgs = null, List<string> parentKeys = null)
    {   
        foreach (var node in childChildren)
        {
            if (!node.content.Contains(">"))
            {
                throw new ArgumentException("Sprite error on line: "+node.lineNumber+
                                            "\nSprite def should be in the form: spriteName > (SomeSpriteType paramX=5 etc.)");
            }

            if (verbose)
            {
                Debug.Log("Parsing sprite from: "+node.content+"\nLine: "+node.lineNumber);
            }
            
            List<string> stypes;
            if (parentKeys == null)
            {
                stypes = new List<string>();
            }
            else
            {
                stypes = new List<string>(parentKeys);
            }
            
            var spriteDef = node.content.Split('>');

            var key = spriteDef[0].Trim();
            var sdef = spriteDef[1].Trim();

            if (verbose && parentClass != null)
            {
                var parameters = string.Join(",", parentArgs.Select(item => item.ToString()).ToArray());
                Debug.Log(key+" parentClass "+parentClass+" and arguments ("+parameters+")");
            }
            
            var typeAndArgs = parseArgs(sdef, parentClass, parentArgs);
            
            stypes.Add(key);
            
            //Leaf types update subtypes of all parent types.
            game.updateSubTypes(stypes, key);
            
            //NOTE: ignoring singletons for now, only reason for it to exist
            //is to keep the designer from spawning multiple of them.
            //And the only function that sounds like it would use it in the JAVA framework,
            //doesn't actually check it (TransformToSingleton),
            //even the sprite defs in the only game that uses it don't set the singleton parameter.
            //eg. singleton parameter is overhead that no one uses.

            if (node.children.Any())
            {
                //Add the type unless it's an abstract type (eg. has no class definition)
                if (!string.IsNullOrEmpty(typeAndArgs.sclass))
                {
                    if (verbose)
                    {
                        var parameters = string.Join(",", typeAndArgs.args.Select(item => item.ToString()).ToArray()); 
                        Debug.LogFormat("Defining: {0} as type {1} with parameters ({2}) and parsing it's subtree!", key, typeAndArgs.sclass, parameters);
                    }
                
                    var spriteInfo = new VGDLSpriteInfo(key, typeAndArgs.sclass, typeAndArgs.args, stypes);
                    game.registerSpriteConstructor(key, spriteInfo);

                    game.addOrUpdateKeyInSpriteOrder(key);
                }


                parseSprites(game, node.children, typeAndArgs.sclass, typeAndArgs.args, stypes);
            }
            else //leaf node
            {
                if (verbose)
                {
                    var parameters = string.Join(",", typeAndArgs.args.Select(item => item.ToString()).ToArray()); 
                    Debug.LogFormat("Defining: {0} as type {1} with parameters ({2})", key, typeAndArgs.sclass, parameters);
                }

                var spriteInfo = new VGDLSpriteInfo(key, typeAndArgs.sclass, typeAndArgs.args, stypes);
                game.registerSpriteConstructor(key, spriteInfo);

                game.addOrUpdateKeyInSpriteOrder(key);
            }
        }
    }

    /// <summary>
    /// Parse interaction set
    /// </summary>
    /// <param name="game"></param>
    /// <param name="childChildren"></param>
    /// <exception cref="ArgumentException"></exception>
    private static void parseInteractions(BasicGame game, IEnumerable<VGDLNode> childChildren)
    {
        foreach (var node in childChildren)
        {
            if (!node.content.Contains(">"))
            {
                throw new ArgumentException("Interaction error on line: "+node.lineNumber+
                                            "\nInteraction should be in the form [sprite1 sprite2 etc. > SomeInteraction paramX=5 etc.]");
            }

            if (verbose)
            {
                Debug.Log("Parsing interaction: "+node.content+"\nLine: "+node.lineNumber);
            }
            
            var interaction = node.content.Split(new []{'>'}, StringSplitOptions.RemoveEmptyEntries);
            
            var effectTypeAndArgs = parseArgs(interaction[1]);

            var colliders = interaction[0].Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
            
            //Check for TIME effects
            if(interaction[0].ToUpper().Contains("TIME"))
            {
                if (verbose)
                {
                    var parameters = string.Join(",", effectTypeAndArgs.args.Select(item => item.ToString()).ToArray());
                    Debug.Log("Defining Time Effect > "+effectTypeAndArgs.sclass+" with parameters ("+parameters+")");
                }

                try
                {
                    var timeEffect = CreateInstance<VGDLTimeEffect>("VGDLTimeEffect", effectTypeAndArgs.args, true);
                    var innerEffect = CreateInstance<VGDLEffect>(effectTypeAndArgs, true);
                    timeEffect.effectDelegate = innerEffect;

                    if (colliders[0].CompareAndIgnoreCase("TIME"))
                    {
                        var i = 1;
                        while (colliders.Length > i && string.IsNullOrEmpty(colliders[i]))
                        {
                            //Nothing, just skipping empty spaces
                            i++;
                        }

                        if (colliders.Length > i)
                        {
                            timeEffect.targetType = colliders[i].Trim();
                        }
                    }
                    else
                    {
                        timeEffect.targetType = colliders[0].Trim();
                    }
                    
                    timeEffect.Validate(game);
                    game.addTimeEffect(timeEffect);

                    if (timeEffect.is_stochastic)
                    {
                        game.setStochastic(true);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("TimeEffect type [" + effectTypeAndArgs.sclass +
                                   "] not found, verify it is spelled correctly, the system is case-sensitive");
                    throw;
                }
                
                //If a TIME effect was defined it should not also be defined as a collision.
                continue;
            }

            if (colliders.Length < 2)
            {
                throw new ArgumentException("Not enough colliders for interaction error on line: "+node.lineNumber+
                                            "\nInteraction should be in the form [sprite1 sprite2 (sprite3...) > SomeEffect paramX=5 etc.]"+
                                            "\nAlternative effects are [TIME > SomeTimeEffect etc. ]" +
                                            "\nor [sprite1 TIME > SomeEffect paramX=5 etc.]"+
                                            "\nor [EOS sprite1 (sprite2...)] > SomeEffect etc."+
                                            "\nor [sprite1 EOS (sprite2...)] > SomeEffect etc.");
            }

            VGDLEffect effect;
            try
            {
                //Hack to accomodate addTimer
                if (effectTypeAndArgs.sclass.CompareAndIgnoreCase("addTimer"))
                {
                    var timeEffect = CreateInstance<VGDLTimeEffect>(effectTypeAndArgs, true);
                    timeEffect.Validate(game);
                    var innerEffect = CreateInstance<VGDLEffect>(effectTypeAndArgs.args["ftype"], effectTypeAndArgs.args, true);
                    timeEffect.effectDelegate = innerEffect;
                    timeEffect.isNative = false;
                    
                    //Target type can be set explicitly as a parameter to addTimer.
                    //timeEffect.targetType
                    
                    effect = timeEffect;
                    
                    if (verbose)
                        Debug.Log("Defining addTimer effect with delegated effect ["+effectTypeAndArgs.args["ftype"]+"]");
                }
                else
                {
                    effect = CreateInstance<VGDLEffect>(effectTypeAndArgs);
                    effect.Validate(game);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Effect type [" + effectTypeAndArgs.sclass +
                               "] not found, verify it is spelled correctly, the system is case-sensitive");
                throw;
            }

            if (verbose)
                Debug.Log("Adding collision ["+interaction[0]+"] effect ("+interaction[1]+")");
            
            var obj1 = colliders[0];

            for (var index = 1; index < colliders.Length; index++)
            {
                var obj2 = colliders[index];

                if (obj1.CompareAndIgnoreCase("EOS"))
                {
                    game.addEosEffect(obj2, effect);
                }
                else if(obj2.CompareAndIgnoreCase("EOS"))
                {
                    game.addEosEffect(obj1, effect);
                }
                else
                {
                    game.addCollisionEffect(obj1, obj2, effect);
                }

                if (verbose)
                {                    
                    var parameters = string.Join(",", effectTypeAndArgs.args.Select(item => item.ToString()).ToArray());
                    Debug.Log("Defining interaction "+obj1+" + "+obj2+" > "+effectTypeAndArgs.sclass+" with parameters ("+parameters+")");
                }
                
                if (effect.is_stochastic)
                {
                    game.setStochastic(true);
                }
            }
        }
    }

    /// <summary>
    /// Parse level mappings
    /// </summary>
    /// <param name="game"></param>
    /// <param name="childChildren"></param>
    /// <exception cref="ArgumentException"></exception>
    private static void parseLevelMappings(BasicGame game, List<VGDLNode> childChildren)
    {
        foreach (var node in childChildren)
        {
            if (!node.content.Contains(">"))
            {
                throw new ArgumentException("Mapping error on line: "+node.lineNumber+
                                            "\nMapping should be in the form [c > sprite1 sprite2 etc.]");
            }
            
            if (verbose)
            {
                Debug.Log("Parsing levelMapping from: "+node.content+"\nLine: "+node.lineNumber);
            }
            
            var map = node.content.Split(new []{'>'}, StringSplitOptions.RemoveEmptyEntries);

            var c = map[0].Trim();
            if (c.Length != 1)
            {
                throw new ArgumentException("Only single character mappings allowed on left side of '>'" +
                             "\nLine: "+node.lineNumber+" Found: ["+c+"]");
            }

            var keys = map[1].Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
            
            if (verbose)
                Debug.Log("Mapping ["+c+"] to ("+string.Join(",", keys)+")");

            game.addCharMapping(c[0], keys);  
        }
    }

    private static void parseLevels(BasicGame game, IEnumerable<VGDLNode> childChildren)
    {
        foreach (var node in childChildren)
        {
            if(verbose)
                Debug.Log("Parsing level: "+node.content+"\nLine: "+node.lineNumber);
            
            
            var level = new string[node.children.Count];
            var width = 0;
            for (var index = 0; index < node.children.Count; index++)
            {
                level[index] = node.children[index].content;
                if (width < level[index].Length)
                {
                    width = level[index].Length;
                }
            }
            
            if(verbose)
                Debug.Log("Adding level ["+node.content+"] with dimensions: ("+width+","+level.Length+")");
            
            game.addLevel(level);
        }
    }

    /// <summary>
    /// Parse termination set
    /// </summary>
    /// <param name="game"></param>
    /// <param name="childChildren"></param>
    private static void parseTerminations(BasicGame game, IEnumerable<VGDLNode> childChildren)
    {
        foreach (var node in childChildren)
        {
            if (verbose)
            {
                Debug.Log("Parsing termination from: "+node.content+"\nLine: "+node.lineNumber);
            }
            
            var typeAndArgs = parseArgs(node.content);

            if (verbose)
            {
                var parameters = string.Join(",", typeAndArgs.args.Select(item => item.ToString()).ToArray()); 
                Debug.Log("Parsing and Adding ["+typeAndArgs.sclass+"] " +
                          "with parameters ("+parameters+")");
            }


            try
            {
                var termination = CreateInstance<VGDLTermination>(typeAndArgs);
                termination.Validate(game);
                game.addTerminationCondition(termination);
            }
            catch (Exception e)
            {
                Debug.LogError("Termination type ["+typeAndArgs.sclass+"] not initialized, verify it is spelled correctly, the system is case-sensitive \n"+e.Message);
                throw e;
            }
        }
    }


    /// <summary>
    /// Parse line content from indented VGDL tree.
    /// </summary>
    /// <param name="contentString"></param>
    /// <returns></returns>
    private static VGDLParsedArgs parseArgs(string contentString, string sclass = null, Dictionary<string, string> args = null)
    {
        if (verbose)
        {
            Debug.Log("Parsing arguments from: "+contentString);    
        }
        
        var sparts = contentString.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
        var result = new VGDLParsedArgs();

        result.sclass = sclass;
        if (args == null)
        {
            result.args = new Dictionary<string, string>();
        }
        else
        {
            result.args = new Dictionary<string, string>(args);
        }

        if (string.IsNullOrEmpty(contentString) || sparts.Length == 0) return result;

        // Check if the first argument is a Class.
        if (!sparts[0].Contains("="))
        {
            var trimmed = sparts[0].Trim();
            eval(trimmed);

            result.sclass = trimmed;
        }

        for (var index = 0; index < sparts.Length; index++)
        {
            var part = sparts[index];

            //Ignore lone parameters
            if (!part.Contains("="))
            {
                //Unless we haven't found a sclass yet!
                if (!string.IsNullOrEmpty(part) && result.sclass == sclass)
                {
                    eval(part);
                    result.sclass = part;
                }
                continue;
            }

            var keyValue = part.Split(new []{'='}, StringSplitOptions.RemoveEmptyEntries);

            var val = keyValue[1];
            
            //handle array values
            if (val.Contains("["))
            {
                //add the next parts to the array, until we find "]"
                for (++index; index < sparts.Length; index++)
                {
                    part = sparts[index];
                    if (part.Contains("]"))
                    {
                        val += " " + part;    
                        break;
                    }

                    val += " " + part;
                }    
            }
            
            //This overrides any parent arg definitions, for repeat parameters,
            //like color on parent and on child, keeps child color.
            result.args[keyValue[0]] = val;
        }

        return result;
    }

    /// <summary>
    /// Parse indented VGDL tree
    /// </summary>
    /// <param name="gameString"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static VGDLNode indentTreeLexer(string gameString, int tabSize=4)
    {
        var tabSpace = new string(' ', tabSize);

        //TODO: all of these are ignored by the JAVA version, consider if we need them? 
        gameString = gameString.Replace("\t", tabSpace);
        gameString = gameString.Replace("(", " ");
        gameString = gameString.Replace(")", " ");
        
        //TODO: fix this crap, it seems they want to handle comma separated values, but forgot about this? (see hunger-games.txt)
        //NOTE: Figured out that the JAVA version actually doesn't change the line, because they ignore the result of the line.replace
        //gameString = gameString.Replace(",", " ");
        
        var lines = gameString.Split(new []{'\n'}, StringSplitOptions.RemoveEmptyEntries);

        VGDLNode last = null;
        
        for (var lineNumber = 0; lineNumber < lines.Length; lineNumber++)
        {
            var line = lines[lineNumber];
            //Remove comments starting with #
            if (line.Contains("#"))
            {
                line = line.Split('#')[0];
            }
            //Handle whitepace and indentation
            var content = line.Trim();
            if (content.Length > 0)
            {
                var indent = line.IndexOf(content[0]);
                last = new VGDLNode(content, indent, lineNumber, last);
            }
        }

        if (last == null)
        {
            throw new ArgumentException("Failed to parse:\n" + gameString);
        }
        
        //Return root
        return last.getRoot();
    }

    /// <summary>
    /// Instantiate type from typeAndArgs
    /// </summary>
    /// <param name="typeAndArgs"></param>
    /// <param name="suppressWarnings">Suppresses Warnings, used for TIME and addTimer effects</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static T CreateInstance<T>(VGDLParsedArgs typeAndArgs, bool suppressWarnings = false)
    {
        var type = Type.GetType(typeAndArgs.sclass);
        var instance = (T) Activator.CreateInstance(type);
        parseParameters(instance, typeAndArgs.args, suppressWarnings);
        
        return instance;
    }

    /// <summary>
    /// Instantiate type from sclass and argument list
    /// </summary>
    /// <param name="sclass"></param>
    /// <param name="args"></param>
    /// <param name="suppressWarnings">Suppresses Warnings, used for TIME and addTimer effects</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T CreateInstance<T>(string sclass, IEnumerable<KeyValuePair<string, string>> args, bool suppressWarnings = false)
    {
        var type = Type.GetType(sclass);
        var instance = (T) Activator.CreateInstance(type);
        parseParameters(instance, args, suppressWarnings);
        
        return instance;
    }

    /// <summary>
    /// Check whether the type exists!
    /// </summary>
    /// <param name="str"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void eval(string str)
    {
        var type = Type.GetType(str, false, true);
    
        if (type == null)
        {
            throw new ArgumentException("Could not find Type ["+str+"]");
        }
    }

    /// <summary>
    /// Parse arguments from argument list and set their values on the object given.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="args"></param>
    /// <exception cref="ArgumentException"></exception>
    private static void parseParameters(object obj, IEnumerable<KeyValuePair<string, string>> args, bool suppressWarnings = false)
    {
        var type = obj.GetType();
        var fields = type.GetFields();

        var fieldMap = new Dictionary<string, FieldInfo>();

        foreach (var field in fields)
        {
            if (fieldMap.ContainsKey(field.Name))
            {
                Debug.LogError("Key ["+field.Name+"] already in fieldMap for: "+type);
            }
            fieldMap.Add(field.Name, field);
        }

        foreach (var keyValuePair in args)
        {
            if (fieldMap.ContainsKey(keyValuePair.Key))
            {
                var fieldInfo = fieldMap[keyValuePair.Key];
                
                if (keyValuePair.Value.Contains("["))
                {
                    //value seems to be array, validate that.
                    if (keyValuePair.Value.Contains("]"))
                    {
                        var trimmed = keyValuePair.Value.Trim('[', ']');
                        
                        //find list of value strings
                        var values = trimmed.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);

                        //Reflect the ParseToArray method
                        var methodDefinition = typeof(VGDLParser).GetMethod("ParseToArray");
                        //Generate the generic method, based on the field type
                        // ReSharper disable once PossibleNullReferenceException
                        var parseMethod = methodDefinition.MakeGenericMethod(fieldInfo.FieldType.GetElementType());
                        //Invoke the parse method with our values.
                        var array = parseMethod.Invoke(null, new object[] {values});
                        
                        fieldInfo.SetValue(obj, array);
                        
                        //this keyvalue pair has been handled, continue to the next one.
                        continue;
                    }
                    else
                    {
                        throw new ArgumentException("Parsing parameter ["+keyValuePair.Key+"] failed, array missing ']'");
                    } 
                }

                
                if (fieldInfo.FieldType.IsAssignableFrom(typeof(Color)))
                {
                    //Special case for color parsing!
                    Color color;
                    var success = VGDLColors.ParseColor(keyValuePair.Value, out color);
                    if (success)
                    {
                        fieldInfo.SetValue(obj, color);
                    }
                    else
                    {
                        Debug.LogWarning("Color ["+keyValuePair.Value+"] not found, using magenta instead");
                        fieldInfo.SetValue(obj, Color.magenta);
                    }
                }
                else if (fieldInfo.FieldType.IsAssignableFrom(typeof(Vector2)))
                {
                    //Special case for orientation parsing!
                    var direction = VGDLUtils.ParseVGDLDirection(keyValuePair.Value);
                    if (direction != VGDLUtils.VGDLDirections.NIL.getDirection())
                    {
                        fieldInfo.SetValue(obj, direction);
                    }
                    else
                    {
                        Debug.LogWarning("Direction ["+keyValuePair.Value+"] not found, using 'LEFT' instead");
                        fieldInfo.SetValue(obj, Vector2.left);
                    }
                }
                else
                {
                    try
                    {
                        fieldInfo.SetValue(obj,Convert.ChangeType(keyValuePair.Value, fieldInfo.FieldType));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Unable to parse ["+keyValuePair.Value+"] for field ["+keyValuePair.Key+" with type: "+fieldInfo.FieldType.Name+"] on "+type.Name);
                        throw;
                    }
                        
                }
            }
            else
            {
                //We suppress warnings for TimeEffects, because of the way they are defined
                //eg. [sprite1 TIME > transformToAll stype=portal stypeTo=goalPortal nextExecution=500 timer=500 repeating=False]
                //This means that these parameters don't all belong to the timer, some belong to the desired effect.
                if (!suppressWarnings)
                {
                    Debug.LogWarning("Field ["+keyValuePair.Key+"] does not exist on "+type.Name);
                }
                else
                {
                    if (verbose)
                    {
                        Debug.Log("Parameter ["+keyValuePair.Key+"] ignored on "+type.Name);
                    }
                }
            }
        }
    }

    /// <summary>
    /// A generic method for converting value strings to an array of values. 
    /// </summary>
    /// <param name="values"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>An array of parsed values, with the type T</returns>
    [UsedImplicitly] //Used through Reflection
    public static T[] ParseToArray<T>(string[] values)
    {
        return values.Select(s => (T) Convert.ChangeType(s, typeof(T))).ToArray();
    }
    
    /// <summary>
    /// private struct for holding class name and arguments 
    /// </summary>
    private struct VGDLParsedArgs
    {
        public string sclass;
        public Dictionary<string, string> args;
    }
}