using UnityEngine;
using System.Collections.Generic;
using System;

public enum Col
{
    WHITE, RED, BLUE, ORANGE, GREEN, YELLOW, NULL
}

public class CubeSolver
{
    //public static Col[,] top, front, right, back, left, bottom;

    public static Col[,] top = new Col[,] { { Col.WHITE, Col.WHITE, Col.WHITE }, { Col.WHITE, Col.WHITE, Col.WHITE }, { Col.WHITE, Col.WHITE, Col.WHITE } };
    public static Col[,] front = new Col[,] { { Col.RED, Col.RED, Col.RED }, { Col.RED, Col.RED, Col.RED }, { Col.RED, Col.RED, Col.RED } };
    public static Col[,] right = new Col[,] { { Col.BLUE, Col.BLUE, Col.BLUE }, { Col.BLUE, Col.BLUE, Col.BLUE }, { Col.BLUE, Col.BLUE, Col.BLUE } };
    public static Col[,] back = new Col[,] { { Col.ORANGE, Col.ORANGE, Col.ORANGE }, { Col.ORANGE, Col.ORANGE, Col.ORANGE }, { Col.ORANGE, Col.ORANGE, Col.ORANGE } };
    public static Col[,] left = new Col[,] { { Col.GREEN, Col.GREEN, Col.GREEN }, { Col.GREEN, Col.GREEN, Col.GREEN }, { Col.GREEN, Col.GREEN, Col.GREEN } };
    public static Col[,] bottom = new Col[,] { { Col.YELLOW, Col.YELLOW, Col.YELLOW }, { Col.YELLOW, Col.YELLOW, Col.YELLOW }, { Col.YELLOW, Col.YELLOW, Col.YELLOW } };

    public static Col[][,] sides;

    public static volatile List<Pair<string, bool>> solution = new List<Pair<string, bool>>(150);
    public static volatile List<Pair<int, int>> guides = new List<Pair<int, int>>(10);
    public static volatile bool stopSolving;
    public static bool newStep, done;

    private static System.Threading.Thread solvingThread;

    //Returns an array of steps to scramble the cube and the number of scrambles
    //Each element is in the form <CoroutineName, rotateClockwise>
    public static Pair<string, bool>[] Scramble()
    {
        int numScrambles = (int)(40 * Mathf.Sin(Mathf.PI * UnityEngine.Random.value) + 20);

        Pair<string, bool>[] moves = new Pair<string, bool>[numScrambles];
        int cur, last = -2;

        bool clockwise;

        for (int i = 0; i < numScrambles; i++)
        {
            cur = UnityEngine.Random.Range(0, 6);
            clockwise = UnityEngine.Random.value < 0.5;

            //No repeated moves
            while (cur == last)
            {
                cur = UnityEngine.Random.Range(0, 6);
            }

            switch (cur)
            {
                case 0:
                    moves[i] = new Pair<string, bool>("RotateRight", clockwise);
                    RotateRightOrLeft(true, clockwise);
                    break;
                case 1:
                    moves[i] = new Pair<string, bool>("RotateLeft", clockwise);
                    RotateRightOrLeft(false, clockwise);
                    break;
                case 2:
                    moves[i] = new Pair<string, bool>("RotateTop", clockwise);
                    RotateTopOrBottom(true, clockwise);
                    break;
                case 3:
                    moves[i] = new Pair<string, bool>("RotateBottom", clockwise);
                    RotateTopOrBottom(false, clockwise);
                    break;
                case 4:
                    moves[i] = new Pair<string, bool>("RotateFront", clockwise);
                    RotateFrontOrBack(true, clockwise);
                    break;
                case 5:
                    moves[i] = new Pair<string, bool>("RotateBack", clockwise);
                    RotateFrontOrBack(false, clockwise);
                    break;
            }

            last = cur;
        }

        return moves;
    }

    //Resets the faces to the unscrambled state
    public static void Reset()
    {
        top = new Col[,] { { Col.WHITE, Col.WHITE, Col.WHITE }, { Col.WHITE, Col.WHITE, Col.WHITE }, { Col.WHITE, Col.WHITE, Col.WHITE } };
        front = new Col[,] { { Col.RED, Col.RED, Col.RED }, { Col.RED, Col.RED, Col.RED }, { Col.RED, Col.RED, Col.RED } };
        right = new Col[,] { { Col.BLUE, Col.BLUE, Col.BLUE }, { Col.BLUE, Col.BLUE, Col.BLUE }, { Col.BLUE, Col.BLUE, Col.BLUE } };
        back = new Col[,] { { Col.ORANGE, Col.ORANGE, Col.ORANGE }, { Col.ORANGE, Col.ORANGE, Col.ORANGE }, { Col.ORANGE, Col.ORANGE, Col.ORANGE } };
        left = new Col[,] { { Col.GREEN, Col.GREEN, Col.GREEN }, { Col.GREEN, Col.GREEN, Col.GREEN }, { Col.GREEN, Col.GREEN, Col.GREEN } };
        bottom = new Col[,] { { Col.YELLOW, Col.YELLOW, Col.YELLOW }, { Col.YELLOW, Col.YELLOW, Col.YELLOW }, { Col.YELLOW, Col.YELLOW, Col.YELLOW } };

        //Erases the previous solution
        solution = new List<Pair<string, bool>>(150);

        guides = new List<Pair<int, int>>(10);
    }

    //Requests that the solving thread stop
    public static void RequestStop()
    {
        stopSolving = true;
        if (solvingThread != null && solvingThread.IsAlive)
            solvingThread.Join();
    }

    //Returns if the given cube configuration is one that is physically possible
    public static bool IsValidConfiguration(Block[,,] blocks)
    {
        Block blk;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    blk = blocks[i, j, k];
                    switch (i)
                    {
                        case 0:
                            left[j, k] = ColorToCol(blk.left);
                            break;
                        case 2:
                            right[j, k] = ColorToCol(blk.right);
                            break;
                    }
                    switch (j)
                    {
                        case 0:
                            bottom[i, k] = ColorToCol(blk.bottom);
                            break;
                        case 2:
                            top[i, k] = ColorToCol(blk.top);
                            break;
                    }
                    switch (k)
                    {
                        case 0:
                            front[i, j] = ColorToCol(blk.front);
                            break;
                        case 2:
                            back[i, j] = ColorToCol(blk.back);
                            break;
                    }
                }
            }
        }

        return true;
    }

    private static Col ColorToCol(Color c)
    {
        if (c == Color.white)
            return Col.WHITE;
        else if (c == Color.red)
            return Col.RED;
        else if (c == Color.blue)
            return Col.BLUE;
        else if (c == CubeColorer.ORANGE)
            return Col.ORANGE;
        else if (c == Color.green)
            return Col.GREEN;
        else if (c == Color.yellow)
            return Col.YELLOW;
        else
            return Col.NULL;
    }

    public static void Solve()
    {
        sides = new Col[][,] { top, front, right, back, left, bottom };

        //If there already is a solving thread running, it stops it
        if (solvingThread != null && solvingThread.IsAlive)
        {
            RequestStop();
            solvingThread.Join();
        }

        //Creates the new thread and starts it
        solvingThread = new System.Threading.Thread(new System.Threading.ThreadStart(SolveCubeThread));
        solvingThread.Start();
    }

    private static void SolveCubeThread()
    {
        solution = new List<Pair<string, bool>>(150);
        guides = new List<Pair<int, int>>(10);
        if (SolveWhiteFace())
        {
            if (PlaceNonWhiteOrYellowMiddles())
            {
                if (MakeYellowCross())
                {
                    if (PlaceYellowCross())
                    {
                        if (PlaceYellowCorners())
                        {
                            if (OrientYellowCorners())
                            {
                                //Debug.Log("ENDED NORMALLY");
                            }
                            /*else
                                Debug.Log("FORCEFULLY ENDED (if 6)");*/
                        }
                        /*else
                            Debug.Log("FORCEFULLY ENDED (if 5)");*/
                    }
                    /*else
                        Debug.Log("FORCEFULLY ENDED (if 4)");*/
                }
                /*else
                    Debug.Log("FORCEFULLY ENDED (if 3)");*/
            }
            /*else
                Debug.Log("FORCEFULLY ENDED (if 2)");*/
        }
        /*else
            Debug.Log("FORCEFULLY ENDED (if 1)");*/
    }

    private static bool SolveWhiteFace()
    {
        if (MakeWhiteCross())
        {
            return PlaceWhiteCorners();
        }
        else
            return true;
    }

    //Creates the white cross
    //Returns true if making the cross was successful
    //Return false if the thread was requested to stop
    private static bool MakeWhiteCross()
    {
        //Adds guide to guide list
        guides.Add(new Pair<int, int>(solution.Count, Guides.WHITE_CROSS));

        Col[] cols = { Col.RED, Col.BLUE, Col.ORANGE, Col.GREEN };
        Triplet<int, int, int>[] finalPositions = { new Triplet<int, int, int>(1, 2, 0), new Triplet<int, int, int>(2, 2, 1),
                                                    new Triplet<int, int, int>(1, 2, 2), new Triplet<int, int, int>(0, 2, 1) };
        Col col;

        LinkedList<Pair<string, bool>> steps = new LinkedList<Pair<string, bool>>();

        int iterations = 0;

        for (int i = 0; i < cols.Length; i++)
        {
            col = cols[i];
            while (true)
            {
                if (stopSolving)
                    return false;

                //Finds the position of the col + white 2-faced block
                Triplet<int, int, int> pos = FindTwoFacedBlock(col, Col.WHITE);

                //The side of the block that is not white
                Col[,] colSide = GetSideWithColorAtPosition(col, pos);

                //If the block is in the correct position and the white face is to the top
                if (pos.Equals(finalPositions[i]) && colSide != top)
                {
                    break;
                }

                //If the block's nonwhite color is on the same colored side
                if (colSide == GetSideWithColor(col))
                {
                    int rotations = 0;
                    Pair<string, bool> step = null;

                    //While not rotated properly
                    while (!pos.Equals(finalPositions[i]))
                    {
                        //Keep on rotating clockwise (for simplicity)
                        step = RotateSide(colSide, true);
                        rotations++;

                        pos = FindTwoFacedBlock(col, Col.WHITE);
                        colSide = GetSideWithColorAtPosition(col, pos);
                    }

                    //If the face had to be rotated 3 times that means it could have been rotated the other way once
                    if (rotations == 3)
                    {
                        solution.Add(new Pair<string, bool>(step.Item1, !step.Item2));
                        newStep = true;
                    }
                    else
                    {
                        for (int j = 0; j < rotations; j++)
                        {
                            solution.Add(step);
                            newStep = true;
                        }
                    }

                    i = -1;
                    break;
                }

                //If the block is on the bottom (y-value of 0) and the white side is facing the bottom
                else if (pos.Item2 == 0 && colSide != bottom)
                {
                    int rotations = 0;
                    Pair<string, bool> step = null;

                    //While the nonwhite face is not on the correct side
                    while (colSide != GetSideWithColor(col))
                    {
                        //Keep on rotating the bottom clockwise
                        step = RotateTopOrBottom(false, true);
                        rotations++;
                        colSide = GetSideRightOf(colSide);
                    }
                    //If the face had to be rotated 3 times that means it could have been rotated the other way once
                    if (rotations == 3)
                    {
                        solution.Add(new Pair<string, bool>(step.Item1, !step.Item2));
                        newStep = true;
                    }
                    else
                    {
                        for (int j = 0; j < rotations; j++)
                        {
                            solution.Add(step);
                            newStep = true;
                        }
                    }
                    continue;           //Next iteration it will be brought to place
                }

                //If the block is at the top or bottom and the nonwhite face is on the top or bottom
                else if (pos.Item2 != 1 && (colSide == bottom || colSide == top))
                {
                    //Rotates the face with the white side clockwise 1 rotation
                    solution.Add(RotateSide(GetSideWithColorAtPosition(Col.WHITE, pos), true));
                    newStep = true;
                    continue;
                }

                //If the block is not on the bottom and the colored face is not on the top or bottom
                else if (pos.Item2 != 0 && colSide != top && colSide != bottom)
                {
                    int rotations = 0;
                    Pair<string, bool> step = null;

                    //While the white face is not on the bottom
                    while (pos.Item2 != 0)
                    {
                        //Keep on rotating the side with the white face
                        step = RotateSide(colSide, true);
                        rotations++;
                        pos = FindTwoFacedBlock(col, Col.WHITE);
                    }

                    if (rotations == 3)
                        solution.Add(new Pair<string, bool>(step.Item1, !step.Item2));
                    else
                    {
                        for (int j = 0; j < rotations; j++)
                        {
                            solution.Add(step);
                        }
                    }
                    continue;
                }
            }

            if (i == 3 && !MadeWhiteCross())
                i = 0;


        }

        return true;
    }

    private static bool PlaceWhiteCorners()
    {
        //Adds guide to guide list
        guides.Add(new Pair<int, int>(solution.Count, Guides.WHITE_CORNERS));

        Col[][,] sides = new Col[][,] { front, right, back, left };

        Triplet<int, int, int>[] bottomCorners = { new Triplet<int, int, int>(0, 0, 0), new Triplet<int, int, int>(2, 0, 0),
                                                   new Triplet<int, int, int>(2, 0, 2), new Triplet<int, int, int>(0, 0, 2) };
        Triplet<int, int, int>[] topCorners = { new Triplet<int, int, int>(0, 2, 0), new Triplet<int, int, int>(2, 2, 0),
                                                new Triplet<int, int, int>(2, 2, 2), new Triplet<int, int, int>(0, 2, 2) };

        Pair<int, int> posWhite, posBottom, posOther;
        Col[,] sideOther, targetSide;
        Col bottomCol;

        bool cont = false;

        while (true)
        {
            /*  ____________
               /___/___/___/|
              /___/___/___/ |
             /   /   /   /  |
            +---+---+---+  /|
            |   |   |   | / |
            +---+---+---+/ /|
            |   |   |   | / | 
            +---+---+---+/ /
            |   |   |   | /
            +---+---+---+/

            */

            for (int i = 0; i < sides.Length; i++)
            {
                Col[,] side = sides[i];
                foreach (Triplet<int, int, int> corner in bottomCorners)
                {
                    if (stopSolving)
                        return false;
                    posWhite = Get2DPosition(side, corner);

                    //If the position exists and the color at that position is white
                    if (posWhite != null && side[posWhite.Item1, posWhite.Item2] == Col.WHITE)
                    {
                        //Gets the 2D position of the bottom face and its color
                        posBottom = Get2DPosition(bottom, corner);
                        bottomCol = bottom[posBottom.Item1, posBottom.Item2];

                        //Gets the side and 2D position of the other side
                        //Could be to the right or left of the white so both options must be checked
                        sideOther = GetSideLeftOf(side);
                        posOther = Get2DPosition(sideOther, corner);
                        if (posOther == null)
                        {
                            sideOther = GetSideRightOf(side);
                            posOther = Get2DPosition(sideOther, corner);
                        }

                        //The target side for the other side is the side with the color of the bottom face
                        targetSide = GetSideWithColor(bottomCol);
                        int rotations = 0;
                        Pair<string, bool> step = null;
                        while (targetSide != sideOther)
                        {
                            if (stopSolving)
                            {
                                //Debug.Log("FORCEFULLY ENDED (while 1)");
                                return false;
                            }

                            step = RotateTopOrBottom(false, true);
                            sideOther = GetSideRightOf(sideOther);
                            side = GetSideRightOf(side);
                            rotations++;
                        }

                        //Makes the optimization if it can rotate once counterclockwise instead of 3 times clockwise
                        //Adds the step(s) to the solution
                        if (rotations == 3)
                        {
                            solution.Add(new Pair<string, bool>(step.Item1, !step.Item2));
                        }
                        else
                        {
                            for (int j = 0; j < rotations; j++)
                            {
                                solution.Add(step);
                            }
                        }

                        bool whiteOnRight = side == GetSideRightOf(sideOther);
                        Col[,] opp = GetSideOppositeOf(side);

                        //Rotates the white middle down
                        solution.Add(RotateSide(opp, whiteOnRight));

                        //Rotates the white corner next to the middle
                        solution.Add(RotateTopOrBottom(false, !whiteOnRight));

                        //Rotates both back up to the top
                        solution.Add(RotateSide(opp, !whiteOnRight));

                        //Search all bottom corners again since blocks got moved around
                        i = -1;
                        break;
                    }
                }
            }

            cont = false;
            for (int i = 0; i < sides.Length; i++)
            {
                Col[,] side = sides[i];
                foreach (Triplet<int, int, int> corner in topCorners)
                {
                    if (stopSolving)
                        return false;

                    posWhite = Get2DPosition(side, corner);

                    //If the position exists and the color at that position is white
                    if (posWhite != null && side[posWhite.Item1, posWhite.Item2] == Col.WHITE)
                    {
                        Col[,] onRight = GetSideRightOf(side);

                        //If the white face is on the right of the block
                        bool whiteOnRight = Get2DPosition(onRight, corner) == null;

                        //Rotates side down
                        solution.Add(RotateSide(side, !whiteOnRight));

                        //Rotates bottom away from side
                        solution.Add(RotateSide(bottom, !whiteOnRight));

                        //Rotates side back up
                        solution.Add(RotateSide(side, whiteOnRight));

                        //Return back to the top of the while to add the block (oriented properly) into its correct corner
                        cont = true;
                        break;
                    }
                }
                if (cont)
                    break;
            }
            if (cont)
                continue;

            cont = false;
            for (int i = 0; i < bottomCorners.Length; i++)
            {
                if (stopSolving)
                    return false;

                Triplet<int, int, int> corner = bottomCorners[i];
                Pair<int, int> pos = Get2DPosition(bottom, corner);

                if (bottom[pos.Item1, pos.Item2] == Col.WHITE)
                {
                    Pair<string, bool> step = null;
                    int rotations = 0, j = i;

                    while (top[pos.Item1, pos.Item2] == Col.WHITE)
                    {
                        if (stopSolving)
                        {
                            //Debug.Log("FORCEFULLY ENDED (while 2)");
                            return false;
                        }
                        corner = bottomCorners[++j == bottomCorners.Length ? j = 0 : j];
                        pos = Get2DPosition(bottom, corner);
                        rotations++;
                        step = RotateSide(bottom, true);
                    }

                    //Adds the steps to the solution
                    //Optimizes if can do 1 counterclockwise instead of 3 clockwise
                    if (rotations == 3)
                    {
                        solution.Add(new Pair<string, bool>(step.Item1, !step.Item2));
                    }
                    else
                    {
                        for (int k = 0; k < rotations; k++)
                        {
                            solution.Add(step);
                        }
                    }

                    Col[][,] sidesWithCorner = new Col[2][,];
                    int n = 0;
                    foreach (Col[,] side in sides)
                    {
                        if (stopSolving)
                            return false;

                        if (Get2DPosition(side, corner) != null)
                        {
                            sidesWithCorner[n++] = side;
                            //TODO: Get which face to rotate down to then rotate the bottom twice to bring the white bottom corner to
                            //The bottom row
                        }
                    }
                    if (GetSideRightOf(sidesWithCorner[0]) == sidesWithCorner[1])
                    {
                        solution.Add(RotateSide(sidesWithCorner[0], true));
                        solution.Add(RotateTopOrBottom(false, true));
                        solution.Add(RotateTopOrBottom(false, true));
                        solution.Add(RotateSide(sidesWithCorner[0], false));
                    }
                    else
                    {
                        solution.Add(RotateSide(sidesWithCorner[1], true));
                        solution.Add(RotateTopOrBottom(false, true));
                        solution.Add(RotateTopOrBottom(false, true));
                        solution.Add(RotateSide(sidesWithCorner[1], false));
                    }
                    cont = true;
                    break;
                }
            }
            if (cont)
                continue;

            if (WhiteCornersInPlace())
                break;
        }

        return true;
    }

    private static bool PlaceNonWhiteOrYellowMiddles()
    {
        //Adds guide to guide list
        guides.Add(new Pair<int, int>(solution.Count, Guides.MIDDLE_ROW));

        Col[][,] sides = { front, right, back, left };

        Triplet<int, int, int>[] bottomMiddles = { new Triplet<int, int, int>(1, 0, 0), new Triplet<int, int, int>(2, 0, 1),
                                                new Triplet<int, int, int>(1, 0, 2), new Triplet<int, int, int>(0, 0, 1) };

        Triplet<int, int, int>[] sideMiddles = { new Triplet<int, int, int>(0, 1, 0), new Triplet<int, int, int>(2, 1, 0),
                                                 new Triplet<int, int, int>(2, 1, 2), new Triplet<int, int, int>(0, 1, 2) };

        while (true)
        {
            for (int i = 0; i < sides.Length; i++)
            {
                if (stopSolving)
                    return true;

                Triplet<int, int, int> pos = bottomMiddles[i];
                Col[,] side = sides[i];

                Pair<int, int> bottomPos = Get2DPosition(bottom, pos);
                Pair<int, int> sidePos = Get2DPosition(side, pos);

                //If the 2-faced block doesn't have yellow on either of its faces
                if (bottom[bottomPos.Item1, bottomPos.Item2] != Col.YELLOW &&
                    side[sidePos.Item1, sidePos.Item2] != Col.YELLOW)
                {
                    int j = i;
                    int rotations = 0;
                    Pair<string, bool> step = null;

                    Col bottomCol = bottom[bottomPos.Item1, bottomPos.Item2],
                        otherCol = side[sidePos.Item1, sidePos.Item2];

                    //While the side is not opposite of the side of the color on the bottom
                    while (side != GetSideOppositeOf(GetSideWithColor(bottomCol)))
                    {
                        step = RotateTopOrBottom(false, true);
                        side = sides[++j == sides.Length ? j = 0 : j];
                        rotations++;
                    }

                    //Adds the steps to the solution
                    //Optimizes if can do 1 counterclockwise instead of 3 clockwise
                    if (rotations == 3)
                    {
                        solution.Add(new Pair<string, bool>(step.Item1, !step.Item2));
                    }
                    else
                    {
                        for (int k = 0; k < rotations; k++)
                        {
                            solution.Add(step);
                        }
                    }
                    
                    if (GetColorWithSide(GetSideLeftOf(side)) == otherCol)
                    {
                        solution.Add(RotateSide(GetSideOppositeOf(side), true));
                        solution.Add(RotateSide(bottom, false));
                        solution.Add(RotateSide(GetSideOppositeOf(side), false));
                        solution.Add(RotateSide(bottom, false));
                        solution.Add(RotateSide(GetSideLeftOf(side), false));
                        solution.Add(RotateSide(bottom, true));
                        solution.Add(RotateSide(GetSideLeftOf(side), true));
                    }
                    //If its on the left
                    else
                    {
                        solution.Add(RotateSide(GetSideOppositeOf(side), false));
                        solution.Add(RotateSide(bottom, true));
                        solution.Add(RotateSide(GetSideOppositeOf(side), true));
                        solution.Add(RotateSide(bottom, true));
                        solution.Add(RotateSide(GetSideRightOf(side), true));
                        solution.Add(RotateSide(bottom, false));
                        solution.Add(RotateSide(GetSideRightOf(side), false));
                    }

                    i = -1;
                    continue;
                }
            }

            for (int i = 0; i < sides.Length; i++)
            {
                if (stopSolving)
                    return false;

                Col[,] side = sides[i];

                Col[,] toLeft = GetSideLeftOf(side);

                Triplet<int, int, int> pos = sideMiddles[i];

                Pair<int, int> posSide = Get2DPosition(side, pos),
                    posLeft = Get2DPosition(toLeft, pos);

                //If neither faces are yellow, and at least 1 of the faces is in the wrong spot
                if (side[posSide.Item1, posSide.Item2] != Col.YELLOW && toLeft[posLeft.Item1, posLeft.Item2] != Col.YELLOW &&
                    (side[posSide.Item1, posSide.Item2] != GetColorWithSide(side) ||
                     toLeft[posLeft.Item1, posLeft.Item2] != GetColorWithSide(toLeft)))
                {
                    toLeft = side;
                    side = GetSideRightOf(side);

                    solution.Add(RotateSide(GetSideOppositeOf(side), true));
                    solution.Add(RotateSide(bottom, false));
                    solution.Add(RotateSide(GetSideOppositeOf(side), false));
                    solution.Add(RotateSide(bottom, false));
                    solution.Add(RotateSide(GetSideLeftOf(side), false));
                    solution.Add(RotateSide(bottom, true));
                    solution.Add(RotateSide(GetSideLeftOf(side), true));

                    break;
                }
            }

            if (PlacedNonWhiteOrYellowMiddles())
                break;
        }

        return true;
    }

    private static bool MakeYellowCross()
    {
        Triplet<int, int, int>[] bottomMiddles = { new Triplet<int, int, int>(1, 0, 0), new Triplet<int, int, int>(2, 0, 1),
                                                   new Triplet<int, int, int>(1, 0, 2), new Triplet<int, int, int>(0, 0, 1) };

        bool[] bottomMiddlesYellow = { bottom[1, 0] == Col.YELLOW, bottom[2, 1] == Col.YELLOW,
            bottom[1, 2] == Col.YELLOW, bottom[0, 1] == Col.YELLOW };

        Col[][,] sides = { front, right, back, left };

        //If none of the tops are yellow
        if (!bottomMiddlesYellow[0] && !bottomMiddlesYellow[1] && !bottomMiddlesYellow[2] && !bottomMiddlesYellow[3])
        {
            //Adds guide to guide list
            guides.Add(new Pair<int, int>(solution.Count, Guides.YELLOW_DOT));

            solution.Add(RotateFrontOrBack(true, true));
            solution.Add(RotateTopOrBottom(false, true));
            solution.Add(RotateRightOrLeft(false, true));
            solution.Add(RotateTopOrBottom(false, false));
            solution.Add(RotateRightOrLeft(false, false));
            solution.Add(RotateFrontOrBack(true, false));

            bottomMiddlesYellow[0] = bottom[1, 0] == Col.YELLOW;
            bottomMiddlesYellow[1] = bottom[2, 1] == Col.YELLOW;
            bottomMiddlesYellow[2] = bottom[1, 2] == Col.YELLOW;
            bottomMiddlesYellow[3] = bottom[0, 1] == Col.YELLOW;
        }

        if (stopSolving)
            return false;

        //If only 2 of the tops are yellow and they are on opposite sides
        if ((bottomMiddlesYellow[0] && !bottomMiddlesYellow[1] && bottomMiddlesYellow[2] && !bottomMiddlesYellow[3]) ||
                (!bottomMiddlesYellow[0] && bottomMiddlesYellow[1] && !bottomMiddlesYellow[2] && bottomMiddlesYellow[3]))
        {
            //Adds guide to guide list
            guides.Add(new Pair<int, int>(solution.Count, Guides.YELLOW_LINE));

            solution.Add(RotateFrontOrBack(true, true));
            solution.Add(RotateTopOrBottom(false, true));
            solution.Add(RotateRightOrLeft(false, true));
            solution.Add(RotateTopOrBottom(false, false));
            solution.Add(RotateRightOrLeft(false, false));
            solution.Add(RotateFrontOrBack(true, false));

            bottomMiddlesYellow[0] = bottom[1, 0] == Col.YELLOW;
            bottomMiddlesYellow[1] = bottom[2, 1] == Col.YELLOW;
            bottomMiddlesYellow[2] = bottom[1, 2] == Col.YELLOW;
            bottomMiddlesYellow[3] = bottom[0, 1] == Col.YELLOW;
        }

        if (stopSolving)
            return false;


        for (int i = 0; i < bottomMiddlesYellow.Length; i++)
        {
            if (stopSolving)
                return false;

            if (bottomMiddlesYellow[i] && bottomMiddlesYellow[(i + 1) % 4] && !bottomMiddlesYellow[(i + 2) % 4] && !bottomMiddlesYellow[(i + 3) % 4])
            {
                Col[,] side = sides[i == 0 ? 3 : i - 1];

                //Adds guide to guide list
                guides.Add(new Pair<int, int>(solution.Count, Guides.YELLOW_HALF_CROSS));

                solution.Add(RotateSide(side, true));
                solution.Add(RotateTopOrBottom(false, true));
                solution.Add(RotateSide(GetSideLeftOf(side), true));
                solution.Add(RotateTopOrBottom(false, false));
                solution.Add(RotateSide(GetSideLeftOf(side), false));
                solution.Add(RotateSide(side, false));

                return true;
            }
        }

        if (!MadeYellowCross())
        {
            return MakeYellowCross();
        }

        return true;
    }

    //Makes sure the colours below the yellow cross match the faces below them
    private static bool PlaceYellowCross()
    {
        Col[][,] sides = { front, right, back, left };
        Triplet<int, int, int>[] bottomMiddles = { new Triplet<int, int, int>(1, 0, 0), new Triplet<int, int, int>(2, 0, 1),
                                                   new Triplet<int, int, int>(1, 0, 2), new Triplet<int, int, int>(0, 0, 1) };

        for (int i = 0; i < 4; i++)
        {
            if (stopSolving)
            {
                //Debug.Log("FOR 1");
                return false;
            }

            Col[,] side = sides[i], rightSide = sides[(i + 1) % 4], oppSide = sides[(i + 2) % 4], leftSide = sides[(i + 3) % 4];
            Triplet<int, int, int> pos3D = bottomMiddles[i], rightPos3D = bottomMiddles[(i + 1) % 4], oppPos3D = bottomMiddles[(i + 2) % 4],
                leftPos3D = bottomMiddles[(i + 3) % 4];
            Pair<int, int> pos2D = Get2DPosition(side, pos3D), rightPos2D = Get2DPosition(rightSide, rightPos3D), 
                oppPos2D = Get2DPosition(oppSide, oppPos3D), leftPos2D = Get2DPosition(leftSide, leftPos3D);

            //If the opposite sides of the yellow cross are in the correct positions but the ones to the right and left are flipped
            if (oppSide[oppPos2D.Item1, oppPos2D.Item2] == GetColorWithSide(GetSideOppositeOf(GetSideWithColor(side[pos2D.Item1, pos2D.Item2]))) &&
                rightSide[rightPos2D.Item1, rightPos2D.Item2] != GetColorWithSide(GetSideRightOf(GetSideWithColor(side[pos2D.Item1, pos2D.Item2]))))
            {
                //Adds guide to guide list
                guides.Add(new Pair<int, int>(solution.Count, Guides.YELLOW_CROSS_OPPOSITES));

                solution.Add(RotateSide(side, true));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(side, false));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(side, true));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(side, false));

                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(leftSide, true));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(leftSide, false));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(leftSide, true));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(leftSide, false));

                break;
            }

            if (leftSide[leftPos2D.Item1, leftPos2D.Item2] == GetColorWithSide(GetSideLeftOf(GetSideWithColor(side[pos2D.Item1, pos2D.Item2]))) &&
                rightSide[rightPos2D.Item1, rightPos2D.Item2] != GetColorWithSide(GetSideRightOf(GetSideWithColor(side[pos2D.Item1, pos2D.Item2]))))
            {
                //Adds guide to guide list
                guides.Add(new Pair<int, int>(solution.Count, Guides.YELLOW_CROSS_ADJACENTS));

                solution.Add(RotateSide(side, true));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(side, false));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(side, true));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(bottom, true));
                solution.Add(RotateSide(side, false));

                break;
            }
        }

        int rotations = 0;
        Pair<string, bool> step = null;

        while (!PlacedYellowCross())
        {
            if (stopSolving)
            {
                //Debug.Log("WHILE 1");
                return false;
            }

            step = RotateTopOrBottom(false, true);
            rotations++;
        }

        if (rotations == 3)
        {
            solution.Add(new Pair<string, bool>(step.Item1, !step.Item2));
            newStep = true;
        }
        else
        {
            for (int j = 0; j < rotations; j++)
            {
                solution.Add(step);
                newStep = true;
            }
        }

        return true;
    }

    //Places the 4 yellow corners in the correct positions
    private static bool PlaceYellowCorners()
    {
        Triplet<int, int, int>[] bottomCorners = { new Triplet<int, int, int>(0, 0, 0), new Triplet<int, int, int>(2, 0, 0),
                                                   new Triplet<int, int, int>(2, 0, 2), new Triplet<int, int, int>(0, 0, 2) };

        Col[][,] sides = { front, right, back, left };

        int inRightPlace = 0;

        Triplet<Col, Col, Col>[] cornerCols = new Triplet<Col, Col, Col>[4];

        Pair<int, int> pos = null;

        for (int i = 0; i < 4; i++)
        {
            if (stopSolving)
                return false;

            cornerCols[i] = new Triplet<Col, Col, Col>(Col.NULL, Col.NULL, Col.NULL);

            cornerCols[i].Item1 = GetColorAt(bottom, bottomCorners[i]);
            cornerCols[i].Item2 = GetColorAt(sides[i], bottomCorners[i]);
            cornerCols[i].Item3 = GetColorAt(sides[(i + 3) % 4], bottomCorners[i]);

            if (cornerCols[i].Contains(GetColorWithSide(sides[i])) && cornerCols[i].Contains(GetColorWithSide(sides[(i + 3) % 4])))
                inRightPlace |= 1 << i;
        }

        if (inRightPlace == 0)
        {
            //Adds guide to guide list
            guides.Add(new Pair<int, int>(solution.Count, Guides.YELLOW_CORNERS_POSITION_ONE));

            solution.Add(RotateSide(bottom, true));
            solution.Add(RotateSide(left, true));
            solution.Add(RotateSide(bottom, false));
            solution.Add(RotateSide(right, false));
            solution.Add(RotateSide(bottom, true));
            solution.Add(RotateSide(left, false));
            solution.Add(RotateSide(bottom, false));
            solution.Add(RotateSide(right, true));

            return PlaceYellowCorners();
        }

        if (inRightPlace != 15)
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                if (inRightPlace == 1 << i)
                    break;
            }

            Col[,] side = sides[(i + 3) % 4], oppSide = GetSideOppositeOf(side);

            //Adds guide to guide list
            guides.Add(new Pair<int, int>(solution.Count, Guides.YELLOW_CORNERS_POSITION_ALL));

            solution.Add(RotateSide(bottom, true));
            solution.Add(RotateSide(side, true));
            solution.Add(RotateSide(bottom, false));
            solution.Add(RotateSide(oppSide, false));
            solution.Add(RotateSide(bottom, true));
            solution.Add(RotateSide(side, false));
            solution.Add(RotateSide(bottom, false));
            solution.Add(RotateSide(oppSide, true));

            return PlaceYellowCorners();
        }

        return true;
    }

    private static bool OrientYellowCorners()
    {
        //Adds guide to guide list
        guides.Add(new Pair<int, int>(solution.Count, Guides.ORIENT_YELLOW_CORNERS));

        Pair<int, int>[] corners = { new Pair<int, int>(0, 0), new Pair<int, int>(2, 0), new Pair<int, int>(2, 2), new Pair<int, int>(0, 2) };

        Col[][,] sides = { front, right, back, left };

        Pair<int, int> corner = null;
        Col[,] side = null;

        int rotations;
        Pair<string, bool> step;

        int i;
        for (i = 0; i < 4; i++)
        {
            if (stopSolving)
                return false;

            corner = corners[i];
            if (bottom[corner.Item1, corner.Item2] != Col.YELLOW)
            {
                side = sides[(i + 3) % 4];

                solution.Add(RotateSide(side, false));
                solution.Add(RotateSide(top, false));
                solution.Add(RotateSide(side, true));
                solution.Add(RotateSide(top, true));

                solution.Add(RotateSide(side, false));
                solution.Add(RotateSide(top, false));
                solution.Add(RotateSide(side, true));
                solution.Add(RotateSide(top, true));

                break;
            }
        }

        while (!OrientedYellowCorners())
        {
            if (stopSolving)
                return false;

            rotations = 0;
            step = null;

            while (bottom[corner.Item1, corner.Item2] == Col.YELLOW)
            {
                if (stopSolving)
                    return false;

                step = RotateTopOrBottom(false, true);
                rotations++;
            }

            if (rotations == 3)
            {
                solution.Add(new Pair<string, bool>(step.Item1, !step.Item2));
                newStep = true;
            }
            else
            {
                for (int j = 0; j < rotations; j++)
                {
                    solution.Add(step);
                    newStep = true;
                }
            }

            solution.Add(RotateSide(side, false));
            solution.Add(RotateSide(top, false));
            solution.Add(RotateSide(side, true));
            solution.Add(RotateSide(top, true));

            solution.Add(RotateSide(side, false));
            solution.Add(RotateSide(top, false));
            solution.Add(RotateSide(side, true));
            solution.Add(RotateSide(top, true));
        }

        rotations = 0;
        step = null;

        while (front[0, 0] != Col.RED)
        {
            step = RotateTopOrBottom(false, true);
            rotations++;
        }

        if (rotations == 3)
        {
            solution.Add(new Pair<string, bool>(step.Item1, !step.Item2));
            newStep = true;
        }
        else
        {
            for (int j = 0; j < rotations; j++)
            {
                solution.Add(step);
                newStep = true;
            }
        }

        return true;
    }

    //Returns whether or not the white cross has been made
    private static bool MadeWhiteCross()
    {
        return top[1, 0] == Col.WHITE && top[2, 1] == Col.WHITE && top[1, 2] == Col.WHITE && top[0, 1] == Col.WHITE &&
            front[1, 2] == Col.RED && right[2, 1] == Col.BLUE && back[1, 2] == Col.ORANGE && left[2, 1] == Col.GREEN;
    }

    //Returns if all the white corners are in their place and facing the right way
    private static bool WhiteCornersInPlace()
    {
        return top[0, 0] == Col.WHITE && top[2, 0] == Col.WHITE && top[0, 2] == Col.WHITE && top[2, 2] == Col.WHITE &&
            front[0, 2] == Col.RED && front[2, 2] == Col.RED &&
            right[2, 0] == Col.BLUE && right[2, 2] == Col.BLUE &&
            back[0, 2] == Col.ORANGE && back[2, 2] == Col.ORANGE &&
            left[2, 0] == Col.GREEN && left[2, 2] == Col.GREEN;
    }

    private static bool PlacedNonWhiteOrYellowMiddles()
    {
        return front[0, 1] == Col.RED && front[2, 1] == Col.RED &&
            right[1, 0] == Col.BLUE && right[1, 2] == Col.BLUE &&
            back[0, 1] == Col.ORANGE && back[2, 1] == Col.ORANGE &&
            left[1, 0] == Col.GREEN && left[1, 2] == Col.GREEN;
    }

    private static bool MadeYellowCross()
    {
        return bottom[1, 0] == Col.YELLOW && bottom[2, 1] == Col.YELLOW &&
            bottom[1, 2] == Col.YELLOW && bottom[0, 1] == Col.YELLOW;
    }

    private static bool PlacedYellowCross()
    {
        return front[1, 0] == Col.RED && right[0, 1] == Col.BLUE &&
            back[1, 0] == Col.ORANGE && left[0, 1] == Col.GREEN;
    }

    private static bool OrientedYellowCorners()
    {
        return bottom[0, 0] == Col.YELLOW && bottom[2, 0] == Col.YELLOW &&
            bottom[2, 2] == Col.YELLOW && bottom[0, 2] == Col.YELLOW;
    }

    //Finds the 2-faced block specified by the 2 colors
    //If the block exists it returns its global position in the form of a Triplet
    //If the block doesn't exist it returns null
    private static Triplet<int, int, int> FindTwoFacedBlock(Col c1, Col c2)
    {
        Triplet<int, int, int>[][] locs = new Triplet<int, int, int>[2][];
        locs[0] = new Triplet<int, int, int>[4];
        locs[1] = new Triplet<int, int, int>[4];

        int i1 = 0, i2 = 0;

        Col[] cols = { c1, c2 };
        int[][] arr = { new int[] { 0, 1 }, new int[] { 1, 0 }, new int[] { 2, 1 }, new int[] { 1, 2 } };
        Col[][,] sides = new Col[][,] { top, front, right, back, left, bottom };

        //Iterates through the 4 possible positions 2-faced blocks can be on a side
        foreach (int[] a in arr)
        {
            //Iterates through all 6 sides
            foreach (Col[,] c in sides)
            {
                //Iterates through both colors
                for (int i = 0; i < cols.Length; i++)
                {
                    //If the color at the current position on the current face is the same as the current colour
                    //Record it in the appropriate array
                    if (c[a[0], a[1]] == cols[i])
                        locs[i][(i == 0 ? i1++ : i2++)] = Get3DPosition(c, a[0], a[1]);
                }
            }
        }

        foreach (Triplet<int, int, int> t1 in locs[0])
        {
            foreach (Triplet<int, int, int> t2 in locs[1])
            {
                if (t1.Equals(t2))
                    return t1;
            }
        }

        return null;
    }

    //Returns the global position of a block on a face
    //Returns null if arr is not one of the face arrays
    private static Triplet<int, int, int> Get3DPosition(Col[,] arr, int i, int j)
    {
        if (arr == top)
            return new Triplet<int, int, int>(i, 2, j);
        else if (arr == front)
            return new Triplet<int, int, int>(i, j, 0);
        else if (arr == right)
            return new Triplet<int, int, int>(2, i, j);
        else if (arr == back)
            return new Triplet<int, int, int>(i, j, 2);
        else if (arr == left)
            return new Triplet<int, int, int>(0, i, j);
        else if (arr == bottom)
            return new Triplet<int, int, int>(i, 0, j);
        else
            return null;
    }

    private static Pair<int, int> Get2DPosition(Col[,] arr, Triplet<int, int, int> pos)
    {
        return Get2DPosition(arr, pos.Item1, pos.Item2, pos.Item3);
    }

    //Returns the 3D position of an object as a 2D position relative to the given array
    //Returns null if the position isn't on the array or the array is not a side
    private static Pair<int, int> Get2DPosition(Col[,] arr, int x, int y, int z)
    {
        //If the point is not on the side
        if ((arr == top && y != 2) ||
            (arr == front && z != 0) ||
            (arr == right && x != 2) ||
            (arr == back && z != 2) ||
            (arr == left && x != 0) ||
            (arr == bottom && y != 0))
            return null;

        if (arr == top)
            return new Pair<int, int>(x, z);
        else if (arr == front)
            return new Pair<int, int>(x, y);
        else if (arr == right)
            return new Pair<int, int>(y, z);
        else if (arr == back)
            return new Pair<int, int>(x, y);
        else if (arr == left)
            return new Pair<int, int>(y, z);
        else if (arr == bottom)
            return new Pair<int, int>(x, z);
        else
            return null;
    }

    private static Col[,] GetSideWithColor(Col col)
    {
        switch (col)
        {
            case Col.WHITE:
                return top;
            case Col.RED:
                return front;
            case Col.BLUE:
                return right;
            case Col.ORANGE:
                return back;
            case Col.GREEN:
                return left;
            case Col.YELLOW:
                return bottom;
            default:
                return null;
        }
    }

    private static Col GetColorWithSide(Col[,] side)
    {
        if (side == top)
            return Col.WHITE;
        else if (side == front)
            return Col.RED;
        else if (side == right)
            return Col.BLUE;
        else if (side == back)
            return Col.ORANGE;
        else if (side == left)
            return Col.GREEN;
        else if (side == bottom)
            return Col.YELLOW;
        else
            return Col.NULL;
    }

    private static Col GetColorAt(Col[,] side, Triplet<int, int, int> pos)
    {
        Pair<int, int> pos2D = Get2DPosition(side, pos);
        if (pos2D != null)
            return side[pos2D.Item1, pos2D.Item2];
        else
            return Col.NULL;
    }

    private static Col[,] GetSideWithColorAtPosition(Col col, Triplet<int, int, int> pos)
    {
        return GetSideWithColorAtPosition(col, pos.Item1, pos.Item2, pos.Item3);
    }
    
    //Returns which side of the cube has the given color at the given position
    //Null if it does not exist
    private static Col[,] GetSideWithColorAtPosition(Col col, int x, int y, int z)
    {
        Pair<int, int> pos;
        foreach (Col[,] s in sides)
        {
            //Gets local position of point
            pos = Get2DPosition(s, x, y, z);

            //If the position is on the side and the color at the face is the desired one
            //Return the side
            if (pos != null && s[pos.Item1, pos.Item2] == col)
            {
                return s;
            }
        }

        return null;
    }
    private static Col[,] GetSideRightOf(Col[,] side)
    {
        return GetSideRightOf(GetColorWithSide(side));
    }


    //Returns the side that is to the right of the current color
    private static Col[,] GetSideRightOf(Col col)
    {
        switch (col)
        {
            case Col.RED:
                return right;
            case Col.BLUE:
                return back;
            case Col.ORANGE:
                return left;
            case Col.GREEN:
                return front;
            default:            //White and yellow dont have sides to the right
                return null;
        }
    }

    private static Col[,] GetSideLeftOf(Col[,] side)
    {
        return GetSideLeftOf(GetColorWithSide(side));
    }


    //Returns the side that is to the left of the current color
    private static Col[,] GetSideLeftOf(Col col)
    {
        switch (col)
        {
            case Col.RED:
                return left;
            case Col.BLUE:
                return front;
            case Col.ORANGE:
                return right;
            case Col.GREEN:
                return back;
            default:            //White and yellow dont have sides to the right
                return null;
        }
    }

    //Returns the side opposite of the given side
    //Pairs are top/bottom, left/right and front/back
    private static Col[,] GetSideOppositeOf(Col[,] side)
    {
        if (side == top)
            return bottom;
        else if (side == front)
            return back;
        else if (side == right)
            return left;
        else if (side == back)
            return front;
        else if (side == left)
            return right;
        else if (side == bottom)
            return top;
        else
            return null;
    }

    //Rotates the given side in the given direction
    //Does nothing if not a valid side
    //Returns the step was performed or null if no rotation occured
    private static Pair<string, bool> RotateSide(Col[,] side, bool clockwise)
    {
        if (side == top)
            return RotateTopOrBottom(true, clockwise);
        else if (side == front)
            return RotateFrontOrBack(true, clockwise);
        else if (side == right)
            return RotateRightOrLeft(true, clockwise);
        else if (side == back)
            return RotateFrontOrBack(false, clockwise);
        else if (side == left)
            return RotateRightOrLeft(false, clockwise);
        else if (side == bottom)
            return RotateTopOrBottom(false, clockwise);
        return null;
    }

    private static Pair<string, bool> RotateFrontOrBack(bool isFront, bool clockwise)
    {
        int c = 0, m = 0, z = 0, s = 0;
        Col temp;
        Col[,] arr1 = null, arr2 = null, face = null;
        Col[] tempArr;
        if (isFront)
        {
            face = front;
            if (clockwise)
            {
                c = 0;
                m = 0;
                z = 0;
                s = 0;
                arr1 = left;
                arr2 = right;
            }
            else
            {
                c = 2;
                m = 2;
                z = 0;
                s = 2;
                arr1 = right;
                arr2 = left;
            }
        }
        else
        {
            face = back;
            z = 2;
            if (!clockwise)
            {
                c = 0;
                m = 0;
                s = 0;
                arr1 = left;
                arr2 = right;
            }
            else
            {
                c = 2;
                m = 2;
                s = 2;
                arr1 = right;
                arr2 = left;
            }
        }

        //Rotation of corners on the face
        temp = face[0, 2];
        face[0, 2] = face[c, c];
        face[c, c] = face[2, 0];
        face[2, 0] = face[2 - c, 2 - c];
        face[2 - c, 2 - c] = temp;

        //Rotation of middles of the face
        temp = face[0, 1];
        face[0, 1] = face[1, m];
        face[1, m] = face[2, 1];
        face[2, 1] = face[1, 2 - m];
        face[1, 2 - m] = temp;

        //Rotation of sides
        tempArr = new Col[] { top[0, z], top[1, z], top[2, z] };

        top[0, z] = arr1[s, z];
        top[1, z] = arr1[1, z];
        top[2, z] = arr1[2 - s, z];

        arr1[0, z] = bottom[2 - s, z];
        arr1[1, z] = bottom[1, z];
        arr1[2, z] = bottom[s, z];

        bottom[0, z] = arr2[s, z];
        bottom[1, z] = arr2[1, z];
        bottom[2, z] = arr2[2 - s, z];

        arr2[0, z] = tempArr[2 - s];
        arr2[1, z] = tempArr[1];
        arr2[2, z] = tempArr[s];

        return new Pair<string, bool>("Rotate" + (isFront ? "Front" : "Back"), clockwise);
    }

    private static Pair<string, bool> RotateRightOrLeft(bool isRight, bool clockwise)
    {
        int c = 0, m = 0, x = 0, s = 0;
        Col temp;
        Col[,] arr1 = null, arr2 = null, face = null;
        Col[] tempArr;
        if (isRight)
        {
            face = right;
            x = 2;
            if (clockwise)
            {
                c = 2;
                m = 2;
                s = 2;
                arr1 = front;
                arr2 = back;
            }
            else
            {
                c = 0;
                m = 0;
                s = 0;
                arr1 = back;
                arr2 = front;
            }
        }
        else
        {
            face = left;
            x = 0;
            if (!clockwise)         //Counter-clockwise
            {
                c = 2;
                m = 2;
                s = 2;
                arr1 = front;
                arr2 = back;
            }
            else                    //Clockwise
            {
                c = 0;
                m = 0;
                s = 0;
                arr1 = back;
                arr2 = front;
            }
        }

        //Rotation of corners on the face
        temp = face[0, 2];
        face[0, 2] = face[c, c];
        face[c, c] = face[2, 0];
        face[2, 0] = face[2 - c, 2 - c];
        face[2 - c, 2 - c] = temp;

        //Rotation of middles of the face
        temp = face[0, 1];
        face[0, 1] = face[1, m];
        face[1, m] = face[2, 1];
        face[2, 1] = face[1, 2 - m];
        face[1, 2 - m] = temp;

        //Rotation of sides
        tempArr = new Col[] { top[x, 0], top[x, 1], top[x, 2] };

        top[x, 0] = arr1[x, 2 - s];
        top[x, 1] = arr1[x, 1];
        top[x, 2] = arr1[x, s];

        arr1[x, 0] = bottom[x, s];
        arr1[x, 1] = bottom[x, 1];
        arr1[x, 2] = bottom[x, 2 - s];

        bottom[x, 0] = arr2[x, 2 - s];
        bottom[x, 1] = arr2[x, 1];
        bottom[x, 2] = arr2[x, s];

        arr2[x, 0] = tempArr[s];
        arr2[x, 1] = tempArr[1];
        arr2[x, 2] = tempArr[2 - s];

        return new Pair<string, bool>("Rotate" + (isRight ? "Right" : "Left"), clockwise);
    }

    private static Pair<string, bool> RotateTopOrBottom(bool isTop, bool clockwise)
    {
        int c = 0, m = 0, y = 0, s = 0;
        Col temp;
        Col[,] arr1 = null, arr2 = null, face = null;
        Col[] tempArr;
        if (isTop)
        {
            face = top;
            y = 2;
            if (clockwise)
            {
                c = 2;
                m = 2;
                s = 2;
                arr1 = right;
                arr2 = left;
            }
            else
            {
                c = 0;
                m = 0;
                s = 0;
                arr1 = left;
                arr2 = right;
            }
        }
        else
        {
            face = bottom;
            y = 0;
            if (!clockwise)         //Counter-clockwise
            {
                c = 2;
                m = 2;
                s = 2;
                arr1 = right;
                arr2 = left;
            }
            else                    //Clockwise
            {
                c = 0;
                m = 0;
                s = 0;
                arr1 = left;
                arr2 = right;
            }
        }

        //Rotation of corners on the face
        temp = face[0, 2];
        face[0, 2] = face[2 - c, 2 - c];
        face[2 - c, 2 - c] = face[2, 0];
        face[2, 0] = face[c, c];
        face[c, c] = temp;

        //Rotation of middles of the face
        temp = face[0, 1];
        face[0, 1] = face[1, 2 - m];
        face[1, 2 - m] = face[2, 1];
        face[2, 1] = face[1, m];
        face[1, m] = temp; 

        //Rotation of sides
        tempArr = new Col[] { front[0, y], front[1, y], front[2, y] };

        front[0, y] = arr1[y, 2 - s];
        front[1, y] = arr1[y, 1];
        front[2, y] = arr1[y, s];

        arr1[y, 0] = back[s, y];
        arr1[y, 1] = back[1, y];
        arr1[y, 2] = back[2 - s, y];

        back[0, y] = arr2[y, 2 - s];
        back[1, y] = arr2[y, 1];
        back[2, y] = arr2[y, s];

        arr2[y, 0] = tempArr[s];
        arr2[y, 1] = tempArr[1];
        arr2[y, 2] = tempArr[2 - s];

        return new Pair<string, bool>("Rotate" + (isTop ? "Top" : "Bottom"), clockwise);
    }

    private static void PrintCube()
    {
        string topStr = "", frontStr = "", rightStr = "", backStr = "", leftStr = "", bottomStr = "";

        topStr += "\nTOP\n";
        frontStr += "\nFRONT\n";
        rightStr += "\nRIGHT\n";
        backStr += "\nBACK\n";
        leftStr += "\nLEFT\n";
        bottomStr += "\nBOTTOM\n";

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                topStr += top[i, j] + " ";
                frontStr += front[i, j] + " ";
                rightStr += right[i, j] + " ";
                backStr += back[i, j] + " ";
                leftStr += left[i, j] + " ";
                bottomStr += bottom[i, j] + " ";
            }
            topStr += "\n";
            frontStr += "\n";
            rightStr += "\n";
            backStr += "\n";
            leftStr += "\n";
            bottomStr += "\n";
        }
    }

    private static void PrintSide(Col[,] side)
    {
        string str = "";
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                str += side[i, j] + " ";
            }
            str += "\n";
        }
    }
}