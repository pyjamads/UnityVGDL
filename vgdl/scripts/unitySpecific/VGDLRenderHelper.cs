using System;
using UnityEngine;

public static class VGDLRenderHelper
{
    public static void RenderGameUsingDrawTexture(VGDLGame game, RenderTexture renderTarget, bool OnGUI = false)
	{
		var drawOrder = game.getSpriteOrder();

		RenderTexture prevRT = null;
		
		if (renderTarget != null)
		{
			prevRT = RenderTexture.active;
			Graphics.SetRenderTarget(renderTarget);
		}
		
		GL.PushMatrix(); //Saves both projection and modelview matrices to the matrix stack.
		GL.LoadPixelMatrix(0, game.screenSize.x, game.screenSize.y, 0); //Setup a matrix for pixel-correct rendering.
		
		//Clear the background before we render a new round of sprites.
		GL.Clear(true, true, Color.black);
		
		foreach (var stype in drawOrder)
		{	
			foreach (var vgdlSprite in game.getSprites(stype))
			{
				//NOTE: hidden should just be handled when making observations.
				if(vgdlSprite.invisible.CompareAndIgnoreCase("True")) continue; 
				   //|| vgdlSprite.hidden.CompareAndIgnoreCase("True")) continue;
				
				var show = false;
				var invis = vgdlSprite.invisible.Split(',');

				//NOTE: this is similar to the boolean.parseBoolean() logic in JAVA.
				bool invis0 = bool.TryParse(invis[0], out invis0) && invis0;

				if (game.no_players == 2)
				{
					bool invis1 = invis.Length > 1 ? bool.TryParse(invis[1], out invis1) && invis1 : invis0;

					//NOTE: Actually it seems game.humanPlayers is always false for all elements, in GVGAI
					//and humanID is never used, so the code has been refactored.
					/*
						boolean displayP1 = game.humanPlayer[0] && !invis0;
						boolean displayP2 = game.humanPlayer[1] && !invis1;
			
						if (game.humanPlayer[0] && game.humanPlayer[1] || !game.humanPlayer[0] && !game.humanPlayer[1]) {
							if (invis0 == invis1) show = !invis0;
							else if (color == Types.DARKGRAY) show = false;
							else show = !invis0 || !invis1;
						} else
							show = displayP1 || displayP2;
					 */
					
					if (invis0 == invis1) show = !invis0;
					else
					{
						//NOTE: this seems odd, an implicit invisibility feature.
						show = vgdlSprite.color != VGDLColors.DarkGray;
					}
				}
				else
				{
					show = !invis0;
				}

				if (!show || vgdlSprite.is_disabled()) continue;
				
				//var screenRect = ConvertToScreenRect(game, vgdlSprite.rect);
				var screenRect = new Rect(vgdlSprite.rect);
				
				if(vgdlSprite.shrinkfactor != 1f && vgdlSprite.shrinkfactor > 0f)
				{
					screenRect.width *= vgdlSprite.shrinkfactor;
					screenRect.height *= vgdlSprite.shrinkfactor;
					screenRect.x += (vgdlSprite.rect.width-screenRect.width)/2;
					screenRect.y += (vgdlSprite.rect.height-screenRect.height)/2;
				}
				
				if (vgdlSprite.is_avatar && vgdlSprite.is_oriented)
				{
					//TODO: Draw arrow, look at _drawOriented()
					
					GL.PushMatrix();
					GL.LoadPixelMatrix(0, game.screenSize.x, game.screenSize.y, 0); //Setup a matrix for pixel-correct rendering.

					//backup GUI matrix, before modifying.
					var matrixBackup  = GUI.matrix;
					
					if (OnGUI)
					{
						//Convert it to a Screen rect, then convert it to a GUI rect.
						screenRect = ConvertToScreenRect(game, screenRect);
						screenRect = GUIUtility.ScreenToGUIRect(screenRect);
						
						//GUIUtility.ScaleAroundPivot(Vector2.one, screenRect.center);
						GUIUtility.RotateAroundPivot(vgdlSprite.rotation * Mathf.Rad2Deg, screenRect.center);
						
						//NOTE: Don't think this is needed, but maybe adjust this if the sprite shows up behind other things.
						//GUI.depth=149;
					}
					else
					{
						GL.MultMatrix(Matrix4x4.TRS(screenRect.center, Quaternion.AngleAxis(vgdlSprite.rotation * Mathf.Rad2Deg, Vector3.forward), screenRect.size));	
					}

					
					if (!OnGUI)
					{
						//NOTE: Translation already applied by MatrixMult, but we need an offset of half the width/height.
						screenRect.x = -0.5f;
						screenRect.y = -0.5f;
						screenRect.width = 1;
						screenRect.height = 1;
					}


					if (vgdlSprite.image != null)
					{
						//TODO: rescale location based on renderTarget?
						Graphics.DrawTexture(screenRect, vgdlSprite.image.texture);
					}
					else
					{
						Graphics.DrawTexture(screenRect, Texture2D.whiteTexture, 
							Rect.MinMaxRect(0, 0, 1, 1), 0, 0, 
							0,0, vgdlSprite.color);
					}

					if (OnGUI)
					{
						GUI.matrix = matrixBackup;
					}
					
					GL.PopMatrix();
					
					//TODO: Draw Arrow if vgdlSprite.draw_arrow
				}
				else
				{
					if (vgdlSprite.image != null)
					{
						//TODO: rescale location based on renderTarget?
						Graphics.DrawTexture(screenRect, vgdlSprite.image.texture);
					}
					else
					{
						Graphics.DrawTexture(screenRect, Texture2D.whiteTexture, Rect.MinMaxRect(0,0,1,1),0,0,0,0,vgdlSprite.color);
					}
				}

				
				//TODO:Draw Health and Resources
				/*
				 * if(resources.size() > 0)
	            {
	                _drawResources(gphx, game, r);
	            }
	
	            if(healthPoints > 0)
	            {
	                _drawHealthBar(gphx, game, r);
	            }
				*/
			}
		}

		GL.PopMatrix(); //Restores both projection and modelview matrices off the top of the matrix stack.
		
		if (prevRT != null)
		{	
			RenderTexture.active = prevRT;	
		}
	}
    
    /**
     * In case this sprite is oriented and has an arrow to draw, it draws it.
     * @param g graphics device to draw in.
     */
//    private static void _drawOriented(VGDLSprite sprite)
//    {
	    //Color arrowColor = new Color(color.getRed(), 255-color.getGreen(), color.getBlue());
	    //Polygon p = Utils.triPoints(r, orientation);
	    
	    // Rotation information
//	    if(shrinkfactor != 1)
//	    {
//		    r.width *= shrinkfactor;
//		    r.height *= shrinkfactor;
//		    r.x += (rect.width-r.width)/2;
//		    r.y += (rect.height-r.height)/2;
//	    }
//
//	    int w = image.getWidth(null);
//	    int h = image.getHeight(null);
//	    float scale = (float)r.width/w; //assume all sprites are quadratic.
//
//	    AffineTransform trans = new AffineTransform();
//	    trans.translate(r.x, r.y);
//	    trans.scale(scale,scale);
//	    trans.rotate(rotation,w/2.0,h/2.0);
//	    // Uncomment this line to have only one sprite
//	    //g.drawImage(image, trans, null);
//
//	    /* Code added by Carlos*/
//	    g.drawImage(image, trans, null);
//	    /* End of code added by carlos*/
//
//	    // We only draw the arrow if the directional sprites are null
//	    if (draw_arrow) {
//		    g.setColor(arrowColor);
//		    g.drawPolygon(p);
//		    g.fillPolygon(p);
//	    }
	    
//		//TODO: drawing arrows and stuff.
//	    var mesh = new Mesh();
//	    //mesh.vertices = p.points or something like that 
//	    //mesh.normal = assign these
//	    //mesh.triangles = asigne these
//	    //Then do this:
//	    Graphics.DrawMeshNow(mesh, sprite.rect.position, Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.right, sprite.orientation), Vector3.forward));
//    }

//    
//     /**
//     * Draws the resources hold by this sprite, as an horizontal bar on top of the sprite.
//     * @param gphx graphics to draw in.
//     * @param game game being played at the moment.
//     */
//    protected void _drawResources(Graphics2D gphx, Game game, Rectangle r)
//    {
//        int numResources = resources.size();
//        double barheight = r.getHeight() / 3.5f / numResources;
//        double offset = r.getMinY() + 2*r.height / 3.0f;
//
//        Set<Map.Entry<Integer, Integer>> entries = resources.entrySet();
//        for(Map.Entry<Integer, Integer> entry : entries)
//        {
//            int resType = entry.getKey();
//            int resValue = entry.getValue();
//
//            if(resType > -1) {
//                double wiggle = r.width / 10.0f;
//                double prop = Math.max(0, Math.min(1, resValue / (double) (game.getResourceLimit(resType))));
//
//                Rectangle filled = new Rectangle((int) (r.x + wiggle / 2), (int) offset, (int) (prop * (r.width - wiggle)), (int) barheight);
//                Rectangle rest = new Rectangle((int) (r.x + wiggle / 2 + prop * (r.width - wiggle)), (int) offset, (int) ((1 - prop) * (r.width - wiggle)), (int) barheight);
//
//                gphx.setColor(game.getResourceColor(resType));
//                gphx.fillRect(filled.x, filled.y, filled.width, filled.height);
//                gphx.setColor(Types.BLACK);
//                gphx.fillRect(rest.x, rest.y, rest.width, rest.height);
//                offset += barheight;
//            }
//        }
//
//    }
//
//
//    /**
//     * Draws the health bar, as a vertical bar on top (and left) of the sprite.
//     * @param gphx graphics to draw in.
//     * @param game game being played at the moment.
//     * @param r rectangle of this sprite.
//     */
//    protected void _drawHealthBar(Graphics2D gphx, Game game, Rectangle r)
//    {
//        int maxHP = maxHealthPoints;
//        if(limitHealthPoints != 1000)
//            maxHP = limitHealthPoints;
//
//        double wiggleX = r.width * 0.1f;
//        double wiggleY = r.height * 0.1f;
//        double prop = Math.max(0,Math.min(1, healthPoints / (double) maxHP));
//
//        double barHeight = r.height-wiggleY;
//        int heightHealth = (int) (prop*barHeight);
//        int heightUnhealth = (int) ((1-prop)*barHeight);
//        int startY = (int) (r.getMinY()+wiggleY*0.5f);
//
//        int barWidth = (int) (r.width * 0.1f);
//        int xOffset = (int) (r.x+wiggleX * 0.5f);
//
//        Rectangle filled = new Rectangle(xOffset, startY + heightUnhealth, barWidth, heightHealth);
//        Rectangle rest   = new Rectangle(xOffset, startY, barWidth, heightUnhealth);
//
//        if (game.no_players > 1)
//            gphx.setColor(color);
//        else
//            gphx.setColor(Types.RED);
//        gphx.fillRect(filled.x, filled.y, filled.width, filled.height);
//        gphx.setColor(Types.BLACK);
//        gphx.fillRect(rest.x, rest.y, rest.width, rest.height);
//    }
	
	public static Rect ConvertToScreenRect(VGDLGame game, Rect spriteRect)
	{
		var screenWidth = Screen.width; //Camera.main.scaledPixelWidth;
		var screenHeight = Screen.height; //Camera.main.scaledPixelHeight;

		var gameWidth = Mathf.RoundToInt(game.screenSize.x);
		var gameHeight = Mathf.RoundToInt(game.screenSize.y);
		
		var pos = new Vector2((spriteRect.x / gameWidth) * screenWidth, (spriteRect.y / gameHeight) * screenHeight);
		var size = new Vector2(screenWidth / game.size.x, screenHeight / game.size.y); 
		
		var screenRect = new Rect(pos, size);

		return screenRect;
	}
}
