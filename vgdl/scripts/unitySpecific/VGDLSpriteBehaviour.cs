using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class made up of a VGDLSprite and its VGDLEffects
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class VGDLSpriteBehaviour : MonoBehaviour
{
    public int spriteID;
    public string spriteType; // Should probably be set as a Tag.
    public VGDLSprite sprite;
    public VGDLGame game;
    
    public Dictionary<string, VGDLEffect> effects = new Dictionary<string, VGDLEffect>();

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidbody2D;
    private BoxCollider2D boxCollider;
    
    void Start()
    {
        //TODO: we might want to tweak the transform size here, otherwise it should be done in the VGDLSceneGenerator.
        
        //Setup name and type
        gameObject.name = spriteID + "_" + spriteType;
        gameObject.tag = spriteType;
        
        //Get Components
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        //Setup renderer
        //TODO: set z-order based on SpriteOrder
        //Check if the image exists, if not, just draw a square of the sprite.color.
        if (sprite.image == null)
        {
            spriteRenderer.color = sprite.color;  
        }
        else
        {
            spriteRenderer.sprite = sprite.image;
        }

        //Setup Physics and Collider
        //TODO: physics and collider setup
        //sprite.physics
        //TODO: remove colliders and rigidbodies when not needed, for things like Immovable
        //TODO: setup correct collision layer
        
        
    }
    
    void Update()
    {
        //TODO: sprite.preMovement
        //TODO: sprite.update
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //Check if this type had to deal with the thing we collided with.
        if (effects.ContainsKey(other.gameObject.tag))
        {
            //TODO: figure out if this is gonna work.
            //Execute effect
            game.executeEffect(effects[other.gameObject.tag], other.gameObject.GetComponent<VGDLSpriteBehaviour>().sprite, sprite);
        }
    }
    
    
}