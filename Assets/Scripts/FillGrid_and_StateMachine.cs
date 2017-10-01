using System;
using System.Collections.Generic;

/// <summary>
/// Summary description for Class1
/// </summary>
public class Class1
{
	public Class1()
	{
        int[] fillArray(int x)
        {
            if (x == 1) {
                int[] xGrid = { 1, 0, 2, 3, 4, 5, 6 };
                return xGrid;
            }
            else if (x == 2) {
                int[] xGrid = { 2, 1, 3, 0, 4, 5, 6 };
                return xGrid;
            }
            else if (x == 3) {
                int[] xGrid = { 3, 2, 4, 1, 5, 0, 6 };
                return xGrid;
            }
            else if (x == 4) {
                int[] xGrid = { 4, 3, 5, 2, 6, 1, 0 };
                return xGrid;
            }
            else if (x == 5) {
                int[] xGrid = { 5, 4, 6, 3, 2, 1, 0 };
                return xGrid;
            }
            else if (x == 6) {
                int[] xGrid = { 6, 5, 4, 3, 2, 1, 0 };
                return xGrid;
            }
            else {
                int[] xGrid = { 0,1,2,3,4,5,6 };
                return xGrid;
            }
        }

        int enemyXPos = 4, enemyYPos = 0;
        int[,] positionGrid = new int[7, 7];
        int[] x = fillArray(enemyXPos);
        int[] y = fillArray(enemyYPos);
        Dictionary<int[][], int> valueGrid = new Dictionary<int[,], int>();
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                valueGrid.Add(positionGrid[i][j],0);
            }
        }
        int value;
        for (int i = 0; i < 7; i++) {
            for (int j = 0; j < 7; j++) {
                if (i != 0 || i != 6) {
                    if (Math.Abs(x[i] - x[i + 1]) != Math.Abs(x[i] - x[i - 1]))
                    {
                        //doesn't increment when same spacing away
                        value = -6 + j;
                    }
                }
                else value = -6 + j;
                valueGrid[x[i], y[j]] = value;
            }
        }
    }
}

public class StateMachine{
    StateMachine() { }
    enum DistanceAway { NOT_ASSIGNED, NEAR, MID, FAR };
    enum State { NOT_ASSIGNED, RETREATING, CAUTIOUS, PURSUING };
    DistanceAway currentDistance { get; set; }
    State currentState { get; set; }
    void changeState() {
        if ((currentState == PURSUING || currentState == CAUTIOUS) && (currentDistance == NEAR))
        {
            currentState = RETREATING;
            Retreat();
        }
        else if (currentState == PURSUING && currentDistance == MID)
        {
            currentState = CAUTIOUS;
            Caution();
        }
        else if ((currentState == CAUTIOUS || currentState == RETREATING) && (currentDistance == FAR))
        {
            currentState = PURSUING;
            Pursue();
        }
    }
    abstract void Retreat() { }
    abstract void Caution() { }
    abstract void Pursue() { }
    void changeDistance(float d)
    {
        if (d >= 0 && d < 0.4) currentDistance = NEAR;
        else if (d >= 0.4 && d < 0.7) currentDistance = MID;
        else if (d >= 0.7 && d <= 1) currentDistance = FAR;
        else d = NOT_ASSIGNED;
    }
}