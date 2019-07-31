using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class VGDLSprite
{
/**
* Name of this sprite.
*/
    public string name;

/**
* Indicates if this sprite is static or not.
*/
    public bool is_static;

/**
* Indicates if passive movement is denied for this sprite.
*/
    public bool only_active;

/**
* Indicates if this sprite is the avatar (player) of the game.
*/
    public bool is_avatar;

/**
* Indicates if the sprite has a stochastic behaviour.
*/
    public bool is_stochastic;

/**
* Color of this sprite.
*/
    public Color color;

/**
* States the pause ticks in-between two moves
*/
    public int cooldown;

/**
* Scalar speed of this sprite.
*/
    public float speed;

/**
* identifies whether this sprite should move (if an Avatar)
*/
    public bool stationary;

/**
* Mass of this sprite (for Continuous physics).
*/
    public float mass;

/**
* Id of the type if physics this sprite responds to.
*/
    public string physicstype;


/**
* Reference to the physics object this sprite belongs to.
*/
    public VGDLPhysics physics;

/**
* The amount of gravity force that pushes down on the sprite
*/
    public float gravity;

/**
* The amount of friction this sprite has when moving.
*/
    public float friction;

/**
* Scale factor to draw this sprite.
*/
    public float shrinkfactor;

/**
* Indicates if this sprite has an oriented behaviour.
*/
    public bool is_oriented;

/**
* Tells if an arrow must be drawn to indicate the orientation of the sprite.
*/
    public bool draw_arrow;

/**
* Orientation of the sprite.
*/
    public Vector2 orientation;

/**
* Rectangle that this sprite occupies on the screen.
*/
    public Rect rect;

/**
* Rectangle occupied for this sprite in the previous game step.
*/
    public Rect lastrect;

/**
* Tells how many timesteps ago was the last move
*/
    public int lastmove;

/**
* Strength measure of this sprite.
*/
    public float jump_strength;

/**
* Indicates if this sprite is a singleton.
*/
    public bool singleton;

/**
* Indicates if this sprite is a resource.
*/
    public bool is_resource;

/**
* Indicates if this sprite is a portal.
*/
    public bool portal;

/**
* Indicates if the sprite is invisible. If it is, the effect is that
* it is not drawn.
*/
    public string invisible;

/**
* If true, this sprite is never present in the observations passed to the controller.
*/
    public string hidden;

/**
* Indicates if the tile support autotiling
*/
    public bool autotiling;

/**
* Indicates if the tile picking is random
*/
    public float randomtiling;

/**
* max frameRate for animating sprites
*/
    public float frameRate;

/**
* remaining frame speed
*/
    public float frameRemaining;

/**
* the current frame to be drawn
*/
    public int currentFrame;

/**
* If true, this sprite's functionality is disabled.
* Disabled sprites are not drawn.
* Information about disabled sprites can still be accessed.
*/
    private bool disabled;

/**
* List of types this sprite belongs to. It contains the ids, including itself's, from this sprite up
* in the hierarchy of sprites defined in SpriteSet in the game definition.
*/
public List<string> types = new List<string>();

/**
* Indicates the amount of resources this sprite has, for each type defined as its key identifier.
*/
public Dictionary<string, int> resources = new Dictionary<string, int>();

    
/**
* All images in case there's orientation changes and/or animations.
*/
public Dictionary<string, List<Sprite>> images = new Dictionary<string, List<Sprite>>();

/**
* Unique and current image of this sprite.
*/
    public Sprite image;
    public Texture2D texture;

/**
* String that represents the image in VGDL.
*/
    public string img;

/**
* String that represents the image in VGDL.
*/
    public string orientedImg;

/**
* Indicates if this sprite is an NPC.
*/
    public bool is_npc;

/**
* ID of this sprite.
*/
    public int spriteID;

/**
* Indicates if this sprite was created by the avatar.
*/
    public bool is_from_avatar;

/**
* Bucket
*/
    public int bucket;

/**
* Bucket remainder.
*/
    public bool bucketSharp;

/**
* Indicates if the sprite is able to rotate in place.
*/
    public bool rotateInPlace;

/**
* Indicates if the sprite is in its first cycle of existence.
* Passive movement is not allowed in the first tick.
*/
    public bool isFirstTick;

/**
* Health points of this sprite. It does not automatically kill the sprite
* when it gets to 0 (an effect must do that, like 'SubtractHealthPoints').
* Its default value is set to 0, if not set specifically in VGDL.
*/
    public int healthPoints;


/**
* Maximum health points of this sprite.
* If not set specifically in VGDL, the default value is set to the healthPoints value set.
* This is NOT the maximum possible amount of points, it's the max. ever had.
*/
    public int maxHealthPoints;


/**
* Limit of health points of this can have.
* If not set specifically in VGDL, the default value is set to a very high value (1000)
*/
    public int limitHealthPoints;

/**
* Time to live for this sprite. Default set to -1 will not affect the sprite.
* If ttl > -1, when it gets to 0, the sprite gets killed.
*/
    public int timeToLive = -1;

/**
* The sprites rotation
*/
    public float rotation;

/**
* Multipliers for sprite's rectangle size
*/
    public float wMult = 1.0f;
    public float hMult = 1.0f;

/**
* The sprites size
*/
    public Vector2 size;

/**
* Is the sprite on ground?
*/
    public bool on_ground;

/**
* Is this a SOLID sprite? Meaning, can sprites stand on top of them, triggering the on_ground check.
*/
    public bool solid;

/**
* Maximum speed of the sprites
*/
    public float max_speed;

    public VGDLSprite()
    {
        physicstype = "GRID";
        wMult = hMult = 1.0f;
        physics = null;
        gravity = 0.0f;
        friction = 0.0f;
        speed = 0;
        stationary = false;
        cooldown = 0;
        color = VGDLColors.GetRandomColor();
        only_active = false;
        name = null;
        is_static = false;
        is_avatar = false;
        is_stochastic = false;
        is_from_avatar = false;
        mass = 1;
        shrinkfactor = 1.0f;
        autotiling = false;
        randomtiling = -1;
        frameRate = -1;
        frameRemaining = 0;
        currentFrame = -1;
        is_oriented = false;
        draw_arrow = false;
        orientation = VGDLUtils.VGDLDirections.NONE.getDirection();
        lastmove = 0;
        invisible = "false";
        rotateInPlace = false;
        isFirstTick = true;
        disabled = false;
        limitHealthPoints = 1000;
        
        rotation = 0.0f;
        max_speed = -1.0f;
    }

    public VGDLSprite(VGDLSprite from)
    {
        name = from.name;
        is_static = from.is_static;
        only_active = from.only_active;
        is_avatar = from.is_avatar;
        is_stochastic = from.is_stochastic;
        color = from.color;
        cooldown = from.cooldown;
        speed = from.speed;
        stationary = from.stationary;
        mass = from.mass;
        physicstype = from.physicstype;
        physics = from.physics;
        gravity = from.gravity;
        friction = from.friction;
        shrinkfactor = from.shrinkfactor;
        is_oriented = from.is_oriented;
        draw_arrow = from.draw_arrow;
        orientation = from.orientation;
        rect = new Rect(from.rect);
        lastrect = new Rect(from.lastrect);
        lastmove = from.lastmove;
        jump_strength = from.jump_strength;
        singleton = from.singleton;
        is_resource = from.is_resource;
        portal = from.portal;
        invisible = from.invisible;
        hidden = from.hidden;
        autotiling = from.autotiling;
        randomtiling = from.randomtiling;
        frameRate = from.frameRate;
        frameRemaining = from.frameRemaining;
        currentFrame = from.currentFrame;
        disabled = from.disabled;
        image = from.image;
        texture = from.texture;
        img = from.img;
        orientedImg = from.orientedImg;
        is_npc = from.is_npc;
        spriteID = from.spriteID;
        is_from_avatar = from.is_from_avatar;
        bucket = from.bucket;
        bucketSharp = from.bucketSharp;
        rotateInPlace = from.rotateInPlace;
        isFirstTick = from.isFirstTick;
        healthPoints = from.healthPoints;
        maxHealthPoints = from.maxHealthPoints;
        limitHealthPoints = from.limitHealthPoints;
        rotation = from.rotation;
        size = from.size;
        on_ground = from.on_ground;
        solid = from.solid;
        max_speed = from.max_speed;
        types.AddRange(from.types);
        foreach (var pair in from.resources)
        {
            resources.Add(pair.Key, pair.Value);
        }
        foreach (var pair in from.images)
        {
            images.Add(pair.Key, pair.Value);
        }
    }
    
    public virtual void init(Vector2 position, Vector2 size)
    {
        rect = new Rect(position, size);
        lastrect = new Rect(rect);
        
        this.size = size;
        determinePhysics();
        
        loadImage();

        if(!(orientation.Equals(Vector2.zero)))
        {
            //Any sprite that receives an orientation, is oriented.
            is_oriented = true;
        }

        if (maxHealthPoints == 0)
        {
            maxHealthPoints = healthPoints;
        }

        if (healthPoints > maxHealthPoints)
        {
            healthPoints = maxHealthPoints;
        }


        rect = new Rect(rect.x, rect.y, (int)(rect.width*wMult), (int)(rect.height*hMult));

        //Safety checks:
        if (cooldown < 1)
        {
            cooldown = 1; //Minimum possible value.
        }
    }

    public void loadImage()
    {   
        var imgPath = string.IsNullOrEmpty(img) ? orientedImg : img;

        var isOrientedImg = !string.IsNullOrEmpty(orientedImg);
        
        //If no image is found, don't try and load one.
        if (string.IsNullOrEmpty(imgPath)) return;

        if(images.Count == 0)
        {
            //There is autotiling (disabled now) or animations
            if (autotiling || randomtiling >= 0 || frameRate >= 0)
            {
                if (imgPath.Contains(".png"))
                {
                    imgPath = imgPath.Substring(0, imgPath.Length - 4);
                }

                var imagePathBase = imgPath + "_";

                //Get all the images for each orientation
                if (isOrientedImg)
                {
                    foreach (var dir in VGDLUtils.BASEDIRS)
                    {
                        var strDir = VGDLUtils.DirectionStringFromOrientation(dir);
                        var imagePath = imagePathBase + strDir + "_";
                        var theImages = getAnimatedImages(imagePath);
                        images[strDir] = theImages;
                    }
                }
                else
                {
                    var theImages = getAnimatedImages(imagePathBase);
                    images["NONE"] = theImages;
                }
            }
            else
            {
                //Get all the images for each orientation
                if(isOrientedImg)
                {
                    if (imgPath.Contains(".png"))
                    {
                        imgPath = imgPath.Substring(0, imgPath.Length - 4);
                    }
                    
                    var base_image_file = imgPath;

                    foreach(var dir in VGDLUtils.BASEDIRS) {
                        var strDir = VGDLUtils.DirectionStringFromOrientation(dir);
                        var theImages = new List<Sprite>();
                        var image_file = base_image_file + "_" + strDir;
                        var onlyImage = getImage(image_file);
                        theImages.Add(onlyImage);

                        images[strDir] = theImages;
                        image = theImages[0];
                    }
                }
                else 
                {
                    //Only one image. images stays empty.
                    image = getImage(imgPath);
                }

            }

        }
    }
    
    private Sprite getImage(string imgPath)
    {
        const float pixelsPerUnit = 24f; //All of the VGDL images are 24x24 pixels
        
        try
        {
            return VGDLUtils.LoadNewSprite(imgPath, pixelsPerUnit, Vector2.up);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return null;
    }
    
    private List<Sprite> getAnimatedImages(string imagePath)
    {
        const float pixelsPerUnit = 24f; //All of the VGDL images are 24x24 pixels
        
        var theImages = new List<Sprite>();
        try
        {
            var noMoreFiles = false;
            var i = 0;

            do
            {
                var currentFile = imagePath + i;
                if (!VGDLUtils.CheckSpriteExists(currentFile))
                {
                    noMoreFiles = true;
                }
                else
                {
                    var sprite = VGDLUtils.LoadNewSprite(currentFile, pixelsPerUnit, Vector2.up);    
                    theImages.Add(sprite);
                }

                i += 1;
            } while (!noMoreFiles);

            if (theImages.Count == 0)
            {
                Debug.LogWarning("Image not found: "+imagePath);
            }
            else
            {
                image = theImages[0]; //Default. 
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        
        return theImages;
    }
    
    
    /**
   * Updates the orientation of the avatar to match the orientation parameter.
   * @param orientation final orientation the avatar must have.
   * @return true if orientation could be changed. This returns false in two circumstances:
   * the avatar is not oriented (is_oriented == false) or the previous orientation is the
   * same as the one received by parameter.
   */
    public bool updateOrientation(Vector2 orientation)
    {
        if(!is_oriented) return false;
        if(this.orientation.Equals(orientation)) return false;
        this.orientation = orientation;
        return true;
    }
    
    /**
     * 
     * @param rot the rotation of the sprite
     * @return true if rotation could be changed
     */
    public bool updateRotation(float rot)
    {
        rotation = rot;
        return true;
    }
    
    public void updateBucket()
    {
        bucket = (int)(rect.y / rect.height);
        bucketSharp = ((int)rect.y % (int)rect.height) == 0;
    }
    
    public bool updatePos(Vector2 spriteOrientation, float speed = 1)
    {
        if (speed == 0)
        {
            speed = (int) this.speed;
            if (speed == 0) return false;
        }
        
        if (cooldown <= lastmove && (Mathf.Abs(spriteOrientation.x) + Mathf.Abs(spriteOrientation.y) > 0))
        {
            rect.position = new Vector2((int)rect.position.x + spriteOrientation.x * speed, (int)rect.position.y + spriteOrientation.y * speed);
            updateBucket();
            lastmove = 0;
            return true;
        }

        return false;
    }

    public void setDisabled(bool is_disabled)
    {
        disabled = is_disabled;
    }

    /**
   * Modifies the amount of resource by a given quantity.
   * @param resourceId id of the resource whose quantity must be changed.
   * @param amount_delta amount of units the resource has to be modified by.
   */
    public void modifyResource(string resourceId, int amount_delta)
    {
        int prev = getAmountResource(resourceId);
        int next = Mathf.Max(0, prev + amount_delta);
        resources[resourceId] = next;
    }

    public void subtractResource(string resourceId, int amount_delta)
    {
        var prev = getAmountResource(resourceId);
        var next = Mathf.Max(0, prev - amount_delta);
        resources[resourceId] = next;
    }

    /**
     * Removes all resources collected of the specified type.
     * @param resourceId - id of the resource whose quantity must be changed.
     */
    public void removeResource(string resourceId) {
        resources[resourceId] = 0;
    }

    /**
    * Returns the amount of resource of a given type this sprite has.
    * @param resourceId id of the resource to check.
    * @return how much of this resource this sprite has.
    */
    public int getAmountResource(string resource)
    {
        return resources.ContainsKey(resource) ? resources[resource] : 0;
    }
    
    public bool is_disabled()
    {
        return disabled;
    }

    public virtual void preMovement()
    {
        lastrect = new Rect(rect);
        lastmove += 1;

        frameRemaining -= 1;

        if (images.Count > 0)
        {
            List<Sprite> allImages;
            var isOrientedImg = !string.IsNullOrEmpty(orientedImg);
            if (!isOrientedImg)
            {
                allImages = images["NONE"];
            }
            else
            {
                allImages = images[VGDLUtils.DirectionStringFromOrientation(orientation)];
            }

            if (allImages.Count > 0)
            {
                if (frameRate > 0 && frameRemaining <= 0)
                {
                    if (allImages.Count > 0)
                    {
                        currentFrame = (currentFrame + 1) % allImages.Count;
                        frameRemaining = frameRate;
                        image = allImages[currentFrame];
                    }

                }
                else if (!autotiling)
                {
                    image = allImages[0];
                }
            }
        }
    }
    
    /**
     * Updates this sprite applying the passive movement.
     */
    public virtual void updatePassive() {

        if (!is_static && !only_active) {
            physics.passiveMovement(this);
        }
    }

    /**
     * Updates this sprite, performing the movements and actions for the next step.
     * @param game the current game that is being played.
     */
    public virtual void update(VGDLGame game)
    {
        updatePassive();
        if (timeToLive > -1) {
            if (timeToLive > 0) timeToLive--;
            else game.killSprite(this,false);
        }
    }

    public string getType()
    {
        return types.Last();
    }
    
    
    
    /**
    * Determines the physics type of the game, creating the VGDLPhysics objects that performs the calculations.
    * @param physicstype identifier of the physics type.
    * @param size dimensions of the sprite.
    * @return the vgdlphyics object.
    */
    private VGDLPhysics determinePhysics() {

        if (physicstype.CompareAndIgnoreCase(VGDLPhysics.GRID))
        {
            physics = new GridPhysics(size);
        }
        else if (physicstype.CompareAndIgnoreCase(VGDLPhysics.CONT))
        {
            physics = new ContinuousPhysics();
        }
        
        return physics;
    }


    public virtual bool intersects(VGDLSprite s2)
    {
        return rect.Overlaps(s2.rect);
    }
    
    /**
     * Overwritting intersects to check if we are on ground.
     * @return true if it directly intersects with sp (as in the normal case), but additionally checks for on_ground condition.
     */
    public bool groundIntersects (VGDLSprite sp)
    {
        var normalIntersect = rect.Overlaps(sp.rect);

        var otherHigher = sp.lastrect.yMin > (lastrect.yMin+(rect.height/2));
        var goingDown = rect.yMin > lastrect.yMin;

        if(!on_ground && sp.solid)
        {
            //No need to keep checking. Actually, we shouldn't (we won't intersect with all sprites!).
            var test_rect = new Rect(rect);
            test_rect.position = new Vector2(rect.x,rect.y+3);

            on_ground = test_rect.Overlaps(sp.rect) && otherHigher && goingDown;
        }

        return normalIntersect;
    }
    
    /**
     * Returns the last direction this sprite is following.
     * @return the direction.
     */
    public Vector2 lastDirection() {
        return new Vector2(rect.min.x - lastrect.min.x,
            rect.min.y - lastrect.min.y);
    }

    public Vector2 getPosition()
    {
        return rect.position;
    }

    /**
     * Gets the last position of this sprite. Returns null if same as current position.
     * @return the position as a Vector2.
     */
    public Vector2 getLastPosition()
    {
        if (!lastrect.Overlaps(rect)) {
            return new Vector2(lastrect.x, lastrect.y);
        }
        return Vector2.negativeInfinity;
    }

    /**
     * Returns the velocity of the sprite, in a Vector2d object.
     * @return the velocity of the sprite
     */
    public Vector2 getVelocity()
    {
        if (speed == 0) {
            return Vector2.zero;
        }

        return new Vector2(orientation.x * speed, orientation.y * speed);
    }
}