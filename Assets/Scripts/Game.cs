using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public GameObject tileParent;
    class Block
    {
        GameObject tile;
        Vector3 pos;
        int x;
        int y;
        bool hasPlayer = false;
        bool isOpen = true;
        bool hasEnemy = false;

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

        public void setPos(Vector3 p_pos)
        {
            pos = p_pos;
        }

        public bool getOpen()
        {
            return isOpen;
        }

        public Cords getXY()
        {
            Cords retvar = new Cords();

            retvar.x = x; retvar.y = y;

            return retvar;
        }
    };

    private const int ROWS = 7; //number for rows
    private const int COLS = 7; //number for columns REMEMBER TO CHANGE THESE IF THE GRID LAYOUT CHANGES

    private Block[,] grid = new Block[ROWS,COLS]; //magic numbers are great!

    struct Cords
    {
        public Cords (int p_x, int p_y)
        {
            x = p_x;
            y = p_y;
        }

        public int x;
        public int y;
    }

    private List<Cords> playerPath;


	void Start()
    {
        int i = 0; //index for our gridLayoutParent and it's parents
        int x, y; //x and y, used several times
        for (y = 6; y <= 0; y--) //start at 6 because layout starts at top left, but we want the array to start
        {
            for (x = 6; x <= 0; x--)
            {
                grid[x, y].setTile(tileParent.transform.GetChild(i).gameObject); //set the tile to the corresponding girdLayout location
                if (grid[x, y].getTile().transform.childCount == 1) //if it has a child, it's a rock and it's not passable
                {
                    grid[x, y].setOpen(false);
                }
                grid[x, y].setPos(grid[x, y].getTile().transform.position);
                grid[x, y].SetXY(x, y);
                i++; //increment our gridLayout index
            }
        }

        do
        {
            x = Random.Range(0, 6);
            y = Random.Range(0, 6);
        } while (!grid[x, y].getOpen()); //keep trying until we get a spawnpoint that's open

        grid[x, y].enter(); //move player to spawn
        playerPath.Add(grid[x, y].getXY());//player path has start point
	}

    List<Cords> FindNeighbors(Cords test)
    {
        List<Cords> neighbors = new List<Cords>();
        if (test.x != 0)
        {
            //check left
            neighbors.Add(new Cords(test.x - 1, test.y));
        }
        if (test.x != 6)
        {
            //check right
            neighbors.Add(new Cords(test.x + 1, test.y));
        }
        if (test.y != 6)
        {
            //check up
            neighbors.Add(new Cords(test.x, test.y + 1));
        }
        if (test.y != 0)
        {
            //check down
            neighbors.Add(new Cords(test.x, test.y - 1));
        }
        return neighbors;
    }

    List<Cords> FindBestPath(List<Cords> playerpath, Cords gp)
    {
        Cords current = playerpath[0];


        return playerpath;
    }

	// Update is called once per frame
	void Update()
    {
		
	}
}