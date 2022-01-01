using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; set; }
    private bool[,] allowedMoves { get; set; }
    private bool[,] highlightTest { get; set; }

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;

    private Quaternion whiteOrientation = Quaternion.Euler(0, 270, 0);
    private Quaternion blackOrientation = Quaternion.Euler(0, 90, 0);

    public Chessman[,] Chessmans { get; set; }
    private Chessman selectedChessman;

    //public VariationDatabase variationDatabase;

    public bool isWhiteTurn = true;
    public int turnCount = 1;
    public String moveOrder = "";
    private bool capturedPiece = false;
    private int beforeMoveX = 0;
    private int beforeMoveY = 0;
    public bool singlePlayer = false;
    public String blacksMove = "";
    public int[,] preMove = new int[8, 8];
    public int[,] postMove = new int[8, 8];

    private int rankBefore = 0;
    private int fileBefore = 0;
    private int rankAfter = 0;
    private int fileAfter = 0;


    private Material previousMat;
    public Material selectedMat;


    public int[] EnPassantMove { set; get; }

    // Use this for initialization
    void Start()
    {
        Instance = this;
        SpawnAllChessmans();
        EnPassantMove = new int[2] { -1, -1 };

        VariationDatabase.readfile();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSelection();

        if (singlePlayer)
        {
            //2. we know blacks move so we can pull it here.
            print("Black's move is : " + blacksMove);

            if(blacksMove.Equals(""))
            {
                //chill here so we aren't spamming errors left and right.
                print("We have a novelty - you're on your own.");
                singlePlayer = false;
            }
            else
            {
                noNameFunction();

                selectionX = fileBefore;
                selectionY = rankBefore;
                SelectChessman(selectionX, selectionY);//original position

                selectionX = fileAfter;
                selectionY = rankAfter;

                MoveChessman(selectionX, selectionY);//desired position

                singlePlayer = false;//must be false 

                //Vector3 moveToPosition = new Vector3(0, 2, -0.01f);
                //transform.position = Vector3.Lerp(transform.position, moveToPosition, 2f);
                print("it moved");

                
                VariationDatabase.VariationFinderHighlights(moveOrder);
            }


        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedChessman == null)
                {
                    // Select the chessman
                    SelectChessman(selectionX, selectionY);

                    beforeMoveX = selectionX;
                    beforeMoveY = selectionY;
                }
                else
                {
                    // Move the chessman
                    MoveChessman(selectionX, selectionY);
                }

                //need some logic here for if the same square is clicked twice.


                //need some logic here for if an invalid square is attempted. (this one doesn't break it, but does throw an error)

            }
        }

        if(isWhiteTurn)
        {
            print("yooo");
            VariationDatabase.VariationFinderHighlights(moveOrder);
            BoardHighlights.Instance.HighlightOption(4, 1);
        }

        if (Input.GetKey("escape"))
        {
            print("Quit application");
            Application.Quit();
            UnityEditor.EditorApplication.isPlaying = false;
        }

    }
    IEnumerator Wait(int duration)
    {
        print("gets here");
        yield return new WaitForSeconds(duration);

    }

    private void SelectChessman(int x, int y)
    {

        if (Chessmans[x, y] == null)//if you click an empty square
        {
            print("its null");
            return;
        }



        if (Chessmans[x, y].isWhite != isWhiteTurn)//if you click the opponents piece.
        {
            print("its != isWhiteTurn");
            return;
        }




        bool hasAtLeastOneMove = false;

        allowedMoves = Chessmans[x, y].PossibleMoves();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (allowedMoves[i, j])
                {
                    hasAtLeastOneMove = true;
                    i = 8;
                    break;
                }
            }
        }

        if (!hasAtLeastOneMove)
        {
            print("no allowable moves");
            return;
        }


        selectedChessman = Chessmans[x, y];
        previousMat = selectedChessman.GetComponent<MeshRenderer>().material;
        selectedMat.mainTexture = previousMat.mainTexture;
        selectedChessman.GetComponent<MeshRenderer>().material = selectedMat;

        BoardHighlights.Instance.HighLightAllowedMoves(allowedMoves);
    }

    private void MoveChessman(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            Chessman c = Chessmans[x, y];

            if (c != null && c.isWhite != isWhiteTurn)
            {
                // Capture a piece

                if (c.GetType() == typeof(King))
                {
                    // End the game
                    EndGame();
                    return;
                }

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
                capturedPiece = true;
            }
            if (x == EnPassantMove[0] && y == EnPassantMove[1])
            {
                if (isWhiteTurn)
                    c = Chessmans[x, y - 1];
                else
                    c = Chessmans[x, y + 1];

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
                capturedPiece = true;
            }
            EnPassantMove[0] = -1;
            EnPassantMove[1] = -1;
            if (selectedChessman.GetType() == typeof(Pawn))
            {
                if(y == 7) // White Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(1, x, y, true);
                    selectedChessman = Chessmans[x, y];
                }
                else if (y == 0) // Black Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(7, x, y, false);
                    selectedChessman = Chessmans[x, y];
                }
                EnPassantMove[0] = x;
                if (selectedChessman.CurrentY == 1 && y == 3)
                    EnPassantMove[1] = y - 1;
                else if (selectedChessman.CurrentY == 6 && y == 4)
                    EnPassantMove[1] = y + 1;
            }

            //if castles - this was below but i gotta figure out the right spot for it.
            if (selectedChessman.GetType() == typeof(King))
            {
                //if he moves two squares - then its a castle
                print("yaooao");

                //if it moves two squares to the left -white
                if(selectedChessman.isWhite)
                {
                    if (x == 6 && beforeMoveX == 4)
                    {
                        print("White castles short.");
                        //Chessman c = Chessmans[7, 0];
                        activeChessman.Remove(Chessmans[7, 0].gameObject);
                        //activeChessman.Remove(c.gameObject);
                        Destroy(Chessmans[7, 0].gameObject);
                        //white castle short
                        activeChessman.Remove(selectedChessman.gameObject);
                        Destroy(selectedChessman.gameObject);

                        SpawnChessman(0, 6, 0, true);//Spawn new King
                        SpawnChessman(2, 5, 0, true);//Spawn new Rook

                        //selectedChessman = Chessmans[x, y];
                    }
                    if (x == 2 && beforeMoveX == 4)
                    {
                        print("White castles long.");
                        activeChessman.Remove(Chessmans[0, 0].gameObject);
                        Destroy(Chessmans[0, 0].gameObject);
                        activeChessman.Remove(selectedChessman.gameObject);
                        Destroy(selectedChessman.gameObject);

                        SpawnChessman(0, 2, 0, true);//Spawn new King
                        SpawnChessman(2, 3, 0, true);//Spawn new Rook

                    }
                }
                if(!selectedChessman.isWhite)//if its blacks castle
                {
                    if(x == 6 && beforeMoveX == 4)
                    {
                        print("Black castles short.");
                        activeChessman.Remove(Chessmans[7, 7].gameObject);
                        //activeChessman.Remove(c.gameObject);
                        Destroy(Chessmans[7, 7].gameObject);
                        //white castle short
                        activeChessman.Remove(selectedChessman.gameObject);
                        Destroy(selectedChessman.gameObject);

                        SpawnChessman(6, 6, 7, false);//Spawn new King
                        SpawnChessman(8, 5, 7, false);//Spawn new Rook
                    }
                    if(x == 2 && beforeMoveX == 4)
                    {
                        activeChessman.Remove(Chessmans[0, 7].gameObject);
                        //activeChessman.Remove(c.gameObject);
                        Destroy(Chessmans[0, 7].gameObject);
                        //white castle short
                        activeChessman.Remove(selectedChessman.gameObject);
                        Destroy(selectedChessman.gameObject);

                        SpawnChessman(6, 2, 7, false);//Spawn new Black King
                        SpawnChessman(8, 3, 7, false);//Spawn new Black Rook
                        print("Black castles long.");
                    }

                }
            }

            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;

            isWhiteTurn = !isWhiteTurn;
        }

 

        selectedChessman.GetComponent<MeshRenderer>().material = previousMat;

        BoardHighlights.Instance.HideHighlights();
        selectedChessman = null;

        createMoveList();
        CreateFEN();

    }

    private void UpdateSelection()
    {
        if (!Camera.main) return;

        RaycastHit hit;
        //cursor is within the chessboard boundary.
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("ChessPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;

        }
        //cursor is outside the chessboard boundary.
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void SpawnChessman(int index, int x, int y, bool isWhite)
    {
        Vector3 position = GetTileCenter(x, y);
        GameObject go;

        if (isWhite)
        {
            go = Instantiate(chessmanPrefabs[index], position, whiteOrientation) as GameObject;
        }
        else
        {
            go = Instantiate(chessmanPrefabs[index], position, blackOrientation) as GameObject;
        }

        go.transform.SetParent(transform);
        Chessmans[x, y] = go.GetComponent<Chessman>();
        Chessmans[x, y].SetPosition(x, y);

        activeChessman.Add(go);

        //print(go.);
        //print(Chessmans[x, y]);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;

        return origin;
    }

    private void SpawnAllChessmans()//called at the start of the game.
    {
        Debug.Log("Spawn all the chessmen");
        activeChessman = new List<GameObject>();
        Chessmans = new Chessman[8, 8];

        /////// White ///////

        // King
        SpawnChessman(0, 4, 0, true);

        // Queen
        SpawnChessman(1, 3, 0, true);

        // Rooks
        SpawnChessman(2, 0, 0, true);
        SpawnChessman(2, 7, 0, true);

        // Bishops
        SpawnChessman(3, 2, 0, true);
        SpawnChessman(3, 5, 0, true);

        // Knights
        SpawnChessman(4, 1, 0, true);
        SpawnChessman(4, 6, 0, true);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i, 1, true);
        }


        /////// Black ///////

        // King
        SpawnChessman(6, 4, 7, false);

        // Queen
        SpawnChessman(7, 3, 7, false);

        // Rooks
        SpawnChessman(8, 0, 7, false);
        SpawnChessman(8, 7, 7, false);

        // Bishops
        SpawnChessman(9, 2, 7, false);
        SpawnChessman(9, 5, 7, false);

        // Knights
        SpawnChessman(10, 1, 7, false);
        SpawnChessman(10, 6, 7, false);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(11, i, 6, false);
        }
        turnCount = 1;

    }

    private void EndGame()
    {
        if (isWhiteTurn)
            Debug.Log("White wins");
        else
            Debug.Log("Black wins");

        foreach (GameObject go in activeChessman)
        {
            Destroy(go);
        }

        isWhiteTurn = true;
        BoardHighlights.Instance.HideHighlights();
        SpawnAllChessmans();
    }
    private String CreateFEN()
    {
        //A FEN "record" defines a particular game position, all in one text line and using only the ASCII character set. A text file with only FEN data records should have the file extension ".fen".[4]
        //A FEN record contains six fields. The separator between fields is a space. The fields are:[5]

        //Piece placement(from White's perspective). Each rank is described, starting with rank 8 and ending with rank 1; within each rank, the contents of each square are described from file "a" through file "h". Following the Standard Algebraic Notation (SAN), each piece is identified by a single letter taken from the standard English names (pawn = "P", knight = "N", bishop = "B", rook = "R", queen = "Q" and king = "K"). White pieces are designated using upper-case letters ("PNBRQK") while black pieces use lowercase ("pnbrqk"). Empty squares are noted using digits 1 through 8 (the number of empty squares), and "/" separates ranks.
        //Active color. "w" means White moves next, "b" means Black moves next.
        //Castling availability. If neither side can castle, this is "-".Otherwise, this has one or more letters: "K"(White can castle kingside), "Q"(White can castle queenside), "k"(Black can castle kingside), and / or "q"(Black can castle queenside).A move that temporarily prevents castling does not negate this notation.
        //En passant target square in algebraic notation. If there's no en passant target square, this is "-". If a pawn has just made a two-square move, this is the position "behind" the pawn. This is recorded regardless of whether there is a pawn in position to make an en passant capture.[6]
        //Halfmove clock: This is the number of halfmoves since the last capture or pawn advance.The reason for this field is that the value is used in the fifty - move rule.[7]
        //Fullmove number: The number of the full move.It starts at 1, and is incremented after Black's move.

        //Starting FEN
        //rnbqkbnr / pppppppp / 8 / 8 / 8 / 8 / PPPPPPPP / RNBQKBNR w KQkq -0 1


        String myFEN = "";
        int count = 0;
        bool isWhite = true;
        bool whiteCanCastleShort = true;
        bool whiteCanCastleLong = true;
        bool blackCanCastleShort = true;
        bool blackCanCastleLong = true;



        for (int j=7; j >= 0; j--)
        {
            for (int i=0; i <= 7; i++)
            {

                Chessman c = Chessmans[i, j];

                //selectedChessman.GetType() == typeof(Pawn)
                //print(c.GetType());

                if (c)
                {
                    if (count > 0)
                    {
                        myFEN = myFEN + count;
                        count = 0;
                    }

                    if(isWhite == c.isWhite)
                    {
                        if (c.GetType() == typeof(King))
                        {
                            myFEN = myFEN + "K";
                        }
                        if (c.GetType() == typeof(Queen))
                        {
                            myFEN = myFEN + "Q";
                        }
                        if (c.GetType() == typeof(Rook))
                        {
                            myFEN = myFEN + "R";
                        }
                        if (c.GetType() == typeof(Bishop))
                        {
                            myFEN = myFEN + "B";
                        }
                        if (c.GetType() == typeof(Knight))
                        {
                            myFEN = myFEN + "N";
                        }
                        if (c.GetType() == typeof(Pawn))
                        {
                            myFEN = myFEN + "P";
                        }
                    }
                    else
                    {
                        if (c.GetType() == typeof(King))
                        {
                            myFEN = myFEN + "k";
                        }
                        if (c.GetType() == typeof(Queen))
                        {
                            myFEN = myFEN + "q";
                        }
                        if (c.GetType() == typeof(Rook))
                        {
                            myFEN = myFEN + "r";
                        }
                        if (c.GetType() == typeof(Bishop))
                        {
                            myFEN = myFEN + "b";
                        }
                        if (c.GetType() == typeof(Knight))
                        {
                            myFEN = myFEN + "n";
                        }
                        if (c.GetType() == typeof(Pawn))
                        {
                            myFEN = myFEN + "p";
                        }
                    }

                }
                else
                {
                    count++;

                }
            }
            //evaluate the count here.
            if (count > 0)
            {
                myFEN = myFEN + count;
            }
            if (j>0)
            {
                myFEN = myFEN + "/";//add the / to signify end of the current rank.
            }

            count = 0;
        }

        myFEN = myFEN + " ";
        if (isWhiteTurn)
        {
            myFEN = myFEN + "w ";
        }
        else
        {
            myFEN = myFEN + "b ";
        }

        if(whiteCanCastleShort)
        {
            myFEN = myFEN + "K";
        }
        if(whiteCanCastleLong)
        {
            myFEN = myFEN + "Q";
        }
        if (blackCanCastleShort)
        {
            myFEN = myFEN + "k";
        }
        if (blackCanCastleLong)
        {
            myFEN = myFEN + "q";
        }


        //add w or b depending on whose turn it is.

        //print the FEN here
        print(myFEN);

        return myFEN;
    }

    private void createMoveList()
    {
        String move = "";
        
        //1.e4 c6 2.d4 d5 3.e5 Bf5 4.Nf3 e6 5.c3 c5 6.Bb5+ Nd7 7.Be3 cxd4 8.Nxd4 Ne7 9.f4 a6 10.Ba4 g5 11.fxg5 b5 12.Bc2 Bxc2 13.Qxc2 Nxe5

        move = createMove(selectionX, selectionY, false, false, false, false);
        //if it was white, add the count number.
        if (!isWhiteTurn)
        {
            moveOrder = moveOrder + turnCount + "." + move + " ";
            blacksMove = VariationDatabase.VariationFinder(moveOrder);
            print("Blacks move is: " + blacksMove);
            //if (blacksMove == "")
            //{
            //    print("Resetting");
            //    EndGame();
            //}
            singlePlayer = true;//needs to be true for training mode.
        }
        else
        {
            moveOrder = moveOrder + move + " ";
            turnCount++;
        }

        //print the move list.
        //print(moveOrder);
        
        //test();


    }
    //This function creates the a single move notation.
    private String createMove(int x, int y, bool shortCastle, bool longCastle, bool check, bool mate)
    {
        String move = "";
        Chessman c = Chessmans[x, y];

        //choose the piece
        if (c.GetType() == typeof(King))
        {
            move = move + 'K';
        }
        if (c.GetType() == typeof(Queen))
        {
            move = move + 'Q';
        }
        if (c.GetType() == typeof(Rook))
        {
            move = move + 'R';
        }
        if (c.GetType() == typeof(Bishop))
        {
            move = move + 'B';
        }
        if (c.GetType() == typeof(Knight))
        {
            move = move + 'N';
        }
        if (c.GetType() == typeof(Pawn))
        {
                
        }

        //now add a capture
        if (capturedPiece)
        {
            if(c.GetType() == typeof(Pawn))
            {
                move = move + alphabetTranslator(beforeMoveX);
            }

            //if more than one of that piece can make that capture. Like if one of two rooks can make that capture.

            move = move + "x";
            capturedPiece = false;
        }

        //now add the rank
        move = move + alphabetTranslator(x);

        //now add the file
        y = y + 1;//add one to change it from 0-7 to 1-8
        move = move + y;


        //now add a check

        return move;
    }

    //This function translates an integer into its corresponding string value for the chessboard.
    String alphabetTranslator(int x)
    {
        String localString = "";

        if (x == 0)
        {
            localString = "a";
        }
        if (x == 1)
        {
            localString = "b";
        }
        if (x == 2)
        {
            localString = "c";
        }
        if (x == 3)
        {
            localString = "d";
        }
        if (x == 4)
        {
            localString = "e";
        }
        if (x == 5)
        {
            localString = "f";
        }
        if (x == 6)
        {
            localString = "g";
        }
        if (x == 7)
        {
            localString = "h";
        }

        return localString;
    }
    //This function translates an alphabet character into its corresponding integer for the chessboard.

    int alphabetReverser(String x)
    {
        int localInt = 0;

        if (x == "a")
        {
            localInt = 0;
        }
        if (x == "b")
        {
            localInt = 1;
        }
        if (x == "c")
        {
            localInt = 2;
        }
        if (x == "d")
        {
            localInt = 3;
        }
        if (x == "e")
        {
            localInt = 4;
        }
        if (x == "f")
        {
            localInt = 5;
        }
        if (x == "g")
        {
            localInt = 6;
        }
        if (x == "h")
        {
            localInt = 7;
        }

        return localInt;
    }

    //need to be able to provide the previous location like its being clicked before moving.
    //then translate the c4 move back into 0,4 coordinates using the split and alphabet reverser.

    //starting from blacksMove position, search the possible moves from that piece until you find the valid piece on the map.

    void noNameFunction()
    {
        int rank = 0;
        int file = 0;
        int piece = 0;
        bool capture = false;
        int doubleCheck = 0;

        rank = rankNumber();
        file = fileNumber();
        piece = pieceNumber();
        capture = captureCheck();
        

        //print("newRankNumber = " + rank);
        //print("newFileNumber = " + file);
        //print("newPieceNumber = " + piece);

        rankAfter = rank;
        fileAfter = file;

        //King      = 1;
        //Queen     = 2;
        //Rook      = 3;
        //Bishop    = 4;
        //Knight    = 5;
        //Pawn      = 6;

        //location after move is (5,2), location before move is (6,2)
        //rank 5, file 2
        //Chessman e = Chessmans[2, 6];

        if (piece == 6)//pawn
        {
            //explore territory away from original square

            //int[,] localArray = new int[8, 8];
            Chessman c = Chessmans[file, rank + 2];

            //if first move can move twice forward
            //if all moves can move once forward
            //can capture diagonally

            if (capture)
            {
                //if its a pawn it will look like this dxc3
                //if its a knight it will look like Nxd4
                //if there are multiple knights that can attack it will look like Ndxe4
            }

            //

            if (rank == 4)
            {
                //it could have been moved twice or just once.

                //Chessman c = Chessmans[file, rank + 2];

                if (c)
                {
                    if (c.GetType() == typeof(Pawn))
                    {
                        rankBefore = rank + 2;
                        fileBefore = file;
                    }
                }
                c = Chessmans[file, rank + 1];

                if (c)
                {
                    if (c.GetType() == typeof(Pawn))
                    {
                        rankBefore = rank + 1;
                        fileBefore = file;
                    }
                }
            }
            else
            {
                //only moved once
                c = Chessmans[file, rank + 1];

                if (c)
                {
                    if (c.GetType() == typeof(Pawn))
                    {
                        rankBefore = rank + 1;
                        fileBefore = file;
                    }
                }
                //Chessman e = Chessmans[2,6];
            }
            //if there was a capture

        }
        if (piece == 2)//Queen
        {
            bool found = false;
            int i = rank;//switched
            int j = file;

            Chessman c = Chessmans[j, i];

            // Top Left

            while (true)
            {
                i--;
                j++;
                if (i < 0 || j >= 8) break;

                c = Chessmans[j, i];
                if (c)
                {
                    if (c.GetType() == typeof(Queen))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                        print("found it going top left.");

                    }
                }
            }

            //Top Right
            i = rank;
            j = file;
            while (true)
            {
                i++;
                j++;
                if (i >= 8 || j >= 8) break;

                c = Chessmans[j, i];
                if (c)
                {
                    if (c.GetType() == typeof(Queen))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                        print("found it going top right.");

                    }
                }
            }

            //Bottom Left
            i = rank;
            j = file;
            while (true)
            {
                i--;
                j--;
                if (i < 0 || j < 0) break;

                c = Chessmans[j, i];
                if (c)
                {
                    if (c.GetType() == typeof(Queen))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                        print("found it going bottom left.");

                    }
                }

            }
            //Bottom Right
            i = rank;
            j = file;
            while (true)
            {
                i++;
                j--;
                if (i >= 8 || j < 0) break;

                c = Chessmans[j, i];
                if (c)
                {
                    if (c.GetType() == typeof(Queen))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                        print("found it going bottom left.");

                    }
                }


            }
            //Right
            i = rank;
            j = file;
            while (true)
            {
                i++;
                if (i >= 8) break;

                c = Chessmans[j, i];
                if (c)
                {
                    if (c.GetType() == typeof(Queen))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                        print("found it going bottom left.");

                    }
                }
            }
            //Left
            i = rank;
            j = file;
            while (true)
            {
                i--;
                if (i < 0) break;

                c = Chessmans[j, i];
                if (c)
                {
                    if (c.GetType() == typeof(Queen))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                        print("found it going left.");

                    }
                }
            }
            //Up
            i = rank;
            j = file;
            while (true)
            {
                j++;
                if (j >= 8) break;

                c = Chessmans[j, i];
                if (c)
                {
                    if (c.GetType() == typeof(Queen))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                        print("found it going up.");

                    }
                }
            }
            //Down
            i = rank;
            j = file;
            while (true)
            {
                j--;
                if (j <= 0) break;

                c = Chessmans[j, i];
                if (c)
                {
                    if (c.GetType() == typeof(Queen))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                        print("found it going down.");

                    }
                }
            }
        }

        if (piece == 3)//rook
        {
            bool found = false;
            int i = rank;//switched
            int j = file;

            Chessman c = Chessmans[j, i];

            // Right
            while (true)
            {
                i++;
                if (i >= 8) break;

                c = Chessmans[j, i];

                if (c)
                {
                    if (c.GetType() == typeof(Rook))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                    }
                }
                //found it going right.
            }

            // Left
            while (true)
            {
                i--;
                if (i < 0) break;

                c = Chessmans[j, i];

                if (c)
                {
                    if (c.GetType() == typeof(Rook))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                    }
                }
                //found it going right.
            }

            // Up
            while (true)
            {
                j++;
                if (j >= 8) break;

                c = Chessmans[j, i];

                if (c)
                {
                    if (c.GetType() == typeof(Rook))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                    }
                }
                //found it going right.
            }

            // Down
            while (true)
            {
                j++;
                if (j < 0) break;

                c = Chessmans[j, i];

                if (c)
                {
                    if (c.GetType() == typeof(Rook))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                    }
                }
                //found it going right.
            }

        }

        if (piece == 4)//bishop
        {
            bool found = false;
            int i = rank;//switched
            int j = file;

            Chessman c = Chessmans[j, i];

            //calc top left
            while (!found)
            {
                i--;
                j++;

                if (i < 0 || j >= 8)
                {
                    break;
                }
                c = Chessmans[j, i];

                if (c)
                {
                    if (c.GetType() == typeof(Bishop))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                    }
                }
            }

            i = rank;
            j = file;
            //calc top right
            while (!found)
            {
                i++;
                j++;

                if (i >= 8 || j >= 8)
                {
                    break;
                }
                c = Chessmans[j, i];

                if (c)
                {
                    if (c.GetType() == typeof(Bishop))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                    }
                }
            }

            i = rank;
            j = file;
            //calc down left
            while (!found)
            {
                i--;
                j--;

                if (i < 0 || j < 0)
                {
                    break;
                }
                c = Chessmans[j, i];

                if (c)
                {
                    if (c.GetType() == typeof(Bishop))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                    }
                }
            }

            i = rank;
            j = file;
            //calc down left
            while (!found)
            {
                i++;
                j--;

                if (i >= 8 || j < 0)
                {
                    break;
                }
                c = Chessmans[j, i];

                if (c)
                {
                    if (c.GetType() == typeof(Bishop))
                    {
                        fileBefore = j;
                        rankBefore = i;
                        found = true;
                    }
                }
            }
        }
        if (piece == 5)//knight
        {
            print("It's a knight");
            bool found = false;
            int i = rank;//switched
            int j = file;

            Chessman c = Chessmans[j, i];

            if (!found)
            {
                //calc up up left
                i = rank - 1;
                j = file + 2;
                if (!found && i >= 0 && j < 8)
                {
                    c = Chessmans[j, i];
                    if (c)
                    {
                        if (c.GetType() == typeof(Knight))
                        {
                            fileBefore = j;
                            rankBefore = i;
                            found = true;
                            print("Found up up left");
                        }
                    }
                }

                //calc up up right
                i = rank + 1;
                j = file + 2;
                if (!found && i < 8 && j < 8)
                {
                    c = Chessmans[j, i];
                    if (c)
                    {
                        if (c.GetType() == typeof(Knight))
                        {
                            fileBefore = j;
                            rankBefore = i;
                            found = true;
                            print("Found up up right");
                        }
                    }
                }

                //calc down down left
                i = rank - 1;
                j = file - 2;
                if (!found && i >= 0 && j >= 0)
                {
                    c = Chessmans[j, i];
                    if (c)
                    {
                        if (c.GetType() == typeof(Knight))
                        {
                            fileBefore = j;
                            rankBefore = i;
                            found = true;
                            print("Found down down left");
                        }
                    }
                }

                //calc down down right
                i = rank + 1;
                j = file - 2;
                if (!found && i < 8 && j >= 0)
                {
                    c = Chessmans[j, i];
                    if (c)
                    {
                        if (c.GetType() == typeof(Knight))
                        {
                            fileBefore = j;
                            rankBefore = i;
                            found = true;
                            print("Found down down right");
                        }
                    }
                }

                //calc left left up
                i = rank - 2;
                j = file + 1;
                if (!found && i >= 0 && j < 8)
                {
                    c = Chessmans[j, i];
                    if (c)
                    {
                        if (c.GetType() == typeof(Knight))
                        {
                            fileBefore = j;
                            rankBefore = i;
                            found = true;
                            print("Found left left up");
                        }
                    }
                }

                //calc right right up
                i = rank + 2;
                j = file + 1;
                if (!found && i < 8 && j < 8)
                {
                    c = Chessmans[j, i];
                    if (c)
                    {
                        if (c.GetType() == typeof(Knight))
                        {
                            fileBefore = j;
                            rankBefore = i;
                            found = true;
                            print("Found right right up");
                        }
                    }
                }

                //calc left left down
                i = rank - 2;
                j = file - 1;
                if (!found && i >= 0 && j >= 0)
                {
                    c = Chessmans[j, i];
                    if (c)
                    {
                        if (c.GetType() == typeof(Knight))
                        {
                            fileBefore = j;
                            rankBefore = i;
                            found = true;
                            print("Found left left down");
                        }
                    }
                }

                //calc right right down
                i = rank + 2;
                j = file - 1;
                if (!found && i < 8 && j >= 0)
                {
                    c = Chessmans[j, i];
                    if (c)
                    {
                        if (c.GetType() == typeof(Knight))
                        {
                            fileBefore = j;
                            rankBefore = i;
                            found = true;
                            print("Found right right down");
                        }
                    }
                }
            }
            else
            {
                print("problem here");
            }
        }
    }

    //this function returns the file number from the blacksmove.
    int fileNumber()
    {
        int number = 0;

        string localMove = blacksMove;

        for (int i = 0; i < localMove.Length; i++)
        {
            if ((localMove[i] == 'a'))
            {
                number = 0;
            }
            else if ((localMove[i] == 'b'))
            {
                number = 1;
            }
            else if ((localMove[i] == 'c'))
            {
                number = 2;
            }
            else if ((localMove[i] == 'd'))
            {
                number = 3;
            }
            else if ((localMove[i] == 'e'))
            {
                number = 4;
            }
            else if ((localMove[i] == 'f'))
            {
                number = 5;
            }
            else if ((localMove[i] == 'g'))
            {
                number = 6;
            }
            else if ((localMove[i] == 'h'))
            {
                number = 7;
            }

        }

        return number;
    }
    int rankNumber()
    {
        int number = 0;

        string localMove = blacksMove;

        for (int i = 0; i < localMove.Length; i++)
        {
            if ((localMove[i] == '1'))
            {
                number = 0;
            }
            else if ((localMove[i] == '2'))
            {
                number = 1;
            }
            else if ((localMove[i] == '3'))
            {
                number = 2;
            }
            else if ((localMove[i] == '4'))
            {
                number = 3;
            }
            else if ((localMove[i] == '5'))
            {
                number = 4;
            }
            else if ((localMove[i] == '6'))
            {
                number = 5;
            }
            else if ((localMove[i] == '7'))
            {
                number = 6;
            }
            else if ((localMove[i] == '8'))
            {
                number = 7;
            }

        }

        return number;
    }
    //
    int pieceNumber()
    {
        //King      = 1;
        //Queen     = 2;
        //Rook      = 3;
        //Bishop    = 4;
        //Knight    = 5;
        //Pawn      = 6;
        int number = 0;

        string localMove = blacksMove;

        for (int i = 0; i < localMove.Length; i++)
        {
            if ((localMove[i] == 'K'))
            {
                number = 1;
            }
            else if ((localMove[i] == 'Q'))
            {
                number = 2;
            }
            else if ((localMove[i] == 'R'))
            {
                number = 3;
            }
            else if ((localMove[i] == 'B'))
            {
                number = 4;
            }
            else if ((localMove[i] == 'N'))
            {
                number = 5;
            }

        }

        if (number == 0)
        {
            number = 6;

        }

        return number;
    }
    bool captureCheck()
    {
        bool capture = false;

        string localMove = blacksMove;

        for (int i = 0; i < localMove.Length; i++)
        {
            if ((localMove[i] == 'x'))
            {
                print("we have a capture");
                capture = true;
            }
        }

        return capture;
    }
    int doubleCheck()//this function checks if there are two checks at the same time.
    {
        return 0;
    }

}

//TODO
//1. reorder to be 1-8 instead of 0-7
//2. use same number for character codes as used for the creation of pieces.
//3. finish the allowable piece moves calculator.
//4. try to reuse some of the code!
//5. Fix castling
//6. input checks into the 
//7. erase blank ' ' inputs when selecting / deselecting. Also, if its an invalid move then kick it out.
//8. If the move can't be found, stop searching rather than infinitely looping to death.