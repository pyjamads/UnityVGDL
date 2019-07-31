using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BasicGame : VGDLGame
{
	public int square_size = -1;

	/**
	* List of sprites that should not be traversable for the pathfinder. This
	* list can be specified with sprite string identifiers separated by commas.
	* (NOTE: copied from GVGAI, should be changed to handle python style array: [sprite1, sprite2, ...])
	*/
	public string obs;
	
	// List of IDs of the sprites should not be traversable for the pathfinder.
	private List<string> obstacles;
	
	public const int MAX_WINDOW_SIZE = 800;
	
	public BasicGame()
	{
		//default mappings
		charMapping.Add('w', new []{"wall"});
		charMapping.Add('A', new []{"avatar"});
		
		//default order
		spriteOrder.Add("wall");
		spriteOrder.Add("avatar");
		
		// Default values for frame rate and maximum number of sprites allowed.
		square_size = -1;
		MAX_SPRITES = 10000;
	}

	public void buildLevelFromFile(string filename, int randomSeed = 0)
	{
		//Read text asset from resources
		var textAsset = Resources.Load<TextAsset>(filename);

		if (textAsset == null)
		{
			Debug.LogError("Failed to load level: "+filename);
			return;
		}

		buildLevelFromTextAsset(textAsset, randomSeed);
	}
	
	public void buildLevelFromTextAsset(TextAsset textAsset, int randomSeed = 0)
	{
		//Read lines
		var lines = textAsset.text.SplitLines().ToList();

		//Trim any excess empty lines at the start.
		while (string.IsNullOrEmpty(lines[0]))
		{
			lines.RemoveAt(0);
		}

		//Trim any excess empty lines at the end.
		while (string.IsNullOrEmpty(lines[lines.Count - 1]))
		{
			lines.RemoveAt(lines.Count - 1);
		}

		//NOTE: custom alignment from the LEFT only works if the designer
		//adds a random character instead of the first space on the line.
		for (var index = 0; index < lines.Count; index++)
		{
			//Handle indentations and excess whitespace at the end.
			lines[index] = lines[index].Trim();
		}

		//Build level
		buildLevelFromLines(lines.ToArray(), randomSeed);
	}
	
	public void buildLevelFromLines(string[] lines, int randomSeed = 0)
	{
		// Pathfinder
		obstacles = new List<string>();
		bool doPathf = false;

		if (obs != null) {
			doPathf = true;
			var obsArray = obs.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
			foreach (var observation in obsArray)
			{
				obstacles.Add(observation);	
			}
		}

		if (doPathf)
		{
			pathf = new PathFinder(obstacles);
		}
		
		//set height
		size.y = lines.Length;

		int maxWidth = 0;
		foreach (var line in lines)
		{
			if (line.Length > maxWidth)
			{
				maxWidth = line.Length;
			}
		}
		//set width
		size.x = maxWidth;

		if (square_size != -1)
		{
			//todo: fix the screen scaling when somebody overrides this
			block_size = square_size;
		}
		else
		{
			block_size = (int)Mathf.Max(2, MAX_WINDOW_SIZE / Mathf.Max(size.x, size.y));
		}
		
		screenSize = new Vector2(size.x * block_size, size.y * block_size);

		if (VGDLParser.verbose)
		{
			Debug.Log("Creating level with block size="+block_size+" screenWidth="+screenSize.x+" screenHeight="+screenSize.y);
		}
		
		//Pad lines with spaces at the end to match size.x (level width)
		for (int i = 0; i < lines.Length; i++)
		{
			if (lines[i].Length < size.x)
			{
				lines[i] = lines[i].PadRight((int)size.x);
			}
		}

		var avatars = new List<VGDLSprite>();
		//Create and add all sprites
		for (var i = 0; i < size.y; i++)
		{
			var line = lines[i];

			//For every line check every character.
			for (var j = 0; j < size.x; j++)
			{
				var c = line[j];

				//If the character is defined in the charMapping, create it.
				if (charMapping.ContainsKey(c))
				{
					//Add each sprited defined by the charMapping
					foreach (var obj in charMapping[c])
					{
						//this allows the tiling feature.
						var similarTiles = 0;
						for (var x = -1; x <= 1; x++)
						{
							for (var y = -1; y <= 1; y++)
							{
								if (Mathf.Abs(x) == Mathf.Abs(y) || j + x < 0 || !(j + x < size.x) || i + y < 0 ||
								    !(i + y < size.y)) continue;

								if (!charMapping.ContainsKey(lines[i + y][j + x])) continue;

								var neighborTiles = charMapping[lines[i + y][j + x]];
								if (neighborTiles.Contains(obj))
								{
									similarTiles += Mathf.FloorToInt(Mathf.Abs(x) * (x + 3) / 2f) +
									                Mathf.Abs(y) * (y + 3) * 2;
								}
							}
						}

						var position = new Vector2(j * block_size, i * block_size);
						var sprite = createAndAddSpriteAt(obj, position);

						//Sprite limit reached or sprite definition not found!
						if (sprite == null) continue;

						if (sprite.is_avatar)
						{
							avatars.Add(sprite);
						}

						//TODO: AutoTiling and randomTiling
						//Used for creating varied backgrounds etc.
						if (sprite.autotiling)
						{

							var images = sprite.images["NONE"];
							if (images.Count > 0)
							{
								//NOTE: added Mathf.Min(images.Count, .. to protect it from overflowing if similarTiles > images.Conut
								sprite.image = images[Mathf.Min(images.Count, similarTiles)];
							}
						}

						if (sprite.randomtiling >= 0)
						{
							//NOTE: resetting the random to the randomSeed for every iteration, kinda defeats the purpose of randomizing.
							//Random random = new Random(randomSeed);
							var allImages = sprite.images["NONE"];
							if (Random.value > sprite.randomtiling && allImages.Count > 0)
							{
								sprite.image = allImages[Random.Range(0, allImages.Count)];
							}
						}
					}
				}
				else if (!char.IsWhiteSpace(c))
				{
					Debug.LogWarning("Character ["+c+"] is not defined in the level mapping.");
				}
			}
		}

		if (avatars.Count != no_players)
		{
			Debug.LogWarning("Player count and avatar count mismatch");
		}
		
		//Reset kill list
		killList = new List<VGDLSprite>();

		//TODO: forward model and other ai tools
		createAvatars();
		initForwardModel();
		
		if (doPathf) {
			var t = DateTime.Now;

			pathf.run(getObservation());
			Debug.Log("PathFinding took: "+(DateTime.Now - t).TotalMilliseconds+"ms");
		}
	}
	
	/**
	 * Method to create the array of avatars from the sprites.
	 */

	/**
	 * Cleans the array of shielded effects.
	 */

	/**
	 * Checks if the game must finish because of number of cycles played. This
	 * is a value stored in CompetitionParameters.MAX_TIMESTEPS. If the game is
	 * due to end, the winner is determined and the flag isEnded is set to true.
	 */
//	protected void checkTimeOut() {
//		if (gameTick >= CompetitionParameters.MAX_TIMESTEPS) {
//			isEnded = true;
//			for (int i = 0; i < no_players; i++) {
//				if (avatars[i].getWinState() != Types.WINNER.PLAYER_WINS)
//					avatars[i].setWinState(Types.WINNER.PLAYER_LOSES);
//			}
//		}
//	}

	/**
	 * Checks if a given rectangle is at the edge of the screen.
	 *
	 * @param rect the rectangle to check
	 * @return true if rect is at the edge of the screen.
	 */


	/**
	 * Overloaded method for multi player games. Returns the win state of the
	 * specified player.
	 *
	 * @param playerID
	 *            ID of the player.
	 * @return the win state of the specified player.
	 */

	/**
	 * Method overloaded for multi player games. Returns the game score of the
	 * specified player.
	 *
	 * @param playerID
	 *            ID of the player.
	 */


	/**
	 * Returns an array of type Types.WINNER containing the win state of all
	 * players in the game. Index in the array corresponds to playerID.
	 *
	 * @return array of Types.WINNER values.
	 */

	/**
	 * Class for helping collision detection.
	 */
}

