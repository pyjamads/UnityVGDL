using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[Serializable]
public class VGDLGame
{
    /// <summary>
    /// Contains mappings between characters (symbols) in the Levels
    /// and the sprites that spawn there. 
    /// </summary>
    public Dictionary<char, string[]> charMapping 
        = new Dictionary<char, string[]>();

    /// <summary>
    /// The z-order of the sprite types, so we draw sprites first to last,
    /// with last being topmost.
    /// </summary>
    public List<string> spriteOrder 
        = new List<string>();

    /// <summary>
    /// Contains information on how to construct
    /// the sprites defined in the game description.
    /// </summary>
    public Dictionary<string, VGDLSpriteInfo> spriteContructors 
        = new Dictionary<string, VGDLSpriteInfo>();

    /// <summary>
    /// Contains lists of all sprites, grouped by type.
    /// </summary>
    public Dictionary<string, List<VGDLSprite>> spriteGroups 
        = new Dictionary<string, List<VGDLSprite>>();

    /// <summary>
    /// A list of all subtypes a type belongs to
    /// </summary>
    public Dictionary<string, List<string>> subTypes 
        = new Dictionary<string, List<string>>();

    /// <summary>
    /// Contains collision effects, but multiple effects can be triggered by two sprites colliding.
    /// </summary>
    public Dictionary<KeyValuePair<string, string>, List<VGDLEffect>> collisionEffects 
        = new Dictionary<KeyValuePair<string, string>, List<VGDLEffect>>();

    /// <summary>
    /// Contains all Edge Of Screen (EOS) effects
    /// </summary>
    public Dictionary<string, List<VGDLEffect>> eosEffects 
        = new Dictionary<string, List<VGDLEffect>>();

    /// <summary>
    /// Contains all time effects
    /// </summary>
    public List<VGDLTimeEffect> timeEffects 
        = new List<VGDLTimeEffect>();

    /// <summary>
    /// Contains all shield effects
    /// NOTE: shieldedEffects is a Dictionary of stype1 and list of key value pairs of stype2 and effect hashcodes
    /// </summary>
    public Dictionary<string, List<KeyValuePair<string,int>>> shieldedEffects 
        = new Dictionary<string, List<KeyValuePair<string,int>>>();

//    protected List<Bucket> bucketList 
//        = new List<Bucket>();

    /// <summary>
    /// Contains all termination criteria
    /// </summary>
    public List<VGDLTermination> terminations 
        = new List<VGDLTermination>();

    /// <summary>
    /// Resource properties, by their identifier
    /// </summary>
    protected Dictionary<string, Color> resourceColors 
        = new Dictionary<string, Color>();

    public Dictionary<string, int> resourceLimits 
        = new Dictionary<string, int>();

    /**
     * Historic of events related to the avatar happened during the game. The
     * entries are ordered asc. by game step.
     */
    public List<RecordedEvent> historicEvents = new List<RecordedEvent>();
    
    
    /// <summary>
    /// List of sprites that died this game tick,
    /// that will be removed at the beginning of the next frame. 
    /// </summary>
    public List<VGDLSprite> killList 
        = new List<VGDLSprite>();

    public List<string[]> levelList 
        = new List<string[]>();

    /**
     * Quick reference to the player avatars
     */
    public VGDLAvatar[] avatars;

    /**
	 * Screen size.
	 */
    public Vector2 screenSize;

    /**
     * Dimensions of the game.
     */
    public Vector2 size;

    /**
     * Indicates if the game is stochastic.
     */
    protected bool is_stochastic;

    /**
     * Number of sprites this game has.
     */
    protected int num_sprites;

    /**
     * Game tick
     */
    public int gameTick;

    /**
     * Size of the block in pixels.
     */
    public int block_size = 10;

    /**
     * Indicates if the game is ended.
     */
    public bool isEnded;

    /**
     * State observation for this game.
     */
    public ForwardModel fwdModel;

    /**
     * Maximum number of sprites in a game.
     */
    public static int MAX_SPRITES;

    public const int SCORE_DISQ = -1000;

    /**
     * Random number generator for this game. It can only be received when the
     * game is started.
     */
    private Random.State random;

    /**
     * Id of the sprite type "avatar".
     */
    private int avatarId;

    /**
     * Id of the sprite type "wall".
     */
    private int wallId;

    /**
     * Flag that can only be set to true externally. If true, the agent is
     * disqualified.
     */
    private bool disqualified;

    /**
     * Next ID to generate for sprites;
     */
    public int nextSpriteID;

    /**
     * Key Handler for human play. The default is
     * CompetitionParameters.KEY_INPUT
     */
    public string key_handler;

    /**
     * indicates if player i is human or not
     */
    public bool[] humanPlayer;
    
    /**
     * Pathfinder.
     */
    public PathFinder pathf;


    public int no_players = 1; // default to single player

    public int no_counters = 0; // default no counters
    protected internal int[] counters;
    
    /// <summary>
    /// Input handler interface
    /// </summary>
    public VGDLInputHandler inputHandler;
    
    public float time;
    
    public VGDLGameResult gameResult;
    
    public string name;
    
    
    /**
     * Avatars last actions. Array for all avatars in the game. Index in array
     * corresponds to playerID.
     */
    protected internal VGDLAvatarActions[] avatarLastAction;
    
    public VGDLGame()
    {
        //default sprite constructors need to be added for wall and avatar.
        var wallArgs = new Dictionary<string, string>();
        wallArgs.Add("color", "DARKGRAY");
        wallArgs.Add("solid", "True");
        var wallInfo = new VGDLSpriteInfo("wall", "Immovable", wallArgs, new List<string>{"wall"});
		
        var avatarArgs = new Dictionary<string, string>();
        var avatarInfo = new VGDLSpriteInfo("avatar", "MovingAvatar", avatarArgs, new List<string>{"avatar"});
		
        registerSpriteConstructor("wall", wallInfo);
        registerSpriteConstructor("avatar", avatarInfo);
        
        updateSubTypes(new List<string>{"wall"}, "wall");
        updateSubTypes(new List<string>{"avatar"}, "avatar");
    }

    

    public void init()
    {
        avatars = new VGDLAvatar[no_players];
        counters = new int[no_counters];

        avatarLastAction = new VGDLAvatarActions[no_players];
        for (int i = 0; i < no_players; i++)
            avatarLastAction[i] = VGDLAvatarActions.ACTION_NIL;

        counters = new int[no_counters];
        humanPlayer = new bool[no_players];
        
        // taking care of the key handler parameter:
        //if (key_handler != null && key_handler.CompareAndIgnoreCase("Pulse"))
        //CompetitionParameters.KEY_HANDLER = CompetitionParameters.KEY_PULSE;

        inputHandler = !key_handler.CompareAndIgnoreCase("Pulse") ? new VGDLKeyHandler()
            : new VGDLPulseHandler();
    }

    /**
	 * Sets the game back to the state prior to load a level.
	 */
    public void reset() {
        num_sprites = 0;

        for (int i = 0; i < no_players; i++)
        {
            if(avatars == null) break;
            avatars[i].player = null;
            avatars[i].setKeyHandler(null);
            avatars[i] = null;
        }
        for (int i = 0; i < no_counters; i++) {
            if(counters == null) break;
            counters[i] = 0;
        }
        isEnded = false;
        gameTick = -1;
        disqualified = false;
        avatarLastAction = new VGDLAvatarActions[no_players];
        for (int i = 0; i < no_players; i++)
            avatarLastAction[i] = VGDLAvatarActions.ACTION_NIL;

        // For each sprite type...
        foreach (var spriteGroup in spriteGroups)
        {
            foreach (var sprite in spriteGroup.Value)
            {
                if (sprite.images != null)
                {
                    foreach (var spriteImage in sprite.images)
                    {
                        spriteImage.Value.Clear();
                    }
                    sprite.images.Clear();
                }

                //NOTE: This is Unity specific to remove the texture from memory
                sprite.image = null;
            }
            spriteGroup.Value.Clear();
        }
       
        if (killList != null) {
            killList.Clear();
        }
//        for (int j = 0; j < bucketList.Count; ++j) {
//            bucketList[j].clear();
//        }

        historicEvents.Clear();
        
        //NOTE: these should maybe be cleared here as well?
        //resourceLimits.Clear();
        //resourceColors.Clear();

        resetShieldEffects();
    }
    
    /**
	 * Starts the forward model for the game.
	 */
    public void initForwardModel() {
        fwdModel = new ForwardModel(this, 0);
        fwdModel.update(this);
    }
    
    /**
	 * Initializes some variables for the game to be played, such as the game
	 * tick, sampleRandom number generator, forward model and assigns the player
	 * to the avatar.
	 *
	 * @param players
	 *            Players that play this game.
	 * @param randomSeed
	 *            sampleRandom seed for the whole game.
	 */
    public void prepareGame(VGDLPlayerInterface[] players, int randomSeed = -1) {
        // Start tick counter.
        gameTick = -1;

        if (randomSeed != -1)
        {
            // Create the sampleRandom generator.
            Random.InitState(randomSeed);
            random = Random.state;    
        }

        // Assigns the player to the avatar of the game.
        createAvatars();
        assignPlayer(players);

        // Initialize state observation (sets all non-volatile references).
        initForwardModel();
    }

    public void createAvatars() {

        // Avatars will usually be the first elements, starting from the end.

        // Find avatar sprites
        var avSprites = new List<VGDLAvatar>();
        var idx = spriteOrder.Count;
        var numAvatarSprites = 0;
        while (idx > 0) {
            idx--;
			
            var spriteTypeId = spriteOrder[idx];
            var num = spriteGroups[spriteTypeId].Count;
            if (num > 0) {
                // There should be just one sprite in the avatar's group in
                // single player games.
                // Could be more than one avatar in multiplayer games
                for (var j = 0; j < num; j++) {
                    var thisSprite = spriteGroups[spriteTypeId][j];
                    if (thisSprite.is_avatar) {
                        avSprites.Add(thisSprite as VGDLAvatar);
                    }
                }
            }
        }
		
        numAvatarSprites = avSprites.Count;
        if (VGDLParser.verbose)
        {
            Debug.Log("Done finding avatars: " + numAvatarSprites);
        }

        avSprites.Reverse(); // read in reverse order 
        if (avSprites.Any()) {
            for (int i = 0; i < no_players; i++) {
                if (numAvatarSprites > i) { // check if there's enough avatars just in case
                    avatars[i] = avSprites[i];
                    avatars[i].setKeyHandler(inputHandler);
                    avatars[i].playerID = i;
                }
            }
        } else {
            Debug.LogWarning("No avatars found!");
        }
    }
    
    /* Looks for the avatar of the game in the existing sprites. If the player
    * received as a parameter is not null, it is assigned to it.
    *
    * @param players
    *            the players that will play the game (only 1 in single player
    *            games).
    */
    private void assignPlayer(VGDLPlayerInterface[] players) {
        // iterate through all avatars and assign their players
        if (players.Length == no_players) {
            for (int i = 0; i < no_players; i++) {
                if (players[i] != null && avatars[i] != null) {
                    avatars[i].player = players[i];
                    avatars[i].player.PlayerID = i;
                } else {
                    Debug.Log("Null player.");
                }
            }
        } else {
            Debug.Log("Incorrect number of players.");
        }
    }

    protected VGDLSprite createAndAddSpriteAt(string stype, Vector2 position, bool force = false)
    {
        if (num_sprites > MAX_SPRITES)
        {
            Debug.LogWarning("Sprite limit reached.");
            return null;
        }

        if (!spriteContructors.ContainsKey(stype))
        {
            Debug.LogWarning("Sprite definition for ["+stype+"] not found!");
            return null;
        }
		
        if(VGDLParser.verbose)
            Debug.Log("Creating sprite ["+stype+"] at position ("+position.x+","+position.y+")");

        if (string.IsNullOrEmpty(spriteContructors[stype].sclass))
        {
            Debug.LogError("Can't initialize abstract sprite types: "+stype+" has no Sprite class");
        }
		
        // Check for singleton Sprites
        if (!force) {
            foreach (var type in spriteContructors[stype].stypes)
            {
                // If this type is a singleton and we have one already
                if (spriteContructors.ContainsKey(type)
                    && spriteContructors[type].args.ContainsKey("singleton") 
                    && spriteContructors[type].args["singleton"].CompareAndIgnoreCase("TRUE") 
                    && getNumberOfSprites(type) > 0)
                {
                    // that's it, no more creations of this type.
                    if (VGDLParser.verbose)
                    {
                        Debug.Log("Sprite limit reached for "+stype);
                    }
				
                    return null;
                }    
            }
        }

        var spriteSize = new Vector2(block_size, block_size);
		
        //TODO: consider templates.
		
        //Create sprite
        var newSprite = VGDLParser.CreateInstance<VGDLSprite>(spriteContructors[stype].sclass, spriteContructors[stype].args);
        //set position and size
        newSprite.types = spriteContructors[stype].stypes;
        newSprite.init(position, spriteSize);

        addSprite(stype, newSprite);

        newSprite.name = stype + "_" + newSprite.spriteID;

        //If new sprite is a resource, update the resource dictionaries.
        var res = newSprite as Resource;
        if (res != null)
        {
            resourceLimits[res.resource_name] = res.limit;
            resourceColors[res.resource_name] = res.color;
        }
        
        return newSprite;
    }

    public List<string> getSpriteOrder()
    {
        return spriteOrder;
    }
    

    public int getNumberOfSprites(string stype, bool includeDisabled = true)
    {
        if (!subTypes.ContainsKey(stype))
        {
            Debug.LogWarning("Could not find sprite type: "+stype);
            return 0;
        }
		
        var count = 0;
        foreach (var subtype in subTypes[stype])
        {
            if(!spriteGroups.ContainsKey(subtype)) continue;

            if (includeDisabled)
            {
                count += spriteGroups[subtype].Count;	
            }
            else
            {
                count += spriteGroups[subtype].Count(item => !item.is_disabled());
            }
        }
        return count;
    }

    public List<string> getSubTypes(string stype)
    {
        if (!subTypes.ContainsKey(stype))
        {
            Debug.LogWarning("Could not find sprite type: "+stype);
            return new List<string>();
        }

        return subTypes[stype];
    }

    public void addTerminationCondition(VGDLTermination termination)
    {
        terminations.Add(termination);
    }

    public void addCharMapping(char c, string[] keys)
    {
        charMapping[c] = keys;
    }

    public void addEosEffect(string stype, VGDLEffect effect)
    {
        if (eosEffects.ContainsKey(stype))
        {
            eosEffects[stype].Add(effect);	
        }
        else
        {
            eosEffects[stype] = new List<VGDLEffect>{effect};
        }
    }

    public void addCollisionEffect(string stype1, string stype2, VGDLEffect effect)
    {
        var pair = new KeyValuePair<string, string>(stype1, stype2);
		
        if (collisionEffects.ContainsKey(pair))
        {
            collisionEffects[pair].Add(effect);
        }
        else
        {
            collisionEffects[pair] = new List<VGDLEffect>{effect};
        }
    }

    public void addTimeEffect(VGDLTimeEffect timeEffect)
    {
        timeEffects.Add(timeEffect);
    }

    public void setStochastic(bool stochastic)
    {
        is_stochastic = stochastic;
    }

    public VGDLSprite addSprite(string stype, Vector2 position, bool force = false)
    {
        return createAndAddSpriteAt(stype, position, force);
    }

    public void addSprite(string key, VGDLSprite sprite)
    {
        sprite.spriteID = nextSpriteID++;
        sprite.name = key;
		
        if (spriteGroups.ContainsKey(key))
        {
            spriteGroups[key].Add(sprite);
        }
        else
        {
            spriteGroups[key] = new List<VGDLSprite>{sprite};
        }
			
        num_sprites++;

        if (sprite.is_stochastic)
        {
            is_stochastic = true;
        }
    }

    public void addOrUpdateKeyInSpriteOrder(string key)
    {
        //update the order, by appending the key to the end, and removing the old position.
        spriteOrder.Remove(key);
        spriteOrder.Add(key);
    }

    public VGDLSpriteInfo getRegisteredSpriteConstructor(string stype, bool checkSubTypes = false)
    {
        if (spriteContructors.ContainsKey(stype))
        {
            return spriteContructors[stype];
        }

        //This is for validation purposes, if the stype is an abstract class,
        //then we need to figure out if a subtype is defined. 
        if (checkSubTypes)
        {
            var stypes = getSubTypes(stype);
            foreach (var subType in stypes)
            {
                //Skip stype, because this is a recursive call.
                if (subType.Equals(stype)) continue;
                
                //Don't check subtypes of subtypes, that creates an infinite loop :D
                var spriteInfo = getRegisteredSpriteConstructor(subType, false);
                if (spriteInfo != null)
                {
                    return spriteInfo;
                }
            }
        }

        if (VGDLParser.verbose)
        {
            Debug.LogWarning("No sprite was registered with type: "+stype);	
        }
		
        return null;
    }

    public void registerSpriteConstructor(string key, VGDLSpriteInfo spriteInfo)
    {
        //NOTE: new sprite constructors with the same key will override previous definitions
        if (spriteContructors.ContainsKey(key))
        {
            //Ignore wall and avatar overrides, otherwise Log a warning
            if (!key.CompareAndIgnoreCase("wall") && !key.CompareAndIgnoreCase("avatar"))
            {
                Debug.LogWarning("Sprite ["+key+"] is defined multiple times, the last one will be used.");
            }
        }
		
        spriteContructors[key] = spriteInfo;
        //subTypes[key] = spriteInfo.stypes;
		
        //Also add the sprite group
        if (!spriteGroups.ContainsKey(key))
        {
            spriteGroups[key] = new List<VGDLSprite>();
        }
    }

    public void updateSubTypes(List<string> stypes, string key)
    {
        //Add all parent types to sub type list
        if (!subTypes.ContainsKey(key))
        {
            subTypes[key] = stypes.ToList(); //ToList, copies elements to a new list
        }
        
        //Update all parent types with this additional type
        foreach (var stype in stypes)
        {
            if (subTypes.ContainsKey(stype) && !subTypes[stype].Contains(key))
            {
                subTypes[stype].Add(key);
            }
        }
    }

    public List<VGDLSprite> getSprites(string stype)
    {
        if (!spriteGroups.ContainsKey(stype))
        {
            if (VGDLParser.verbose)
            {
                Debug.LogWarning("Could not find type: "+stype);
            }
            return new List<VGDLSprite>();
        }
        return spriteGroups[stype];
    }
    
    /**
	 * Gets a list for the collection of sprites for a particular sprite
	 * type, AND all subtypes.
	 *
	 * @param spriteItype type of the sprite to retrieve.
	 * @return sprite collection of the specified type and subtypes.
	 */
    public List<VGDLSprite> getAllSubSprites(string stype)
    {
        // Create a sprite group for all the sprites
        var allSprites = new List<VGDLSprite>();
        // Get all the subtypes
        var allTypes = getSubTypes(stype);

        // Add sprites of this type, and all subtypes / parent types.
        foreach (var subType in allTypes) {
            allSprites.AddRange(this.getSprites(subType));
        }

        // Return the list.
        return allSprites;
    }

    public void killSprite(VGDLSprite sprite, bool transformed)
    {
        if (sprite is VGDLAvatar && !transformed)
        {
            //disable
            sprite.setDisabled(true);
        }
        else
        {
            killList.Add(sprite);
        }
    }

    public void addShield(string type1, string type2, int effectHashcode)
    {
        //NOTE: shieldedEffects might as well be Dictionary<KeyValuePair, List<hash>>
        //NOTE: might this actually cause issues, because the comparison fails?
        var kvp = new KeyValuePair<string, string>(type1, type2);
        if (shieldedEffects.ContainsKey(type1))
        {
            shieldedEffects[type1].Add(new KeyValuePair<string, int>(type2, effectHashcode));
        }
        else
        {
            shieldedEffects.Add(type1,
                new List<KeyValuePair<string, int>>
                    { new KeyValuePair<string, int>(type2, effectHashcode) });	
        }
		
    }

    public void addLevel(string[] level)
    {
        levelList.Add(level);
    }
    
    /**
	 * Disqualifies the player in the game, and also sets the isEnded flag to
	 * true.
	 */
    // comment this method out to check if mistakes in method overloading
    // anywhere
    public void disqualify() {
        disqualified = true;
        isEnded = true;
    }

    /**
     * Overloaded method for multiplayer games. Same functionality as above.
     *
     * @param id
     *            - id of the player that was disqualified
     */
    public void disqualify(int id) {
        if (gameResult == null) gameResult = new VGDLGameResult();
        
        isEnded = true;
        
        if (id == -1)
        {
            disqualified = true;
            avatars[0].disqualify(true);
            gameResult.playerAvatars.Add(avatars[0]);
            gameResult.playerOutcomes.Add(avatars[0].winState);
            gameResult.playerScores.Add(avatars[0].score);
            
            avatars[0].passResult(this);
        }
        else
        {
            avatars[id].disqualify(true);

            for (int i = 0; i < no_players; i++)
            {
                gameResult.playerAvatars.Add(avatars[i]);
                gameResult.playerOutcomes.Add(avatars[i].winState);
                gameResult.playerScores.Add(avatars[i].score);    
            }

            for (int i = 0; i < no_players; i++)
            {
                avatars[id].passResult(this);
            }
        }
    }

    public int getGameTick()
    {
        return gameTick;
    }

    public void clearAll(ForwardModel fm)
    {
        foreach (var sprite in killList)
        {
            var removed = spriteGroups[sprite.getType()].Remove(sprite);
            if (fm != null)
            {
                fm.removeSpriteObservation(sprite);
            }

            if (sprite.is_avatar)
            {	
                //NOTE: this works under the assumption each player is only controlling one avatar.
                // go through all avatars to see which avatar is dead
				for (int i = 0; i < no_players; i++)
				{
					if (sprite == avatars[i])
					{
						avatars[i] = null;
					}
				}
            }

            if (removed)
            {
                num_sprites--;
            }
            else
            {
                if (VGDLParser.verbose)
                {
                    Debug.Log("Failed to remove sprite: "+sprite.spriteID + " "+sprite.name+" of type "+sprite.getType());
                }
            }
        }
		
        killList.Clear();
		
		
//        for (int j = 0; j < bucketList.Count; ++j) {
//            bucketList[j].clear();
//        }

        resetShieldEffects();
    }

    private void resetShieldEffects() {
        shieldedEffects.Clear();
    }

    public void updateGameState()
    {
        gameTick++; // next game tick.
		
        //NOTE: only update our forward model if it's needed, eg. for Learning it is not needed, so we don't initialize it.
        // Update our state observation (forward model) with the information of
        // the current game state.
        if (fwdModel != null)
        {
            fwdModel.update(this);
        }
        // System.out.println(avatars[0].rect);
		
        //Update all entities in the game based on the Physics models, time, etc.
        updateEntities();

        //NOTE: buckets are never used in JavaVGDL, they are initialized, but the getters are unused.
        //Update bucket lists for collision handling
//        if (bucketList.Count != spriteGroups.Count)
//        {
//            while (bucketList.Count < spriteGroups.Count)
//            {
//                bucketList.Add(new Bucket());
//            }
//
//            while (bucketList.Count > spriteGroups.Count)
//            {
//                bucketList.RemoveAt(0);
//            }
//        }

        //Run all collision/event handling code
        doEventHandling();

        // clear all additional data, including dead sprites
        clearAll(fwdModel);
		
        //Check all terminations
        checkTerminationConditions();

        // Check for end of game by time steps.
        //NOTE: used for competitions
        //checkTimeOut(); 

//        if ((gameTick == 0 || isEnded) && fwdModel != null)
//        {
//            fwdModel.printObservationGrid(); //uncomment this to show the observation grid.   
//        }
    }

    protected virtual void doEventHandling()
    {
        // First, check the effects that are triggered in a timely manner.
        while (timeEffects.Count > 0 && timeEffects.First().nextExecution <= gameTick)
        {
            //Pop the first element of the list
            var ef = timeEffects.First();
            timeEffects.RemoveAt(0);
			
            if (!ef.enabled) continue;
			
            var targetType = ef.targetType;
            var exec = false;

            // if intId==-1, we have no sprite
            if (string.IsNullOrEmpty(targetType)) {
                // With no sprite, the effect is independent from particular
                // sprites.
                ef.execute(null, null, this);
                exec = true;

                // Affect score for all players:
                if (ef.applyScore) {
                    for (var i = 0; i < no_players; i++) {
                        avatars[i].addScore(ef.getScoreChange(i));
                    }
                }

            } else {

                var allTypes = subTypes[targetType];
                foreach (var stype in allTypes) {
                    // Find all sprites of this subtype.
                    var sprites = getSprites(stype);
                    foreach (var sp in sprites) {
                        // Check that they are not dead (could happen in
                        // this same cycle).
                        if (! killList.Contains(sp) && !sp.is_disabled()) {
                            executeEffect(ef, sp, null);
                            exec = true;
                        }
                    }
                }
            }

            // If the time effect is repetitive, need to reinsert in the
            // list of effects
            if (!ef.repeating) continue;

            if (!exec)
            {
                ef.planExecution(this);
            }
            addTimeEffect(ef);
        }

        // Secondly, we handle single sprite events (eEOS). Take each sprite
        // stype that has an EOS effect defined.
        foreach (var key in eosEffects.Keys) {
            // For each effect that this sprite has assigned.
            foreach (VGDLEffect ef in eosEffects[key]) {
                // Take all the subtypes in the hierarchy of this sprite.
                var allTypes = subTypes[key];
                if (!ef.enabled) continue;
				
                foreach (var stype in allTypes) {
                    // Add all sprites of this subtype to the list of
                    // sprites.
                    // These are sprites that could potentially collide with
                    // EOS
                    var sprites = getSprites(stype);
                    //try{
                    foreach (var sp in sprites) {
                        // Check if they are at the edge to trigger the
                        // effect. Also check that they
                        // are not dead (could happen in this same cycle).
                        if (isAtEdge(sp.rect) && !killList.Contains(sp) && !sp.is_disabled()) {
                            executeEffect(ef, sp, null);
                        }
                    }
//					}
//					catch(Exception e){
//						//TODO: figure out a way to handle offscreen spawning issue...
//						//Logger.getInstance().addMessage(new Message(Message.WARNING, "you can't spawn sprites outside of the screen."));
//							
//					}
                }
            }

        }

        // Now, we handle events between pairs of sprites, for each pair of
        // sprites that has a paired effect defined:
        foreach (var key in collisionEffects.Keys) {

            //pull all the needed sprites
            var firstx = new List<VGDLSprite>();
            var secondx = new List<VGDLSprite>();

            subTypes[key.Key].ForEach(item => firstx.AddRange(getSprites(item)));
			
            subTypes[key.Value].ForEach(item => secondx.AddRange(getSprites(item)));

            // We iterate over the (potential) multiple effects that these
            // two sprites could have defined between them.
            foreach (var ef in collisionEffects[key]) {
                if (!ef.enabled) continue;
				
                if (shieldedEffects.ContainsKey(key.Key) &&
                    shieldedEffects[key.Key].Contains(new KeyValuePair<string, int>(key.Value, ef.hashCode)))
                {
                    continue;
                }

                //NOTE: collision check could also happen in outer loop, 
                //but if a sprite is moved or killed in another effect, 
                //this effect might not occur. Whereas a pre-effect check,
                //would allow all collision effects to happen.					
                foreach (var s1 in firstx)
                {
                    //Don't execute effects on dead things
                    if (killList.Contains(s1)) continue;

                    var new_secondx = new List<VGDLSprite>();

                    foreach (VGDLSprite s2 in secondx) {
                        if ((s1 != s2 && s1.intersects(s2))) {
                            new_secondx.Add(s2);
                        }
                    }
                    
                    if(new_secondx.Count > 0) {
                        if (ef.inBatch) {
                            executeEffectBatch(ef, s1, new_secondx);
                        } else {

                            for (int i = 0; i < new_secondx.Count; i++) {
                                if (!killList.Contains(s1) && s1 != new_secondx[i] && s1.intersects(new_secondx[i])) {
                                    executeEffect(ef, s1, new_secondx[i]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private bool isAtEdge(Rect rect) {
        var r = new Rect(Vector2.zero, screenSize);
        return !r.Overlaps(rect);
    }
    
    private void executeEffectBatch(VGDLEffect ef, VGDLSprite s1, List<VGDLSprite> s2list) {
        // There is a collision. Apply the effect.
        int batchCount = ef.executeBatch(s1, s2list, this);
        if(batchCount == -1)
        {
            Debug.Log("WARNING: Batch collision not or bad implemented (batchCount == -1)");
            batchCount = 0; //So the game keeps making better sense.
        }

        // Affect score:
        if (ef.applyScore) {
            // apply scores for all avatars
            for (int i = 0; i < no_players; i++) {
                var multScore = ef.getScoreChange(i) * batchCount;
                avatars[i].addScore(multScore);
            }
        }

        // Add to events history.
        if (s1 != null && s2list != null)
            foreach (VGDLSprite s2 in s2list)
            {
                addEvent(s1, s2);
            }

        if (ef.count) {
            for (int i = 0; i < no_counters; i++) {
                var multCounter = ef.getCounter(i) * batchCount;
                this.counters[i] += multCounter;
            }
        }

        if (ef.countElse) {
            for (int i = 0; i < no_counters; i++) {
                var multElseCounter = ef.getCounterElse(i) * batchCount;
                this.counters[i] += multElseCounter;
            }
        }
    }

    public void executeEffect(VGDLEffect ef, VGDLSprite s1, VGDLSprite s2) {
        // There is a collision. Apply the effect.
        ef.execute(s1, s2, this);

        // Affect score:
        if (ef.applyScore) {
            // apply scores for all avatars
            for (int i = 0; i < no_players; i++) {
                avatars[i].addScore(ef.getScoreChange(i));
            }
        }

        // Add to events history.
		if (s1 != null && s2 != null)
			addEvent(s1, s2);

        if (ef.count) {
            for (int i = 0; i < no_counters; i++) {
                counters[i] += ef.getCounter(i);
            }
        }

        if (ef.countElse) {
            for (int i = 0; i < no_counters; i++) {
                counters[i] += ef.getCounterElse(i);
            }
        }
    }
    
    private void addEvent(VGDLSprite s1, VGDLSprite s2) {
        if (s1.is_avatar || s1.is_from_avatar)
            historicEvents.Add(
                new RecordedEvent(gameTick, s1.is_from_avatar, 
                    s1.getType(), s2.getType(), 
                    s1.spriteID, s2.spriteID, s1.getPosition()));

        else if (s2.is_avatar || s2.is_from_avatar)
            historicEvents.Add(
                new RecordedEvent(gameTick, s2.is_from_avatar, 
                    s2.getType(), s1.getType(), 
                    s2.spriteID, s1.spriteID, s2.getPosition()));
    }

    /// <summary>
    /// Performs one tick for the game: calling update(this) in all sprites. It
    /// follows the opposite order of the drawing order (inverse spriteOrder[]).
    ///	 Avatar is always updated first. Doesn't update disabled sprites.
    /// </summary>
    public void updateEntities() //aka tick
    {
        // Now, do all of the avatars.
        for (int i = 0; i < no_players; i++) {
            if (avatars[i] != null && !avatars[i].is_disabled()) {
                avatars[i].preMovement();
                avatars[i].updateAvatar(this, true, null);
            } else if (avatars[i] == null) {
                Debug.LogError(gameTick + ": Something went wrong, no avatar, ID = " + i);
            }
        }
		
        var spriteOrderCount = spriteOrder.Count;
        for (var i = spriteOrderCount - 1; i >= 0; --i) {
            var spriteType = spriteOrder[i];
            var spritesList = spriteGroups[spriteType];
            if (spritesList == null) continue;
			
            foreach (var sp in spritesList)
            {	
                if (!sp.is_avatar && !sp.is_disabled())
                {
                    sp.preMovement();
                    sp.update(this);
                }
            }
        }
    }

    public void checkTerminationConditions()
    {
        foreach (var termination in terminations)
        {
            if (termination.isDone(this))
            {
                isEnded = true;
				
                var result = new VGDLGameResult();

                for (int i = 0; i < no_players; i++)
                {
                    avatars[i].winState = termination.getWin(i);
					
                    result.playerAvatars.Add(avatars[i]);
                    result.playerOutcomes.Add(avatars[i].winState);
                    result.playerScores.Add(avatars[i].score);
                }

                gameResult = result;
                
                fwdModel.update(this);

                for (int i = 0; i < no_players; i++)
                {
                    avatars[i].passResult(this);
                }
            }
			
            //if any condition is met, the game is over, and so we need not test anymore.
            if (isEnded) break;
        }
		
        /*
		//NOTE: this is for the AI competition, to stop running a bot that produces too many warnings
		if(Logger.getInstance().getMessageCount() > CompetitionParameters.MAX_ALLOWED_WARNINGS){
			System.out.println("Finishing the game due to number of warnings: " + Logger.getInstance().getMessageCount() +
			 ". Messages will be flushed.");
			Logger.getInstance().printMessages();
		    isEnded = true;
		    Logger.getInstance().flushMessages();
		}
		 */
    }
    
    /**
	 * Handles the result for the game, considering disqualifications. Prints
	 * the result (score, time and winner) and returns the score of the game.
	 * Default player ID used 0 for single player games.
	 *
	 * @return the result of the game.
	 */
    public float[] handleResult() {
        // check all players disqualified and set scores
        for (var i = 0; i < avatars.Length; i++) {
            if (avatars[i] != null) {
                if (avatars[i].is_disqualified) {
                    avatars[i].winState = VGDLPlayerOutcomes.PLAYER_DISQ;
                    avatars[i].score = SCORE_DISQ;
                }
                // For sanity: winning a game always gives a positive score
                else if (avatars[i].winState == VGDLPlayerOutcomes.PLAYER_WINS)
                    if (avatars[i].score <= 0)
                        avatars[i].score = 1;
            }
        }

        // Prints the result: score, time and winner.
        // printResult();

        var scores = new float[no_players];
        for (var i = 0; i < no_players; i++) {
            if (avatars[i] == null) {
                scores[i] = SCORE_DISQ;
            } else {
                scores[i] = avatars[i].score;
            }
        }

        return scores;
    }
    
    /**
	 * Retuns the observation of this state.
	 *
	 * @return the observation.
	 */
    public StateObservation getObservation() {
        return new StateObservation(fwdModel.copy(), 0);
        //return new StateObservation(fwdModel, 0);
    }

    /**
     * Retuns the observation of this state (for multiplayer).
     *
     * @return the observation.
     */
    public StateObservationMulti getObservationMulti(int playerID) {
        return new StateObservationMulti(fwdModel.copy(), playerID);
        //return new StateObservationMulti(fwdModel, playerID);
    }
    
    public List<PathNode> getPath(Vector2 start, Vector2 end) {
        var pathStart = new Vector2(start.x, start.y);
        var pathEnd = new Vector2(end.x, end.y);

        pathStart = pathStart * (1.0f / block_size);
        pathEnd = pathEnd * (1.0f / block_size);

        return pathf.getPath(pathStart, pathEnd);
    }


    public Random.State getRandomGenerator()
    {
        
        return Random.state;
    }

    public int getNoPlayers() {
        return no_players;
    }

    public int getNoCounters() {
        return no_counters;
    }

    public int getValueCounter(int idx) {
        return counters[idx];
    }

    public VGDLPlayerOutcomes getWinner(int playerID = 0) {
        return avatars[playerID].winState;
    }

    public float getScore(int playerID = 0) {
        return avatars[playerID].score;
    }

    public VGDLPlayerOutcomes[] getMultiWinner() {
        var winners = new VGDLPlayerOutcomes[no_players];
        for (var i = 0; i < no_players; i++) {
            winners[i] = avatars[i].winState;
        }
        return winners;
    }
    
    /**
      * Reverses the direction of a given sprite.
      *
      * @param sprite
      *            sprite to reverse.
      */
    public void reverseDirection(VGDLSprite sprite) {
        sprite.orientation = new Vector2(-sprite.orientation.x, -sprite.orientation.y);
    }
    
    /**
	 * Gets the maximum amount of resources of type resourceId that are allowed
	 * by entities in the game.
	 *
	 * @param resourceId
	 *            the id of the resource to query for.
	 * @return maximum amount of resources of type resourceId.
	 */
    public int getResourceLimit(string resourceId)
    {
        return !resourceLimits.ContainsKey(resourceId) ? 0 : resourceLimits[resourceId];
    }

    //NOTE: Buckets are not used in JavaVGDL
//    protected class Bucket {
//        List<VGDLSprite> allSprites;
//        Dictionary<int, List<VGDLSprite>> spriteLists;
//        int totalNumSprites;
//
//        public Bucket() {
//            allSprites = new List<VGDLSprite>();
//            spriteLists = new Dictionary<int, List<VGDLSprite>>();
//            totalNumSprites = 0;
//        }
//
//        public void clear() {
//            allSprites.Clear();
//            spriteLists.Clear();
//            totalNumSprites = 0;
//        }
//
//        public void add(VGDLSprite sp) {
//            int bucket = sp.bucket;
//            List<VGDLSprite> sprites;
//            if (!spriteLists.ContainsKey(bucket)) {
//                sprites = new List<VGDLSprite>();
//                spriteLists.Add(bucket, sprites);
//            }
//            else
//            {
//                sprites = spriteLists[bucket];
//            }
//            sprites.Add(sp);
//            allSprites.Add(sp);
//            totalNumSprites++;
//        }
//
//        public int size() {
//            return totalNumSprites;
//        }
//
//        public int size(int bucket) {
//            if (!spriteLists.ContainsKey(bucket))
//                return 0;
//            return spriteLists[bucket].Count;
//        }
//
//        public List<VGDLSprite> getAllSprites() {
//            return allSprites;
//        }
//
//        public Dictionary<int, List<VGDLSprite>> getSpriteList() {
//            return spriteLists;
//        }
//    }

    public void Abort()
    {
        isEnded = true;
    }
}