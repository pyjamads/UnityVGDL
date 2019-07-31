/**
 * Created by Diego on 19/03/14.
 */

using System;
using UnityEngine;
using Object = System.Object;

public class Observation : IComparable<Observation>
{

    /**
     * Category of this observation (static, resource, npc, etc.).
     */
    public VGDLUtils.VGDLSpriteCategory category;

    /**
     * Type of sprite of this observation.
     */
    public string stype;

    /**
     * unique ID for this observation
     */
    public int obsID;

    /**
     * Position of the observation.
     */
    public Vector2 position;

    /**
     * Reference to the position used for comparing this
     * observation with others.
     */
    public Vector2 reference;

    /**
     * Distance from this observation to the reference.
     */
    public float sqDist;

    public Observation() {
        // used for learning track
        category = VGDLUtils.VGDLSpriteCategory.TYPE_UNINITIALIZED;
        stype = "";
        obsID = -1;
        position = new Vector2();
        reference = Vector2.negativeInfinity;
        sqDist = -1;
    }

    /**
     * New observation. It is the observation of a sprite, recording its ID and position.
     * @param itype type of the sprite of this observation
     * @param id ID of the observation.
     * @param pos position of the sprite.
     * @param posReference reference to compare this position to others.
     * @param category category of this observation (NPC, static, resource, etc.)
     */
    public Observation(string stype, int id, Vector2 pos, Vector2 posReference, VGDLUtils.VGDLSpriteCategory category)
    {
        this.stype = stype;
        this.obsID = id;
        this.position = pos;
        this.reference = posReference;
        sqDist = (pos - posReference).sqrMagnitude;
        this.category = category;
    }

    /**
     * Updates this observation
     * @param itype type of the sprite of this observation
     * @param id ID of the observation.
     * @param pos position of the sprite.
     * @param posReference reference to compare this position to others.
     * @param category category of this observation (NPC, static, resource, etc.)
     */
    public void update(string stype, int id, Vector2 pos, Vector2 posReference, VGDLUtils.VGDLSpriteCategory category)
    {
        this.stype = stype;
        this.obsID = id;
        this.position = pos;
        this.reference = posReference;
        sqDist = (pos - posReference).sqrMagnitude;
        this.category = category;
    }

    /**
     * Compares this observation to others, using distances to the reference position.
     * @param o other observation.
     * @return -1 if this precedes o, 1 if same distance or o is closer to reference.
     */
    public int CompareTo(Observation o) {
        double oSqDist = (o.position - reference).sqrMagnitude;
        if(sqDist < oSqDist)        return -1;
        else if(sqDist > oSqDist)   return 1;
        return 0;
    }

    /**
     * Compares two Observations to check if they are equal. The reference attribute is NOT
     * compared in this object.
     * @param other the other observation.
     * @return true if both objects are the same Observation.
     */
    public override bool Equals(Object other)
    {
        if(other == null || !(other is Observation))
            return false;

        var o = (Observation) other;
        if(!this.stype.Equals(o.stype)) return false;
        if(this.obsID != o.obsID) return false;
        if(!this.position.Equals(o.position)) return false;
        if(this.category != o.category) return false;
        return true;
    }

    public override String ToString() {
        return "Observation{" +
                "category=" + category +
                ", itype=" + stype +
                ", obsID=" + obsID +
                ", position=" + position +
                ", reference=" + reference +
                ", sqDist=" + sqDist +
                "}\n";
    }
}