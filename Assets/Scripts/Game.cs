using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	class Block //block represents each member of the grid
	{
		GameObject tile;
		Transform pos;
		int x, y; //x and y positions
		bool hasAI = false;
		bool isOpen = true;
		public bool visited = false;

		public bool containsAI() { return hasAI; }

		public void SetXY(int p_x, int p_y) { x = p_x; y = p_y; }

		public void setTile(GameObject p_tile) { tile = p_tile; }

		public GameObject getTile() { return tile; }

		public void setOpen(bool val) { isOpen = val; }

		public void leave() { hasAI = false; }

		public void enter() { hasAI = true; }

		public void setPos(Transform p_pos) { pos = p_pos; }

		public Transform getPos() { return pos; }

		public bool getOpen() { return isOpen; }

		public Cords getXY() { return new Cords(x, y); }
	};

	private const int ROWS = 7; //number for rows
	private const int COLS = 7; //number for columns REMEMBER TO CHANGE THESE IF THE GRID LAYOUT CHANGES

	private Block[,] grid = new Block[ROWS, COLS];  //our grid for all of the logic in this game

	class Cords //Cords are a small struct to keep track of x,y values for pathing
	{
		public Cords(int p_x, int p_y, int p_score = -1) { x = p_x; y = p_y; score = p_score; }

		public Cords(Cords newCords) { x = newCords.x; y = newCords.y; score = newCords.score; }

		public int x;
		public int y;
		public int score = -1; //default to impassable
	}

	public GameObject tileParent; //gridLayout
	public GameObject AI; //the AI graphic
	public GameObject Finish; //the end point graphic
	public GameObject Player; //the player graphic

	private List<Cords> aiPath = new List<Cords>();
	private GameObject theAI; //instance for the AI
	private GameObject theGoalPoint; //instance for the goal

	private int interval = 1; //interval for animations (currently one second)
	private int nextTime = 0; //logic for making things timed
	private Vector3 StartPos, EndPos; //vectors to move the objects graphically
	private float t; //time for moving objects

	/* Start
	 * This is called at the beginning of the game.
	 * 
	 * Start will fill the grid array with the grid layout contents.
	 */
	void Start()
	{
		int i = 0; //index for our gridLayoutParent and it's parents
		int x, y; //x and y, used several times
		for (y = COLS-1; y >= 0; y--) //start at 6 because layout starts at top left, but we want the array to start
		{
			for (x = ROWS-1; x >= 0; x--)
			{
				grid[x, y] = new Block(); //allocate memory for the Block instance (error otherwise)
				grid[x, y].setTile(tileParent.transform.GetChild(i).gameObject); //set the tile to the corresponding girdLayout location
				if (grid[x, y].getTile().gameObject.tag == "NoPass") //if its tag is NoPass, it's a rock and it's not passable
				{
					grid[x, y].setOpen(false);
				}
				grid[x, y].setPos(grid[x, y].getTile().transform);
				grid[x, y].SetXY(x, y);
				i++; //increment our gridLayout index
			}
		}

		//***the below logic is all for the AI's spawning***
		do
		{
			x = Random.Range(0, ROWS-1);
			y = Random.Range(0, COLS-1);
		} while (!(grid[x, y].getOpen())); //keep trying until we get a spawnpoint that's open
		grid[x, y].enter(); //move AI to spawn
		aiPath.Add(new Cords(x, y, 0)); //AI path has start point
		aiPath.Add(new Cords(x, y, 0)); //AI path has start point
		aiPath.Add(new Cords(x, y, 0)); //do it three times for the hack to get it to work
		//Don't change the above unless you fix the spawn point stuff
		theAI = Instantiate(AI, grid[x, y].getPos());
		theAI.SetActive(false); //set to false so the AI doesn't appear until he's on the grid (see: Update function)

		//Debug info for spawnpoint
		Debug.Log("AI spawn pos: (" + x + ", " + y + ")\n");

		Cords gp = SpawnGoalPoint();
		List<Cords> thisPath = FindBestPath(gp);
		foreach (Cords tile in thisPath) {
			aiPath.Add(tile);
		}

	}

	private int runs = 0; //specifically to allow the AI to be visible as part of our "hack"

	/* Update
	 * Called every frame.
	 * 
	 * It makes the AI visible and does some other stuff.
	 */
	void Update()
	{
		if (runs == 3)
			theAI.SetActive(true);

		if (Time.time >= nextTime) {
			nextTime += interval;
			MainFunc();
			runs++;
		}

		t += Time.deltaTime / interval;
		theAI.transform.position = Vector3.Lerp(StartPos, EndPos, t);
	}

	/* Restart
	 * This is called everytime the goal point is reached and we need a new one and a new path
	 * 
	 * Doesn't return anything.
	 */
	void Restart()
	{
		CleanGrid();

		Cords gp = SpawnGoalPoint();

		aiPath = FindBestPath(gp);
	}

	/* CleanGrid
	 * This is called everytime we want to clean the grid for the next search in pathing.
	 * 
	 * After it is called, the grid's visited values will all be false.
	 */ 
	void CleanGrid()
	{
		for (int y = 0; y < COLS; y++) {
			for (int x = 0; x < ROWS; x++) {
				grid [x, y].visited = false; //reset visited for the path finding
			}
		}
	}

	/* SpawnGoalPoint
	 * Spawns the goal point and destroys the old goal point (if it exists).
	 * 
	 * Returns the Cords for the new goal point for use with path finding.
	 */ 
	Cords SpawnGoalPoint()
	{
		int x, y;

		do //goal point
		{
			x = Random.Range(0, ROWS-1);
			y = Random.Range(0, COLS-1);
		} while (!(grid[x, y].getOpen()) || (x == aiPath[0].x && y == aiPath[0].y)); //keep trying until sp is open
		Cords gp = new Cords(x, y);

		//Debug info for spawnpoint
		Debug.Log("GoalPoint spawn pos: (" + x + ", " + y + ")\n");

		Destroy(theGoalPoint);
		theGoalPoint = Instantiate(Finish, grid[x, y].getPos());
		//theGoalPoint = Instantiate(Finish, new Vector3(0,0,0), Quaternion.identity);
		//theGoalPoint.transform.SetParent(grid [x, y].getTile().gameObject.transform, false);

		//theGoalPoint = Instantiate(Finish, grid[x, y].getPos());
		return gp;
	}

	/* MainFunc
	 * The main function. It does shit.
	 */
	void MainFunc()
	{
		Debug.Log("Current Position: (" + aiPath[0].x + ", " + aiPath[0].y + ")");
	
		if (aiPath.Count == 1) {
			Restart();
		}

		grid[aiPath[0].x, aiPath[0].y].leave(); //AI leaves this grid

		aiPath.RemoveAt(0); //remove the current grid
		Debug.Log("Moving to: (" + aiPath[0].x + ", " + aiPath[0].y + ")");
		grid[aiPath[0].x, aiPath[0].y].enter(); //AI enters this grid
		Transform target = grid[aiPath[0].x, aiPath[0].y].getPos(); //where we're moving
		StartPos = theAI.transform.position;
		EndPos = target.transform.position;
		t = 0; //reset timer
	}

	/* isAdjacent
	 * Takes two arguments, both of types "Cords"
	 * 
	 * Returns true if one and two are adjacent.
	 */
	bool isAdjacent(Cords one, Cords two)
	{
		if (one.x-1 == two.x && one.y == two.y) {
			return true;
		}
		if (one.x+1 == two.x && one.y == two.y) {
			return true;
		}
		if (one.x == two.x && one.y-1 == two.y) {
			return true;
		}
		if (one.x== two.x && one.y+1 == two.y) {
			return true;
		}

		return false;
	}

	/* CheckSpot
	 * Checks the current spot if it's open and if it's already been visited.
	 * 
	 * Returns true if it's available for pathing. False if otherwise.
	 */
	bool CheckSpot(int x, int y)
	{
		bool retvar = true;

		if (grid[x, y].getOpen() && !(grid[x, y].visited))
			grid[x, y].visited = true;
		else //it's not open or it's already been visited
			retvar = false;

		return retvar;
	}

	/* FindBestPath
	 * Searches for the best path for AI's chasing state
	 * 
	 * Takes a goal point (gp) and paths to that
	 * 
	 * Returns the list that is the best path
	 */ 
	List<Cords> FindBestPath(Cords gp)
	{
		int count = 0;
		int x = aiPath[0].x; //assume aiPath[0] will always contain the current position of the AI
		int y = aiPath[0].y;

		bool isFinished = false;
		Stack<Cords> check, move, temp;
		check = new Stack<Cords>();
		move = new Stack<Cords>();
		temp = new Stack<Cords>();
		Cords StartPoint = aiPath[0];
		StartPoint.score = 0;
		List<Cords> fullPath = new List<Cords>();
		List<Cords> bestPath = new List<Cords>(); //the list to be returned

		grid[x, y].visited = true;
		move.Push(new Cords(x, y, count));
		fullPath.Add(new Cords(x, y, count));

		while (!isFinished)
		{
			count++;
			while (!(move.Count == 0))
			{
				x = move.Peek().x;
				y = move.Peek().y;

				if (x != 0 && CheckSpot(x-1, y)) //check left
				{
					temp.Push(new Cords(x-1, y, count));
					check.Push(new Cords(x-1, y, count));
					fullPath.Add(new Cords(x-1, y, count));
				}
				if (x != 6 && CheckSpot(x+1, y)) //check right
				{
					temp.Push(new Cords(x+1, y, count));
					check.Push(new Cords(x+1, y, count));
					fullPath.Add(new Cords(x+1, y, count));
				}
				if (y != 6 && CheckSpot(x, y+1)) //check up
				{
					temp.Push(new Cords(x, y+1, count));
					check.Push(new Cords(x, y+1, count));
					fullPath.Add(new Cords(x, y+1, count));
				}
				if (y != 0 && CheckSpot(x, y-1)) //check down
				{
					temp.Push(new Cords(x, y-1, count));
					check.Push(new Cords(x, y-1, count));
					fullPath.Add(new Cords(x, y-1, count));
				}

				move.Pop();
			}

			while (!(check.Count == 0)) {
				if (check.Peek().x == gp.x && check.Peek().y == gp.y) {
					isFinished = true;
					gp.score = count;
				}
				check.Pop();
			}

			while (!(temp.Count == 0)) {
				move.Push(temp.Peek());
				temp.Pop();
			}

			temp.Clear();
		}

		//backwards trace to the AI to get a path
		List<Cords> newFullPath = new List<Cords>(); //new list to ensure we don't search through old values
		Cords comp; //comparison value to be returned

		int index = gp.score - 1; //set our search index to the next one down from the end point

		bestPath.Add(gp); //put goal point into
		comp = new Cords(gp); //comparison value
		isFinished = false; //reuse isFinished

		while (!isFinished) {
			foreach (Cords tile in fullPath) {
				if (tile.score == index) {
					if (isAdjacent(comp, tile)) {
						comp = tile; //make the new value the next comparion value
						bestPath.Add(tile);
						break;
					}
				} else { //strip away the higher level indexes
					newFullPath.Add(tile);
				}
			}

			//clean up and prepare fullPath for the next iteration
			fullPath.Clear();
			foreach (Cords tile in newFullPath) {
				fullPath.Add(tile);
			}
			newFullPath.Clear();
			index--; //search for the next lowest scores

			if (index < 0) //once we've gone below zero we've reached the goal point
				isFinished = true;
		}

		/*
		 * The following lines switch the order of the list (using a stack) to make the path in the correct order
		 * Before, the path is gp -> intermediate pos(int) 3 -> int 2 -> int 1 -> AI position
		 * Afterwards, the path is AI position -> int 1 -> int 2 -> int 3 -> gp
		 */
		Stack<Cords> s = new Stack<Cords>(); //create a stack to reverse our path

		foreach (Cords tile in bestPath) {
			s.Push(tile);
		}
		bestPath.Clear(); //clean out the best path before we dump the correct order into it

		while (!(s.Count == 0)) {
			bestPath.Add(s.Peek());
			s.Pop();
		}

		return bestPath;
	}
}