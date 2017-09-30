using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	public GameObject tileParent; //gridLayout
	class Block
	{
		GameObject tile;
		Transform pos;
		//Vector3 pos;
		int x;
		int y;
		bool hasPlayer = false;
		bool isOpen = true;
		bool hasEnemy = false;
		public bool visited = false;

		public void SetXY(int p_x, int p_y)
		{
			x = p_x;
			y = p_y;
		}

		public void setTile(GameObject p_tile)
		{
			tile = p_tile;
		}

		public GameObject getTile()
		{
			return tile;
		}

		public void setOpen(bool val)
		{
			isOpen = val;
		}

		public void leave()
		{
			hasPlayer = false;
		}

		public void enter()
		{
			hasPlayer = true;
		}

		public void setPos(Transform p_pos)
		{
			pos = p_pos;
		}

		public Transform getPos()
		{
			return pos;
		}

		/*
		public void setPos(Vector3 p_pos)
		{
			pos = p_pos;
		}

		public Vector3 getPos()
		{
			return pos;
		}
		*/

		public bool getOpen()
		{
			return isOpen;
		}

		public Cords getXY()
		{
			return new Cords(x, y);
		}
	};

	private const int ROWS = 7; //number for rows
	private const int COLS = 7; //number for columns REMEMBER TO CHANGE THESE IF THE GRID LAYOUT CHANGES

	private Block[,] grid = new Block[ROWS, COLS]; 

	class Cords
	{
		public Cords(int p_x, int p_y, int p_score = -1)
		{
			x = p_x;
			y = p_y;
			score = p_score;
		}

		public Cords(Cords newCords)
		{
			x = newCords.x; y = newCords.y;
		}

		public int x;
		public int y;
		public int score = -1; //default to impassable
		public bool labled = false;
		public bool visited = false;
	}

	private List<Cords> playerPath = new List<Cords>();
	public GameObject Player;
	public GameObject Finish;
	private GameObject myPlayer;
	private int interval = 1;
	private int nextTime = 0;
	private Vector3 StartPos;
	private Vector3 EndPos;
	private float t;

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

		do //player spawn
		{
			x = Random.Range(0, ROWS-1);
			y = Random.Range(0, COLS-1);
		} while (!(grid[x, y].getOpen())); //keep trying until we get a spawnpoint that's open
		grid[x, y].enter(); //move player to spawn
		playerPath.Add(new Cords(x, y, 0)); //player path has start point

		Debug.Log("Player spawn pos:");
		Debug.Log(x);
		Debug.Log(y);
		Debug.Log('\n');

		myPlayer = Instantiate(Player, grid[x, y].getPos());
		myPlayer.transform.position = grid[x, y].getPos().position;

		do //goal point
		{
			x = Random.Range(0, ROWS-1);
			y = Random.Range(0, COLS-1);
		} while (!(grid[x, y].getOpen())); //keep trying until sp is open
		Cords gp = new Cords(x, y);

		Debug.Log("GoalPoint spawn pos:");
		Debug.Log(x);
		Debug.Log(y);
		Debug.Log('\n');

		Instantiate(Finish, grid[x, y].getPos());

		playerPath = FindBestPath(gp);
	}

	void Update()
	{
		if (Time.time >= nextTime) {
			nextTime += interval;
			MainFunc();
		}

		t += Time.deltaTime / interval;
		myPlayer.transform.position = Vector3.Lerp(StartPos, EndPos, t);
	}

	void MainFunc()
	{
		playerPath.RemoveAt(0);
		Transform target = grid[playerPath[0].x, playerPath[0].y].getPos();
		StartPos = myPlayer.transform.position;
		EndPos = target.transform.position;
		t = 0;
	}

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

	bool checkSpot(int x, int y)
	{
		bool retvar = true;

		if (grid[x, y].getOpen() && !(grid[x, y].visited))
			grid[x, y].visited = true;
		else //it's not open or it's already been visited
			retvar = false;

		return retvar;
	}

	List<Cords> FindBestPath(Cords gp)
	{
		int count = 0;
		int x = playerPath[0].x;
		int y = playerPath[0].y;
		bool isFinished = false;
		Stack<Cords> check, move, temp;
		check = new Stack<Cords>();
		move = new Stack<Cords>();
		temp = new Stack<Cords>();
		Cords StartPoint = playerPath[0];
		StartPoint.score = 0;
		List<Cords> fullPath = new List<Cords>();
		List<Cords> partialPath = new List<Cords>(); //the list to be returned

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

				if (x != 0 && checkSpot(x-1, y)) //check left
				{
					temp.Push(new Cords(x-1, y, count));
					check.Push(new Cords(x-1, y, count));
					fullPath.Add(new Cords(x-1, y, count));
				}
				if (x != 6 && checkSpot(x+1, y)) //check right
				{
					temp.Push(new Cords(x+1, y, count));
					check.Push(new Cords(x+1, y, count));
					fullPath.Add(new Cords(x+1, y, count));
				}
				if (y != 6 && checkSpot(x, y+1)) //check up
				{
					temp.Push(new Cords(x, y+1, count));
					check.Push(new Cords(x, y+1, count));
					fullPath.Add(new Cords(x, y+1, count));
				}
				if (y != 0 && checkSpot(x, y-1)) //check down
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
		/*
		Debug.Log("fullPath:");
		foreach (Cords tile in fullPath) {
			Debug.Log(tile.x);
			Debug.Log(tile.y);
			Debug.Log('\n');
		}
		*/

		//backwards trace to the player to get a path
		List<Cords> newFullPath = new List<Cords>();
		List<Cords> compPath = new List<Cords>();
		List<Cords> newCompPath = new List<Cords>();

		int index = gp.score - 1;

		Debug.Log("index:");
		Debug.Log(index);

		partialPath.Add(gp);
		compPath.Add(gp);
		isFinished = false; //reuse isFinished

		while (!isFinished) {
			foreach (Cords tile in fullPath) {
				if (tile.score == index) {
					foreach (Cords comp in compPath) {
						/*
						Debug.Log("Comparison: (" + comp.x + ", " + comp.y + ")");
						Debug.Log("Comparison index: " + comp.score);
						Debug.Log("Tile: (" + tile.x + ", " + tile.y + ")");
						Debug.Log("Tile index: " + tile.score);
						Debug.Log('\n');*/
						if (isAdjacent(comp, tile)) {
							newCompPath.Add(tile);
							partialPath.Add(tile);
							break;
						}
					}
				} else { //strip away the higher level indexes
					newFullPath.Add(tile);
				}
			}

			//clean up and prepare compPath and fullPath for the next iteration
			fullPath.Clear();
			compPath.Clear();
			foreach (Cords tile in newFullPath) {
				fullPath.Add(tile);
			}
			foreach (Cords tile in newCompPath) {
				compPath.Add(tile);
			}
			newFullPath.Clear();
			newCompPath.Clear();
			index--;
			if (index < 0)
				isFinished = true;
		}

		Stack<Cords> s = new Stack<Cords>(); //create a stack to reverse our path

		foreach (Cords tile in partialPath) {
			s.Push(tile);
		}
		partialPath.Clear();

		while (!(s.Count == 0)) {
			partialPath.Add(s.Peek());
			s.Pop();
		}

		return partialPath;
	}
}