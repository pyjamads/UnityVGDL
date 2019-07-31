/**
 * Created by diego on 24/03/14.
 */

using System;
using UnityEngine;

public class RecordedEvent : IComparable<RecordedEvent>
{
    /**
     * Game step when the event happened.
     */
    public int gameStep;

    /**
     * True if the event is caused by a sprite coming from (or created by) the avatar.
     * False if it's the avatar itself who collides with another sprite.
     */
    public bool fromAvatar;

    /**
     * Type id of the object that triggers the event (either the avatar,
     * or something created by the avatar).
     */
    public string activeType;

    /**
     * Type id of the object that received the event (what did the avatar,
     * or something created by the avatar, collided with?).
     */
    public string passiveType;

    /**
     * Sprite ID of the object that triggers the event (either the avatar,
     * or something created by the avatar).
     */
    public int activeSpriteID;

    /**
     * Sprite ID of the object that received the event (what did the avatar,
     * or something created by the avatar, collided with?).
     */
    public int passiveSpriteID;


    /**
     * Position where the event took place.
     */
    public Vector2 position;

    /**
     * Constructor
     * @param gameStep when the event happened.
     * @param fromAvatar did the avatar trigger the event (false), or something created by him (true)?
     * @param activeTypeId type of the sprite (avatar or from avatar).
     * @param passiveTypeId type of the sprite that collided with activeTypeId.
     * @param activeSpriteId sprite ID of the avatar (or something created by the avatar).
     * @param passiveSpriteId sprite ID of the other object.
     * @param position where did the event take place.
     */
    public RecordedEvent(int gameStep, bool fromAvatar, string activeType, string passiveType,
                 int activeSpriteId, int passiveSpriteId, Vector2 position)
    {
        this.gameStep = gameStep;
        this.fromAvatar = fromAvatar;
        this.activeType = activeType;
        this.passiveType = passiveType;
        this.activeSpriteID = activeSpriteId;
        this.passiveSpriteID = passiveSpriteId;
        this.position = position;
    }

    /**
     * Creates a copy of this event.
     * @return the copy.
     */
    public RecordedEvent copy()
    {
        return new RecordedEvent(gameStep, fromAvatar, activeType, passiveType, activeSpriteID, passiveSpriteID, position);
    }
 
    public int CompareTo(RecordedEvent o) {
        if(this.gameStep < o.gameStep)       return -1;   //First tie break: gameStep.
        if(this.gameStep > o.gameStep)       return 1;
        if(this.fromAvatar && !o.fromAvatar) return -1;   //Second tie break: who triggered.
        if(!this.fromAvatar && o.fromAvatar) return 1;
        if(this.passiveType.CompareTo(o.passiveType) < 0)     return -1;   //Third tie break: against what.
        if(this.passiveType.CompareTo(o.passiveType) > 0)     return 1;
        if(this.activeType.CompareTo(o.activeType) < 0)       return -1;   //Fourth tie break: who triggered it
        if(this.activeType.CompareTo(o.activeType) > 0)       return 1;
        return 0;
    }

    public override bool Equals(object o)
    {
        if(this == o) return true;
        if(!(o is RecordedEvent)) return false;
        var other = (RecordedEvent)o;

        if(this.gameStep != other.gameStep) return false;
        if(this.fromAvatar != other.fromAvatar) return false;
        if(this.activeType != other.activeType) return false;
        if(this.passiveType != other.passiveType) return false;
        if(this.activeSpriteID != other.activeSpriteID) return false;
        if(this.passiveSpriteID != other.passiveSpriteID) return false;
        if(! this.position.Equals(other.position)) return false;
        return true;
    }

    public override int GetHashCode()
    {
     unchecked
     {
      var hashCode = gameStep;
      hashCode = (hashCode * 397) ^ fromAvatar.GetHashCode();
      hashCode = (hashCode * 397) ^ activeType.GetHashCode();
      hashCode = (hashCode * 397) ^ passiveType.GetHashCode();
      hashCode = (hashCode * 397) ^ activeSpriteID;
      hashCode = (hashCode * 397) ^ passiveSpriteID;
      hashCode = (hashCode * 397) ^ position.GetHashCode();
      return hashCode;
     }
    }
}