using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using Random = UnityEngine.Random;

public static class VGDLUtils
{
    #region VGDL Direction

    /* VGDL 
    public static final Vector2d NIL = new Vector2d(-1, -1); => Vector2.one * int.MinValue;
    public static final Vector2d NONE = new Vector2d(0, 0);
    public static final Vector2d RIGHT = new Vector2d(1, 0);
    public static final Vector2d LEFT = new Vector2d(-1, 0);
    public static final Vector2d UP = new Vector2d(0, -1);
    public static final Vector2d DOWN = new Vector2d(0, 1);
    public static final Vector2d[] BASEDIRS = new Vector2d[]{UP, LEFT, DOWN, RIGHT};

    public static final Direction DNIL = new Direction(-1, -1); 
    public static final Direction DNONE = new Direction(0, 0);
    public static final Direction DRIGHT = new Direction(1, 0);
    public static final Direction DLEFT = new Direction(-1, 0);
    public static final Direction DUP = new Direction(0, -1);
    public static final Direction DDOWN = new Direction(0, 1);
    public static final Direction[] DBASEDIRS = new Direction[]{DUP, DLEFT, DDOWN, DRIGHT};
    
    NIL Changed to => Vector2.one * int.MinValue; allowing for DIAGONAL DIRECTIONS
     */

    public enum VGDLDirections
    {
        NONE,
        UP,
        DOWN,
        LEFT,
        RIGHT,
        NIL,
    }
    
    public static Vector2[] BASEDIRS = {Vector2.up, Vector2.left, Vector2.down, Vector2.right};

    public static Vector2 getDirection(this VGDLDirections dir)
    {
        switch (dir)
        {
            case VGDLDirections.NONE:
                return Vector2.zero;
            //NOTE: up and down are reversed, due to drawing bottom to top.
            case VGDLDirections.UP:
                return Vector2.down;
            case VGDLDirections.DOWN:
                return Vector2.up;
            case VGDLDirections.LEFT:
                return Vector2.left;
            case VGDLDirections.RIGHT:
                return Vector2.right;
            case VGDLDirections.NIL:
                return Vector2.negativeInfinity;
            default:
                throw new ArgumentOutOfRangeException("dir", dir, null);
        }
    }

    public static Vector2 ParseVGDLDirection(string direction)
    {
        if (direction.ContainsAndIgnoreCase(Enum.GetName(typeof(VGDLDirections), VGDLDirections.UP))) return VGDLDirections.UP.getDirection();
        
        if (direction.ContainsAndIgnoreCase(Enum.GetName(typeof(VGDLDirections), VGDLDirections.DOWN))) return VGDLDirections.DOWN.getDirection();
        
        if (direction.ContainsAndIgnoreCase(Enum.GetName(typeof(VGDLDirections), VGDLDirections.LEFT))) return VGDLDirections.LEFT.getDirection();
        
        if (direction.ContainsAndIgnoreCase(Enum.GetName(typeof(VGDLDirections), VGDLDirections.RIGHT))) return VGDLDirections.RIGHT.getDirection();
        
        if (direction.ContainsAndIgnoreCase(Enum.GetName(typeof(VGDLDirections), VGDLDirections.NONE))) return VGDLDirections.NONE.getDirection();
        
        if (direction.ContainsAndIgnoreCase(Enum.GetName(typeof(VGDLDirections), VGDLDirections.NIL))) return VGDLDirections.NIL.getDirection();
        
        Debug.LogError("Unable to parse direction ["+direction+"].");
        return Vector2.one * -1;
    }
    
    public static string DirectionStringFromOrientation(Vector2 vec)
    {
        var result = Enum.GetName(typeof(VGDLDirections), VGDLDirections.NONE);
        var dist = Vector2.Distance(vec, getDirection(VGDLDirections.NONE));

        var dist2 = Vector2.Distance(vec, getDirection(VGDLDirections.UP));
        if (dist > dist2)
        {
            dist = dist2;
            result = Enum.GetName(typeof(VGDLDirections), VGDLDirections.UP);
        }
        
        dist2 = Vector2.Distance(vec, getDirection(VGDLDirections.DOWN));
        if (dist > dist2)
        {
            dist = dist2;
            result = Enum.GetName(typeof(VGDLDirections), VGDLDirections.DOWN);
        }
        
        dist2 = Vector2.Distance(vec, getDirection(VGDLDirections.LEFT));
        if (dist > dist2)
        {
            dist = dist2;
            result = Enum.GetName(typeof(VGDLDirections), VGDLDirections.LEFT);
        }
        
        dist2 = Vector2.Distance(vec, getDirection(VGDLDirections.RIGHT));
        if (dist > dist2)
        {
            dist = dist2;
            result = Enum.GetName(typeof(VGDLDirections), VGDLDirections.RIGHT);
        }

        if (vec.Equals(getDirection(VGDLDirections.NIL)))
        {
            result = Enum.GetName(typeof(VGDLDirections), VGDLDirections.NIL);
        }

        return result;
    }
    
    

    #endregion

    #region VGDL SpriteCategory

    public enum VGDLSpriteCategory
    {
        TYPE_UNINITIALIZED = -1,
        TYPE_AVATAR = 0,
        TYPE_RESOURCE = 1,
        TYPE_PORTAL = 2,
        TYPE_NPC = 3,
        TYPE_STATIC = 4,
        TYPE_FROMAVATAR = 5,
        TYPE_MOVABLE = 6,    
    }
    
    
    #endregion
    
    #region VGDL GAMESTATES
    
    /**
     * This is an enum type that describes the potential states of the game
     */
    public enum VGDLGameStates{
        INIT_STATE, 
        ACT_STATE, 
        END_STATE, 
        ABORT_STATE, 
        CHOOSE_LEVEL
    }
    
    #endregion
    
    #region Vector Extensions

    public static Vector3 withX(this Vector3 vect, float x)
    {
        return new Vector3(x, vect.y, vect.z);
    }

    public static Vector3 withY(this Vector3 vect, float y)
    {
        return new Vector3(vect.x, y, vect.z);
    }

    public static Vector3 withZ(this Vector3 vect, float z)
    {
        return new Vector3(vect.x, vect.y, z);
    }

    public static bool othogonal(this Vector2 a, Vector2 b)
    {
        return Vector2.Dot(a, b) == 0;
    }

    public static Vector2 copy(this Vector2 vec)
    {
        return new Vector2(vec.x, vec.y);
    }

    #endregion

    #region List Queue extensions

    //NOTE: we use List<T> instead of Queue<T> in Astar, because we want to Remove specific elements.
    public static void Enqueue<T>(this List<T> list, T element)
    {
        //list?.Add(element); means if(list != null) list.Add(element);
        list?.Add(element);
    }

    public static T Dequeue<T>(this List<T> list)
    {
        if (list == null || list.Count == 0) return default(T);

        var element = list[0];
        list.RemoveAt(0);
        return element;
    }

    #endregion
    
    #region Random Extensions

    public static T RandomElement<T>(this ICollection<T> list)
    {
        if (list == null || list.Count == 0) return default(T);
        
        return list.ElementAt(Random.Range(0, list.Count));
    }
    
    /// <summary>
    /// Returns a random cardinal direction
    /// </summary>
    /// <returns></returns>
    public static Vector2 RandomCardinalDirection()
    {
        return BASEDIRS.RandomElement();
    }
    
    public static int RandomIndexByWeight(this float[] weights) {
        
        var totalWeight = weights.Sum();
        
        // The weight we are after...
        var itemWeightIndex =  Random.value * totalWeight;
        var currentWeightIndex = 0f;

        for (var index = 0; index < weights.Length; index++)
        {
            currentWeightIndex += weights[index];

            // If we've passed the weight we are after for this item then it's the one we want....
            if (currentWeightIndex > 0 && currentWeightIndex >= itemWeightIndex)
                return index;
        }

        //Default if we fail to find one...(i.e. if all weights are zero)
        return -1;
    }


    
    #endregion
    
    #region Rect Extensions

    public static Rect intersection(this Rect rect1, Rect rect2)
    {
        var intersectionRect = Rect.zero;

        var leftX   = Mathf.Max( rect1.x, rect2.x );
        var rightX  = Mathf.Min( rect1.x + rect1.width, rect2.x + rect2.width );
        var topY    = Mathf.Max( rect1.y, rect2.y );
        var bottomY = Mathf.Min( rect1.y + rect1.height, rect2.y + rect2.height );

        if ( leftX < rightX && topY < bottomY ) {
            intersectionRect = new Rect( leftX, topY, rightX-leftX, bottomY-topY );
        } else {
            // Rectangles do not overlap, or overlap has an area of zero (edge/corner overlap)
        }

        return intersectionRect;
    }
    
    /**
     * Translates this {@code Rectangle} the indicated distance,
     * to the right along the X coordinate axis, and
     * downward along the Y coordinate axis.
     * @param dx the distance to move this {@code Rectangle}
     *                 along the X axis
     * @param dy the distance to move this {@code Rectangle}
     *                 along the Y axis
     * @see       java.awt.Rectangle#setLocation(int, int)
     * @see       java.awt.Rectangle#setLocation(java.awt.Point)
     */
    public static Rect translate(this Rect rect, float dx, float dy) {
        var oldv = rect.x;
        var newv = oldv + dx;
        if (dx < 0) {
            // moving leftward
            if (newv > oldv) {
                // negative overflow
                // Only adjust width if it was valid (>= 0).
                if (rect.width >= 0) {
                    // The right edge is now conceptually at
                    // newv+width, but we may move newv to prevent
                    // overflow.  But we want the right edge to
                    // remain at its new location in spite of the
                    // clipping.  Think of the following adjustment
                    // conceptually the same as:
                    // width += newv; newv = MIN_VALUE; width -= newv;
                    rect.width += newv - float.MinValue;
                    // width may go negative if the right edge went past
                    // MIN_VALUE, but it cannot overflow since it cannot
                    // have moved more than MIN_VALUE and any non-negative
                    // number + MIN_VALUE does not overflow.
                }
                newv = float.MinValue;
            }
        } else {
            // moving rightward (or staying still)
            if (newv < oldv) {
                // positive overflow
                if (rect.width >= 0) {
                    // Conceptually the same as:
                    // width += newv; newv = MAX_VALUE; width -= newv;
                    rect.width += newv - float.MaxValue;
                    // With large widths and large displacements
                    // we may overflow so we need to check it.
                    if (rect.width < 0) rect.width = float.MaxValue;
                }
                newv = float.MaxValue;
            }
        }
        rect.x = newv;

        oldv = rect.y;
        newv = oldv + dy;
        if (dy < 0) {
            // moving upward
            if (newv > oldv) {
                // negative overflow
                if (rect.height >= 0) {
                    rect.height += newv - float.MinValue;
                    // See above comment about no overflow in this case
                }
                newv = float.MinValue;
            }
        } else {
            // moving downward (or staying still)
            if (newv < oldv) {
                // positive overflow
                if (rect.height >= 0) {
                    rect.height += newv - float.MaxValue;
                    if (rect.height < 0) rect.height = float.MaxValue;
                }
                newv = float.MaxValue;
            }
        }
        rect.y = newv;

        return rect;
    }
    
    #endregion
    
    
    #region String Operations

    /// <summary>
    /// Compares current string to input, while ignoring the case.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="other"></param>
    /// <returns>true if string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0, false otherwise</returns>
    public static bool CompareAndIgnoreCase(this string str, string other)
    {
        return string.Compare(str, other, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public static bool ContainsAndIgnoreCase(this string str, string other)
    {
        return str.IndexOf(other, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    #endregion


    #region Sprite Loading

    private static Dictionary<string, Sprite[]> sprites;
    public static Sprite[] LoadAllSprites(string path)
    {
        if (sprites == null)
        {
            sprites = new Dictionary<string, Sprite[]>();
        }
        sprites.Add(path, Resources.LoadAll<Sprite>("vgdl/sprites/"+path));

        return sprites[path];
    }

    public static bool CheckSpriteExists(string filepath)
    {
        var splt = filepath.Split('/');
        if (splt.Length > 1)
        {
            if (sprites == null || !sprites.ContainsKey(splt[0]))
            {
                var allSpritesFromPath = LoadAllSprites(splt[0]);
                var actualList = allSpritesFromPath.Where(item => item.name.ContainsAndIgnoreCase(splt[1]));

                return actualList.Any();
            }
            else
            {
                var actualList = sprites[splt[0]].Where(item => item.name.ContainsAndIgnoreCase(splt[1]));
                return actualList.Any();
            }
        }

        return false;
    }
    
    /*
     * IMG2Sprite by user c68
     * https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
     */
    public static Sprite LoadNewSprite(string filepath, float PixelsPerUnit = 100.0f, Vector2 pivot = default(Vector2))
    {
        var splt = filepath.Split('/');
        if (splt.Length > 1)
        {
            if (sprites == null || !sprites.ContainsKey(splt[0]))
            {
                var allSpritesFromPath = LoadAllSprites(splt[0]);
                var actualList = allSpritesFromPath.Where(item => item.name.ContainsAndIgnoreCase(splt[1]));

                if (!actualList.Any())
                {
                    Debug.LogWarning("Sprite image file not found: " + filepath);
                    return null;
                }
                //NOTE: not handling multiple sprites yet.
                return actualList.First();
            }
            else
            {
                var actualList = sprites[splt[0]].Where(item => item.name.ContainsAndIgnoreCase(splt[1]));
                
                if (!actualList.Any())
                {
                    Debug.LogWarning("Sprite image file not found: " + filepath);
                    return null;
                }
                
                //NOTE: not handling multiple sprites yet.
                return actualList.First();
            }
        }

        //NOTE: old fallback logic, that could be used to load files from a custom location.
        
        //Check file Exists, otherwise try adding .png or .jpg
        if (!File.Exists(filepath))
        {
            if (File.Exists(filepath + ".png"))
            {
                filepath += ".png";
            }
            else if (File.Exists(filepath + ".jpg"))
            {
                filepath += ".jpg";
            }
            else
            {
                Debug.LogWarning("Sprite image file not found: " + filepath +
                                 " (also tested with .jpg/.png extension)");
                return null;
            }
        }

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        var spriteTexture = LoadTexture(filepath);
        if (spriteTexture == null)
        {
            Debug.LogWarning("Failed to create texture from: " + filepath);
            return null;
        }

        var newSprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), pivot,
            PixelsPerUnit);

        return newSprite;
    }

    public static Texture2D LoadTexture(string FilePath)
    {
        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        if (!File.Exists(FilePath)) return null; // Return null if file doesn't exist

        var fileData = File.ReadAllBytes(FilePath);
        var tex2D = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
        tex2D.filterMode = FilterMode.Point;
        if (tex2D.LoadImage(fileData)) // Load the imagedata into the texture (size is set automatically)
            return tex2D; // If data = readable -> return texture

        return null; // Return null if load failed
    }

    #endregion
    
    #region Dictionary extensions
    
    public static IEnumerable<V> GetValues<K, V>(this IDictionary<K, V> dict, IEnumerable<K> keys)
    {
        return keys.Select((x) => dict[x]);
    }

    public static Dictionary<K, V> GetSubDictionaryWhereKey<K, V>(this IDictionary<K, V> source, Func<K, bool> predicate)
    {
        return source.Where(s => predicate(s.Key)).ToDictionary(dict => dict.Key, dict => dict.Value);
    }
    
    public static Dictionary<K, V> GetSubDictionaryWhereValue<K, V>(this IDictionary<K, V> source, Func<V, bool> predicate)
    {
        return source.Where(s => predicate(s.Value)).ToDictionary(dict => dict.Key, dict => dict.Value);
    }
    
    public static Dictionary<K, V> GetSubDictionaryWhere<K, V>(this IDictionary<K, V> source, Func<KeyValuePair<K,V>, bool> predicate)
    {
        return source.Where(predicate).ToDictionary(dict => dict.Key, dict => dict.Value);
    }
    
    #endregion
  
}