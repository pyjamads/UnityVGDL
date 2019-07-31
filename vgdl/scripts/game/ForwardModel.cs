/**
 * Created with IntelliJ IDEA.
 * User: Diego
 * Date: 13/11/13
 * Time: 15:37
 * This is a Java port from Tom Schaul's VGDL - https://github.com/schaul/py-vgdl
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ForwardModel : VGDLGame
{

    /**
     * ID of the player that gets this FM
     */
    int playerID;

    /**
     * Private sampleRandom generator. Rolling the state forward from this state
     * observation will use this sampleRandom generator, different from the one
     * that is used in the real game.
     */
    private Random.State randomObs;

    //NOTE: Lists in JavaVGDL are arrays of booleans indexed by itype,
    //I've opted to just pull reference the lists in the spriteGroups dictionary.
    /**
     * bool map of sprite types that are players.
     * npcList[spriteType]==true : spriteType is NPC.
     */
    private Dictionary<string, List<VGDLSprite>> playerList;

    /**
     * bool map of sprite types that are NPCs.
     * npcList[spriteType]==true : spriteType is NPC.
     */
    private Dictionary<string, List<VGDLSprite>> npcList;

    /**
     * bool map of sprite types that are immovable sprites.
     * immList[spriteType]==true : spriteType is immovable sprite.
     */
    private Dictionary<string, List<VGDLSprite>> immList;

    /**
     * bool map of sprite types that can move.
     * movList[spriteType]==true : spriteType can move.
     */
    private Dictionary<string, List<VGDLSprite>> movList;

    /**
     * bool map of sprite types that are resources.
     * resList[spriteType]==true : spriteType is resource.
     */
    private Dictionary<string, List<VGDLSprite>> resList;

    /**
     * bool map of sprite types that are portals or doors.
     * portalList[spriteType]==true : spriteType is portal or door.
     */
    private Dictionary<string, List<VGDLSprite>> portalList;

    /**
     * bool map of sprite types that created by the avatar.
     * fromAvatar[spriteType]==true : spriteType is created by the avatar.
     */
    private Dictionary<string, List<VGDLSprite>> fromAvatar;

    /**
     * bool map of sprite types that are unknown.
     * unknownList[spriteType]==false : spriteType is unknown.
     */
    private Dictionary<string, bool> knownList;

    /**
     * bool map of sprite types that are not hidden.
     * visibleList[playerID][spriteType]==true : sprite.hidden[playerID] = false;
     */
    private Dictionary<string, bool>[] visibleList;

    /**
     * List of (persistent) observations for all sprites, indexed by sprite ID.
     */
    private Dictionary<int, Observation> observations;

    /**
     * Observation grid
     */
    private List<Observation>[][] observationGrid;

    /**
     * Constructor for StateObservation. Initializes everything
     * @param a_gameState
     */
    public ForwardModel(VGDLGame a_gameState, int playerID)
    {
        this.playerID = playerID;

        //All static elements of the game are assigned from the game we create the copy from.
        initNonVolatile(a_gameState);

        //Init those variables that take a determined value at the beginning of a game.
        init();
    }


    /**
     * Dumps the game state into 'this' object. Effectively, creates a state observation
     * from a game state (of class Game).
     * @param a_gameState game to take the state from.
     */
    public void update(VGDLGame a_gameState)
    {
        //int numSpriteTypes = a_gameState.spriteGroups.Count;
        killList = new List<VGDLSprite>();
//        bucketList = new List<Bucket>();
        historicEvents = new List<RecordedEvent>();
        shieldedEffects = new Dictionary<string, List<KeyValuePair<string, int>>>();

        //Copy of sprites from the game.
        spriteGroups = new Dictionary<string, List<VGDLSprite>>();
        num_sprites = 0;
        
        foreach (var spriteGroup in a_gameState.spriteGroups)
        {
            /**
             * Index in the sprite group passed to the checkSpriteFeatures method to
             * identify player in case of avatar sprites (index used as
             * playerID in the avatars array).
             */
            foreach (var sp in spriteGroup.Value)
            {
                var constructorInfo = sp.GetType().GetConstructor(new[] {sp.GetType()});
                if (constructorInfo == null)
                {
                    throw new NotImplementedException("A copy constructor is needed for ["+sp.GetType().Name+"]: \n\n" +
                                                      "public "+sp.GetType().Name+"("+sp.GetType().Name+" from) : base(from){\n"+
                                                        "\t//Insert fields to be copied by copy constructor.\n"+
                                                      "}\n");
                }
                var spCopy = constructorInfo.Invoke(new []{sp}) as VGDLSprite;
                addSprite(spriteGroup.Key, spCopy);
                
                String hidden = "False";
                if (spCopy.hidden != null) {
                    String[] split = spCopy.hidden.Split(',');
                    if (playerID > split.Length - 1)
                        hidden = split[split.Length - 1];
                    else
                        hidden = split[playerID];
                }

                if (bool.Parse(hidden)) continue;

                //NOTE: we only need to check once per stype, because all instances will have the same properties.
                if (!knownList.ContainsKey(spriteGroup.Key))
                {
                    checkSpriteFeatures(spCopy, spriteGroup.Key);
                    //NOTE: this is done inside of the checkSpriteFeatures function
                    //knownList[spriteGroup.Key] = true;
                }
                
                updateObservation(spCopy);
            }
            
            //NOTE: we don't need to do this, because we're using VGDLGame.addSprite(), that already takes care of it.
//            int nSprites = spriteGroups[i].numSprites();
//            num_sprites += nSprites;

            //copy the shields
            foreach (var shieldedEffect in a_gameState.shieldedEffects)
            {
                foreach (var pair in shieldedEffect.Value)
                {
                    addShield(shieldedEffect.Key, pair.Key, pair.Value);    
                }
            }
        }
        
        //events:
        foreach (var historicEvent in a_gameState.historicEvents) {
            historicEvents.Add(historicEvent.copy());
        }

        //copy the time effects:
        this.timeEffects = new List<VGDLTimeEffect>();
        a_gameState.timeEffects.ForEach(item => this.timeEffects.Add(new VGDLTimeEffect(item)));
        
        //Debug.Log("Tef size: " + this.timeEffects.size());

        //Game state variables:
        this.gameTick = a_gameState.gameTick;
        this.isEnded = a_gameState.isEnded;
        this.avatarLastAction = new VGDLAvatarActions[no_players];
        avatarLastAction = (VGDLAvatarActions[])a_gameState.avatarLastAction.Clone();
        this.nextSpriteID = a_gameState.nextSpriteID;
    }

    /**
     * Updates the persistent observation of this sprite, or creates it if the
     * observation is new.
     * @param sprite sprite to take the observation from.
     */
    private void updateObservation(VGDLSprite sprite)
    {
        var spriteId = sprite.spriteID;
        bool moved = false, newObs = false;

        var oldPosition = Vector2.negativeInfinity;

        Observation obs = null;
        var success = observations.TryGetValue(spriteId, out obs);
        if(success && obs != null)
        {
            oldPosition = obs.position;
            var position = sprite.getPosition();
            moved = ! obs.position.Equals(position);
            obs.position = position;
        }else
        {
            obs = createSpriteObservation(sprite);
            newObs = true;
        }

        updateGrid(obs, newObs, moved, oldPosition);
    }

    /**
     * Removes an sprite observation.
     * @param sprite sprite to Remove.
     */
    public void removeSpriteObservation(VGDLSprite sprite)
    {
        var spriteId = sprite.spriteID;
        
        Observation obs = null;
        var success = observations.TryGetValue(spriteId, out obs);        
        if(success && obs != null)
        {
            removeObservationFromGrid(obs, obs.position);
            observations.Remove(spriteId);
        }
    }

    /**
     * Updates a grid observation.
     * @param obs observation to update
     * @param newObs if this is a new observation.
     * @param moved if it is a past observation, and it moved.
     * @param oldPosition the old position of this observation if it moved.
     */
    private void updateGrid(Observation obs, bool newObs, bool moved, Vector2 oldPosition)
    {
        //Insert observation in the grid position.
        if(newObs || moved)
        {
            //First, Remove observation if the sprite moved.
            if(moved)
                removeObservationFromGrid(obs, oldPosition);

            addObservationToGrid(obs, obs.position);
        }
    }

    /**
     * Removes an observation to the grid, from the position specified.
     * @param obs observation to delete.
     * @param position where the sprite was located last time seen.
     */
    private void removeObservationFromGrid(Observation obs, Vector2 position)
    {
        int x = (int) position.x / block_size;
        bool validX = x >= 0 && x < observationGrid.Length;
        bool xPlus = (position.x % block_size) > 0 && (x+1 < observationGrid.Length);
        int y = (int) position.y / block_size;
        bool validY = y >= 0 && y < observationGrid[0].Length;
        bool yPlus = (position.y % block_size) > 0 && (y+1 < observationGrid[0].Length);

        if(validX && validY)
        {
            observationGrid[x][y].Remove(obs);
            if(xPlus)
                observationGrid[x+1][y].Remove(obs);
            if(yPlus)
                observationGrid[x][y+1].Remove(obs);
            if(xPlus && yPlus)
                observationGrid[x+1][y+1].Remove(obs);
        }
    }

    /**
     * Adds an observation to the grid, in the position specified.
     * @param obs observation to Add.
     * @param position where to be added.
     */
    private void addObservationToGrid(Observation obs, Vector2 position)
    {
        int x = (int) position.x / block_size;
        bool validX = x >= 0 && x < observationGrid.Length;
        bool xPlus = (position.x % block_size) > 0 && (x+1 < observationGrid.Length);
        int y = (int) position.y / block_size;
        bool validY = y >= 0 && y < observationGrid[0].Length;
        bool yPlus = (position.y % block_size) > 0 && (y+1 < observationGrid[0].Length);

        if(validX && validY)
        {
            observationGrid[x][y].Add(obs);
            if(xPlus)
                observationGrid[x+1][y].Add(obs);
            if(yPlus)
                observationGrid[x][y+1].Add(obs);
            if(xPlus && yPlus)
                observationGrid[x+1][y+1].Add(obs);
        }
    }

    /**
     * Prints the observation grid. For debug only.
     */
    public void printObservationGrid()
    {
        Debug.Log("#########################");
        for(int j = 0; j < observationGrid[0].Length; ++j)
        {
            for(int i = 0; i < observationGrid.Length; ++i)
            {
                int n = observationGrid[i][j].Count;
                if(n > 0)
                    Debug.Log(n.ToString());
                else
                    Debug.Log(' '.ToString());
            }
            Debug.Log("");
        }
    }


    /**
     * Creates the sprite observation of a given sprite.
     * @param sprite sprite to create the observation from.
     * @return the observation object.
     */
    private Observation createSpriteObservation(VGDLSprite sprite)
    {
        var category = getSpriteCategory(sprite);
        Observation obs = new Observation(sprite.getType(), sprite.spriteID, sprite.getPosition(), VGDLUtils.VGDLDirections.NIL.getDirection(), category);
        observations.Add(sprite.spriteID, obs);
        return obs;
    }

    /**
     * Gets the sprite observation of a given sprite. Creates it if id didn't exist.
     * @param sprite sprite to get/create the observation from.
     * @return the observation object.
     */
    private Observation getSpriteObservation(VGDLSprite sprite)
    {
        var spriteId = sprite.spriteID;
        
        Observation obs = null;
        var success = observations.TryGetValue(spriteId, out obs);  
        if(success && obs != null)
        {
            return obs;
        }else{
            return createSpriteObservation(sprite);
        }
    }

    /**
     * Checks some features of the sprite, to categorize it.
     * @param sp Sprite to categorize.
     * @param itype itype of the sprite.
     */
    private void checkSpriteFeatures(VGDLSprite sp, string stype)
    {
        var category = getSpriteCategory(sp);
        switch (category)
        {
            case VGDLUtils.VGDLSpriteCategory.TYPE_AVATAR:
                //update avatar sprite.
                var a = sp as VGDLAvatar;
                if(a != null && a.getKeyHandler() != null){
                    this.avatars[a.playerID] = a;
                }
                playerList[stype] = spriteGroups[stype];
                break;
            case VGDLUtils.VGDLSpriteCategory.TYPE_RESOURCE:
                resList[stype] = spriteGroups[stype];
                break;
            case VGDLUtils.VGDLSpriteCategory.TYPE_PORTAL:
                portalList[stype] = spriteGroups[stype];
                break;
            case VGDLUtils.VGDLSpriteCategory.TYPE_NPC:
                npcList[stype] = spriteGroups[stype];
                break;
            case VGDLUtils.VGDLSpriteCategory.TYPE_STATIC:
                immList[stype] = spriteGroups[stype];
                break;
            case VGDLUtils.VGDLSpriteCategory.TYPE_FROMAVATAR:
                fromAvatar[stype] = spriteGroups[stype];
                break;
            case VGDLUtils.VGDLSpriteCategory.TYPE_MOVABLE:
                movList[stype] = spriteGroups[stype];
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        knownList[stype] = true;

        String hidden = "False";
        if (sp.hidden != null) {
            String[] split = sp.hidden.Split(',');
            if (playerID > split.Length - 1)
                hidden = split[split.Length - 1];
            else
                hidden = split[playerID];
        }
        visibleList[playerID][stype] = !bool.Parse(hidden);
    }

    private VGDLUtils.VGDLSpriteCategory getSpriteCategory(VGDLSprite sp)
    {
        if(sp.is_avatar)
            return VGDLUtils.VGDLSpriteCategory.TYPE_AVATAR;

        //Is it a resource?
        if(sp.is_resource)
            return VGDLUtils.VGDLSpriteCategory.TYPE_RESOURCE;

        //Is it a portal?
        if(sp.portal)
            return VGDLUtils.VGDLSpriteCategory.TYPE_PORTAL;

        //Is it npc?
        if(sp.is_npc)
            return VGDLUtils.VGDLSpriteCategory.TYPE_NPC;

        //Is it immovable?
        if(sp.is_static)
            return VGDLUtils.VGDLSpriteCategory.TYPE_STATIC;

        //is it created by the avatar?
        if(sp.is_from_avatar)
            return VGDLUtils.VGDLSpriteCategory.TYPE_FROMAVATAR;

        return VGDLUtils.VGDLSpriteCategory.TYPE_MOVABLE;
    }

    /**
     * Initializes the variables of this game that have always a determined value at the beginning
     * of any game.
     */
    private void init()
    {
        //TODO: figure out where to do the Random.InitState();
        this.randomObs = Random.state;
        this.gameTick = 0;
        this.isEnded = false;
        
    }

    /**
     * Initializes the non volatile elements of a game (constructors, termination conditions,
     * effects, etc). 'this' takes these from a_gameState,
     * @param a_gameState Reference to the original game
     */
    private void initNonVolatile(VGDLGame a_gameState)
    {
        //We skip this.resource_colors and sampleRandom.
        this.spriteOrder = a_gameState.spriteOrder;
        //NOTE: this is derived from the spriteConstructors 
        //this.singletons = a_gameState.singletons;
        //NOTE: replaced by spriteConstructors
        //this.classConst = a_gameState.classConst;
        this.spriteContructors = a_gameState.spriteContructors;
        //NOTE: unused in normal games (used for design competition)
        //this.parameters = a_gameState.parameters;
        //NOTE: our spriteConstruction system doesn't use these
        //this.templateSprites = a_gameState.templateSprites;
        
        this.collisionEffects = a_gameState.collisionEffects;
        //NOTE: unused, but might be a valuable optimization!
        //this.definedEffects = a_gameState.definedEffects;
        
        this.eosEffects = a_gameState.eosEffects;
        //NOTE: unused, but might be a valuable optimization!
        //this.definedEOSEffects = a_gameState.definedEOSEffects;
        this.subTypes = a_gameState.subTypes;
        this.charMapping = a_gameState.charMapping;
        this.terminations = a_gameState.terminations;
        this.resourceLimits = a_gameState.resourceLimits;
        this.screenSize = a_gameState.screenSize;
        this.size = a_gameState.size;
        this.block_size = a_gameState.block_size;
        //NOTE: java version does self assignment, of this static variable, meaning setting it to it's own value.
        //this.MAX_SPRITES = a_gameState.MAX_SPRITES;
        this.no_players = a_gameState.no_players;
        this.no_counters = a_gameState.no_counters;
        
        //System.arraycopy(a_gameState.avatarLastAction, 0, avatarLastAction, 0, no_players);
        this.avatarLastAction = (VGDLAvatarActions[])a_gameState.avatarLastAction.Clone();//new VGDLAvatarActions[no_players];

         
        this.avatars = new VGDLAvatar[no_players];
        //NOTE: it seems incorrect that the avatars get copied as non-volatile, it's already being initialized in update
//        for (int i = 0; i < no_players; i++) {
//            if(a_gameState.avatars[i] != null){
//                
//                avatars[i] = (MovingAvatar) a_gameState.avatars[i].copy();
//                avatars[i].setKeyHandler(a_gameState.avatars[i].getKeyHandler());
//            }
//        }
        
        this.counters = (int[]) a_gameState.counters.Clone();
        //System.arraycopy(a_gameState.counter, 0, this.counter, 0, no_counters);

        //create the bool maps of sprite types.
        npcList = new Dictionary<string, List<VGDLSprite>>();
        immList = new Dictionary<string, List<VGDLSprite>>();
        movList = new Dictionary<string, List<VGDLSprite>>();
        resList = new Dictionary<string, List<VGDLSprite>>();
        portalList  = new Dictionary<string, List<VGDLSprite>>();
        fromAvatar  = new Dictionary<string, List<VGDLSprite>>();
        knownList = new Dictionary<string, bool>();
        visibleList = new Dictionary<string, bool>[no_players];

        for (var index = 0; index < visibleList.Length; index++)
        {
            visibleList[index] = new Dictionary<string, bool>();
        }

        playerList  = new Dictionary<string, List<VGDLSprite>>();

        observations = new Dictionary<int, Observation>();
        observationGrid = new List<Observation>[(int) (screenSize.x/block_size)][];

        for (int i = 0; i < observationGrid.Length; i++)
        {
            observationGrid[i] = new List<Observation>[(int) (screenSize.y/block_size)];
        }
        
        for(int i = 0; i < observationGrid.Length; ++i)
            for(int j = 0; j < observationGrid[i].Length; ++j)
                observationGrid[i][j] = new List<Observation>();

        this.pathf = a_gameState.pathf;
    }


    /**
     * Returns the sampleRandom generator of this forward model. It is not the same as the
     * sampleRandom number generator of the main game copy.
     * @return the sampleRandom generator of this forward model.
     */
     public Random.State getRandomGenerator()
    {
        return randomObs;
    }

    /**
     * Sets a new seed for the forward model's random generator (creates a new object)
     *
     * @param seed the new seed.
     */
    public void setNewSeed(int seed)
    {
        Random.InitState(seed);
        randomObs = Random.state;
    }


    /************** Useful functions for the agent *******************/

    /**
     * Method used to access the number of players in a game.
     * @return number of players.
     */
    public int getNoPlayers() { return no_players; }

    /**
     * Calls update(this) in avatar sprites. It uses the action received as the action of the avatar.
     * Doesn't update disabled avatars.
     * @param action Action to be performed by the avatar for this game tick.
     */
    protected void updateAvatars(VGDLAvatarActions action, int playerID)
    { 
        var a = avatars[playerID];
        if (!a.is_disabled()) {
            
            //apply action to correct avatar
            a.preMovement();
            a.updateAvatar(this, false, new List<VGDLAvatarActions>{action});
            avatarLastAction[playerID] = action;
        }
    }

    /**
     * Performs one tick for the game, calling update(this) in all sprites.
     * It follows the same order of update calls as in the real game (inverse spriteOrder[]).
     * Doesn't update disabled sprites.
     */
    protected void tick() {
        for(int i = spriteOrder.Count-1; i >= 0; --i)
        {
            var stype = spriteOrder[i];

            foreach (var sp in spriteGroups[stype])
            {
                if(!(sp is VGDLAvatar) && !sp.is_disabled())
                {
                    sp.preMovement();
                    sp.update(this);
                }
            }
        }
    }


    /**
     * Advances the forward model using the action supplied.
     * @param action
     */
     public void advance(VGDLAvatarActions action) {
        if(!isEnded) {
            //apply player action
            updateAvatars(action, 0);
            //update all the other sprites
            tick();
            //update game state
            advance_aux();
        }
    }

    /**
     * Advances the forward model using the actions supplied.
     * @param actions array of actions of all players (index in array corresponds
     *                to playerID).
     */
     public void advance(VGDLAvatarActions[] actions) {

        if(!isEnded) {
            //apply actions of all players
            for (int i = 0; i < actions.Length; i++) {
                VGDLAvatarActions a = actions[i]; // action
                updateAvatars(a, i); // index in array actions is the playerID
            }
            //update all other sprites in the game
            tick();
            //update game state
            advance_aux();
        }
        //Debug.Log(isMultiGameOver());
    }

    /**
     * Auxiliary method for advance methods, to avoid code duplication.
     */
    private void advance_aux() {
        doEventHandling();
        clearAll(fwdModel);
        checkTerminationConditions();
        //checkTimeOut(); //NOTE: timeout is handled by checkTerminationConditions
        updateAllObservations();
        gameTick++;
    }

    /**
     * Updates all observations of this class.
     */
    private void updateAllObservations() {
        //Now, update all others (but avatar).
        int typeIndex = spriteOrder.Count-1;
        for(int i = typeIndex; i >=0; --i)   //For update, opposite order than drawing.
        {
            var stype = spriteOrder[i];

            foreach (var sp in spriteGroups[stype])
            {
                updateObservation(sp);                
            }
        }
    }

    /**
     * Creates a copy of this forward model.
     * @return the copy of this forward model.
     */
     public ForwardModel copy() {
        ForwardModel copyObs = new ForwardModel(this, this.playerID);
        copyObs.update(this);
        return copyObs;
    }

    /**
     * Gets the game score of this state.
     * @return the game score.
     */
    public float getGameScore() { return this.avatars[0].score; }

    /**
     * Method overloaded for multi player games.
     * Gets the game score of a particular player (identified by playerID).
     * @param playerID ID of the player to query.
     * @return the game score.
     */
    public float getGameScore(int playerID) { return this.avatars[playerID].score; }

    /**
     * Gets the current game tick of this particular state.
     * @return the game tick of the current game state.
     */
    public int getGameTick() { return this.gameTick; }

    /**
     * Indicates if there is a game winner in the current observation.
     * Possible values are VGDLPlayerOutcomes.PLAYER_WINS, VGDLPlayerOutcomes.PLAYER_LOSES and
     * VGDLPlayerOutcomes.NO_WINNER.
     * @return the winner of the game.
     */
    public VGDLPlayerOutcomes getGameWinner() { return this.avatars[0].winState; }

    /**
     * Method overloaded for multi player games.
     * Indicates if there is a game winner in the current observation.
     * Possible values are VGDLPlayerOutcomes.PLAYER_WINS, VGDLPlayerOutcomes.PLAYER_LOSES and
     * VGDLPlayerOutcomes.NO_WINNER.
     * @return the winner of the game.
     */
    public VGDLPlayerOutcomes[] getMultiGameWinner() {
        VGDLPlayerOutcomes[] winners = new VGDLPlayerOutcomes[no_players];
        for (int i = 0; i < no_players; i++) {
            winners[i] = avatars[i].winState;
        }
        return winners; }

    /**
     * Indicates if the game is over or if it hasn't finished yet.
     * @return true if the game is over.
     */
    public bool isGameOver() { return getGameWinner() != VGDLPlayerOutcomes.NO_WINNER; }

    /**
     * Indicates if the game is over or if it hasn't finished yet.
     * @return true if the game is over.
     */
    public bool isMultiGameOver() {
        for (int i = 0; i < no_players; i++) {
            if (getMultiGameWinner()[i] == VGDLPlayerOutcomes.NO_WINNER) return false;
        }
        return true;
    }

    /**
     * Returns the world dimensions, in pixels.
     * @return the world dimensions, in pixels.
     */
    public Vector2 getWorldDimension()
    {
        return screenSize;
    }


    /** avatar-dependent functions **/

    /**
     * Returns the position of the avatar. If the game is finished, we cannot guarantee that
     * this position reflects the real position of the avatar (the avatar itself could be
     * destroyed). If game finished, this returns Types.NIL.
     * @return position of the avatar, or Types.NIL if game is over.
     */
    public Vector2 getAvatarPosition() { return getAvatarPosition(0); }

    /**
     * Method overloaded for multi player games.
     * @param playerID ID of the player to query.
     */
    public Vector2 getAvatarPosition(int playerID) {
        if(isEnded)
            VGDLUtils.getDirection(VGDLUtils.VGDLDirections.NIL);
        return avatars[playerID].getPosition();
    }

    /**
     * Returns the speed of the avatar. If the game is finished, we cannot guarantee that
     * this speed reflects the real speed of the avatar (the avatar itself could be
     * destroyed). If game finished, this returns 0.
     * @return orientation of the avatar, or 0 if game is over.
     */
    public float getAvatarSpeed() { return getAvatarSpeed(0); }

    /**
     * Method overloaded for multi player games.
     * @param playerID ID of the player to query.
     */
    public float getAvatarSpeed(int playerID) {
        if(isEnded)
            return 0;
        return avatars[playerID].speed;
    }

    /**
     * Returns the orientation of the avatar. If the game is finished, we cannot guarantee that
     * this orientation reflects the real orientation of the avatar (the avatar itself could be
     * destroyed). If game finished, this returns Types.NIL.
     * @return orientation of the avatar, or Types.NIL if game is over.
     */
    public Vector2 getAvatarOrientation() { return getAvatarOrientation(0); }

    /**
     * Method overloaded for multi player games.
     * @param playerID ID of the player to query.
     */
    public Vector2 getAvatarOrientation(int playerID) {
        if(isEnded)
            return VGDLUtils.VGDLDirections.NIL.getDirection();
        return new Vector2(avatars[playerID].orientation.x, avatars[playerID].orientation.y);
    }

    /**
     * Returns the actions that are available in this game for
     * the avatar. If the parameter 'includeNIL' is true, the array contains the (always available)
     * NIL action. If it is false, this is equivalent to calling getAvailableActions().
     * @param includeNIL true to include Types.ACTIONS.ACTION_NIL in the array of actions.
     * @return the available actions.
     */
    public List<VGDLAvatarActions> getAvatarActions(bool includeNIL)
    {
        return getAvatarActions(0, includeNIL);
    }

    /**
     * Method overloaded for multi player games.
     * @param playerID ID of the player to query.
     */
    public List<VGDLAvatarActions> getAvatarActions(int playerID, bool includeNIL) {
        if(isEnded || avatars[playerID] == null)
            return new List<VGDLAvatarActions>();
        if(includeNIL)
            return avatars[playerID].actionsNIL;
        return avatars[playerID].actions;
    }

    /**
     * Returns the resources in the avatar's possession. As there can be resources of different
     * nature, each entry is a key-value pair where the key is the resource ID, and the value is
     * the amount of that resource type owned. It should be assumed that there might be other resources
     * available in the game, but the avatar could have none of them.
     * If the avatar has no resources, an empty HashMap is returned.
     * @return resources owned by the avatar.
     */
    public Dictionary<int, int> getAvatarResources()
    {
        return getAvatarResources(0);
    }

    /**
     * Method overloaded for multi player games.
     * @param playerID ID of the player to query.
     */
    public Dictionary<int, int> getAvatarResources(int playerID) {
        //Determine how many different resources does the avatar have.
        Dictionary<int, int> owned = new Dictionary<int, int>();

        if(avatars[playerID] == null)
            return owned;

        var idx = 0;
        foreach (var resource in avatars[playerID].resources)
        {
            //NOTE: translates the stype to idx, but that might be a problem. 
            //TODO: test that this makes sense! (resource ID would be a better key, but it doesn't exist here)
            owned[idx] = resource.Value;
            idx++;
        }
        
        return owned;
    }

    /**
     * Returns the avatar's last move. At the first game cycle, it returns ACTION_NIL.
     * Note that this may NOT be the same as the last action given by the agent, as it may
     * have overspent in the last game cycle.
     * @return the action that was executed in the real game in the last cycle. ACTION_NIL
     * is returned in the very first game step.
     */
    public VGDLAvatarActions getAvatarLastAction() { return getAvatarLastAction(0); }

    /**
     * Method overloaded for multi player games.
     * @param playerID ID of the player to query.
     */
    public VGDLAvatarActions getAvatarLastAction(int playerID) {
        if(avatarLastAction[playerID] != VGDLAvatarActions.ACTION_NIL)
            return avatarLastAction[playerID];
        else return VGDLAvatarActions.ACTION_NIL;
    }


    /**
     * Returns the avatar's type. In case it has multiple types, it returns the most specific one.
     * @return the itype of the avatar.
     */
    public string getAvatarType()
    {
        return getAvatarType(0);
    }

    /**
     * Method overloaded for multi player games.
     * @param playerID ID of the player to query.
     */
    public string getAvatarType(int playerID)
    {
        return avatars[playerID].getType();
    }

    /**
     * Returns the health points of the avatar. A value of 0 doesn't necessarily
     * mean that the avatar is dead (could be that no health points are in use in that game).
     * @return a numeric value, the amount of remaining health points.
     */
    public int getAvatarHealthPoints() { return getAvatarHealthPoints(0); }

    /**
     * Method overloaded for multi player games.
     * @param playerID ID of the player to query.
     */
    public int getAvatarHealthPoints(int playerID) { return avatars[playerID].healthPoints; }


    /**
     * Returns the maximum amount of health points.
     * @return the maximum amount of health points the avatar can have.
     */
    public int getAvatarMaxHealthPoints() { return getAvatarMaxHealthPoints(0); }

    /**
     * Method overloaded for multi player games.
     * @param playerID ID of the player to query.
     */
    public int getAvatarMaxHealthPoints(int playerID) { return avatars[playerID].maxHealthPoints; }

    /**
     * Returns the limit of health points this avatar can have.
     * @return the limit of health points the avatar can have.
     */
    public int getAvatarLimitHealthPoints() {return getAvatarLimitHealthPoints(0);}

    /**
     * Method overloaded for multi player games.
     * @param playerID ID of the player to query.
     */
    public int getAvatarLimitHealthPoints(int playerID) {return avatars[playerID].limitHealthPoints;}

    /**
     * Returns true if the avatar is alive
     * @return true if the avatar is alive
     */
    public bool isAvatarAlive()
    {
        return isAvatarAlive(0);
    }

    /**
     * Method overloaded for multi player games, returns true if the avatar is still alive.
     *
     * @param playerID ID of the player to query.
     */
    public bool isAvatarAlive(int playerID) {
        return !avatars[playerID].is_disabled();
    }

    /** Methods that return positions of things **/

    /**
     * Gets position from the sprites corresponding to the bool map passed by parameter.
     * @param groupArray bool map that indicates which sprite types must be considered.
     * @return List of arrays with Observations. Each entry in the array corresponds to a different
     * sprite type.
     */
    private List<Observation>[] getPositionsFrom(Dictionary<string, List<VGDLSprite>> groupArray, Vector2 refPosition)
    {
        //First, get how many types we have. Need to consider hidden sprites out.
        int numDiffTypes = 0;
        
        foreach (var type in groupArray)
        {
            //There is a sprite type we don't know anything about. Need to check.
            if(!knownList[type.Key] && type.Value != null && type.Value.Count > 0)
                checkSpriteFeatures(type.Value[0], type.Key);

            if(visibleList[playerID][type.Key]) numDiffTypes++;
        }

        if(numDiffTypes == 0)
            return null; //Wait, no types? no sprites of this group then.

        var observations = new List<Observation>[numDiffTypes];
        var reference = refPosition;
        if(refPosition.Equals(Vector2.negativeInfinity))
            reference = VGDLAvatarActions.ACTION_NIL.getDirection();

        var idx = 0;
        foreach (var type in groupArray)
        {
            if (!visibleList[playerID][type.Key]) continue;
            
            observations[idx] = new List<Observation>();
            foreach (var sp in type.Value)
            {
                var observation = getSpriteObservation(sp);
                observation.update(sp.getType(), sp.spriteID, sp.getPosition(), reference, getSpriteCategory(sp));

                observation.reference = reference;
                observations[idx].Add(observation);
            }

            if(refPosition.Equals(Vector2.negativeInfinity))
            {
                observations[idx].Sort();
            }

            idx++;
        }
        
        return observations;
    }

    /**
     * Returns a grid with all observations in the level.
     * @return the grid of observations
     */
    public List<Observation>[][] getObservationGrid()
    {
        return observationGrid;
    }

    /**
     * Returns the list of historic events happened in this game so far.
     * @return list of historic events happened in this game so far.
     */
    public List<RecordedEvent> getEventsHistory()
    {
        return historicEvents;
    }

    /**
     * Returns a list of observations of NPC in the game. As there can be
     * NPCs of different type, each entry in the array corresponds to a sprite type.
     * Every ArrayList contains a list of objects of type Observation, ordered asc. by
     * distance to the avatar. Each Observation holds the position, unique id and
     * sprite id of that particular sprite.
     *
     * @param refPosition Reference position to use when sorting this array,
     *                    by ascending distance to this point.
     * @return Observations of NPCs in the game.
     */
    public List<Observation>[] getNPCPositions(Vector2 refPosition)
    {
        return getPositionsFrom(npcList, refPosition);
    }

    /**
     * Observations of static objects in the game.
     * @param refPosition Reference position to use when sorting this array,
     *                    by ascending distance to this point.
     * @return a list with the observations of static objects in the game..
     */
    public List<Observation>[] getImmovablePositions(Vector2 refPosition) {
        return getPositionsFrom(immList, refPosition);
    }

    /**
     * Returns a list with observations of sprites that move, but are NOT NPCs.
     * @param refPosition Reference position to use when sorting this array,
     *                    by ascending distance to this point.
     * @return a list with observations of sprites that move, but are NOT NPCs.
     */
    public List<Observation>[] getMovablePositions(Vector2 refPosition) {
        return getPositionsFrom(movList, refPosition);
    }

    /*
     * Returns a list with observations of resources.
     * @param refPosition Reference position to use when sorting this array,
     *                    by ascending distance to this point.
     * @return a list with observations of resources.
     */
    public List<Observation>[] getResourcesPositions(Vector2 refPosition) {
        return getPositionsFrom(resList, refPosition);
    }

    /*
     * Returns a list with observations of portals.
     * @param refPosition Reference position to use when sorting this array,
     *                    by ascending distance to this point.
     * @return a list with observations of portals.
     */
    public List<Observation>[] getPortalsPositions(Vector2 refPosition) {
        return getPositionsFrom(portalList, refPosition);
    }

    /**
     * Returns a list of observations of objects created by the avatar's actions.
     * @param refPosition Reference position to use when sorting this array,
     *                    by ascending distance to this point.
     * @return a list with observations of sprites.
     */
    public List<Observation>[] getFromAvatarSpPositions(Vector2 refPosition)
    {
        return getPositionsFrom(fromAvatar, refPosition);
    }
}