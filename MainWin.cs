using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using System.Media;
using System.Diagnostics;
using GameStatistics;

/*
 * Primary class defines the partial class for the main window of the game
 * of Kaboooom (mine sweeper like).
 * 
 * Game is based on code written by A. Lane in Dr Dobbs Journal (DDJ) #163,
 * April 1990.  Code originally converted to Turbo Pascal on 1990-03-18, then
 * converted again to OS/2 Sibyl (Delphi-like Object Pascal) 1997-07-26.
 * Completed rewrite into C# on 2021-11-01.
 * 
 * Note: Cell draw routine is full of magic numbers.  Eventually would like
 * to redo to remove some of them...
 * 
 * Author: Michael G. Slack
 * Written: 2021-10-28
 * Version: 1.0.0.0
 * 
 * ----------------------------------------------------------------------------
 * 
 * Updated: yyyy-mm-dd - xxxxx.
 * 
 */

namespace Kaboooom
{
    public enum CellType { b_Empty, b_Visited, b_Bomb, b_Current, b_Finish, b_Exploded };
    
    public partial class MainWin : Form
    {
        #region Private constants
        private const int GRID_X = 15;
        private const int GRID_Y = 9;

        private const string HTML_HELP_FILE = "Kaboooom_help.html";

        private const string SOUND_NAMESPACE = "Kaboooom.sounds.";
        private const string SOUND_BEEP = "beep.wav";
        private const string SOUND_EXPLODE = "explode.wav";
        private const string SOUND_NOPE = "nope.wav";
        private const string SOUND_WELCOME = "welcome.wav";

        private const string REG_NAME = @"HKEY_CURRENT_USER\Software\Slack and Associates\Games\Kaboooom";
        private const string REG_KEY1 = "PosX";
        private const string REG_KEY2 = "PosY";
        private const string REG_KEY3 = "SafeGame";
        private const string REG_KEY4 = "SoundsOn";
        private const string REG_KEY5 = "VerboseMode";
        private const string REG_KEY6 = "RandomBombCount";
        private const string REG_KEY7 = "BombCount";
        private const string REG_KEY8 = "DebugMode";
        private const string REG_KEY9 = "Confirms";
        #endregion

        #region Internal structs
        struct AdjacencyGroup
        {
            public int BombCount, CellCount;
            public int[,] Cell;  // [0..8][0..1]
        }
        struct Rule3Tuple
        {
            public bool Processed;
            public int NewPLH;
        }
        #endregion

        #region Private variables
        private CellType[,] GameBoard = new CellType[GRID_X, GRID_Y];
        private bool[,] UserMarked = new bool[GRID_X, GRID_Y];
        private int UserX = 0, UserY = 0;
        private int NumberOfMines = 20;
        private bool SafeGame = true, SoundsOn = false, VerboseMove = false, RandomBomb = true;
        private bool DebugOn = false, ConfirmsOn = false;
        private bool I_Helped = false, ShowBombs = false, StopEvaluation = false;
        private AdjacencyGroup[,] AdjacentGroup = new AdjacencyGroup[GRID_X, GRID_Y];
        private Statistics stats = new Statistics(REG_NAME);
        private SoundPlayer beepSound = null;
        private bool bSoundLoaded = false;
        private SoundPlayer explodeSound = null;
        private bool eSoundLoaded = false;
        private SoundPlayer nopeSound = null;
        private bool nSoundLoaded = false;
        private SoundPlayer welcomeSound = null;
        private bool wSoundLoaded = false;
        #endregion

        // --------------------------------------------------------------------

        public MainWin()
        {
            InitializeComponent();
        }

        // --------------------------------------------------------------------

        #region Private Methods
        private void SetupRandomNumberOfMines()
        {
            Random rnd = new Random();

            NumberOfMines = rnd.Next(10, 41);  // 10 - 40
        }

        private void LoadRegistryValues()
        {
            int winX = -1, winY = -1, chk;

            try
            {
                winX = (int)Registry.GetValue(REG_NAME, REG_KEY1, winX);
                winY = (int)Registry.GetValue(REG_NAME, REG_KEY2, winY);
                chk = (int)Registry.GetValue(REG_NAME, REG_KEY3, 1);  // def = true
                SafeGame = chk > 0;
                chk = (int)Registry.GetValue(REG_NAME, REG_KEY4, 0);  // def = false
                SoundsOn = chk > 0;
                chk = (int)Registry.GetValue(REG_NAME, REG_KEY5, 0);  // def = false
                VerboseMove = chk > 0;
                chk = (int)Registry.GetValue(REG_NAME, REG_KEY6, 1);  // def = true
                RandomBomb = chk > 0;
                if (!RandomBomb) NumberOfMines = (int)Registry.GetValue(REG_NAME, REG_KEY7, 20);
                chk = (int)Registry.GetValue(REG_NAME, REG_KEY8, 0);  // def = false
                DebugOn = chk > 0;
                chk = (int)Registry.GetValue(REG_NAME, REG_KEY9, 0);  // def = false
                ConfirmsOn = chk > 0;
            }
            catch (Exception) { /* ignore, go with defaults, but could use MessageBox.Show(e.Message); */ }

            if ((winX != -1) && (winY != -1)) this.SetDesktopLocation(winX, winY);
        }

        private void WriteRegistryValues()
        {
            Registry.SetValue(REG_NAME, REG_KEY3, SafeGame, RegistryValueKind.DWord);
            Registry.SetValue(REG_NAME, REG_KEY4, SoundsOn, RegistryValueKind.DWord);
            Registry.SetValue(REG_NAME, REG_KEY5, VerboseMove, RegistryValueKind.DWord);
            Registry.SetValue(REG_NAME, REG_KEY6, RandomBomb, RegistryValueKind.DWord);
            Registry.SetValue(REG_NAME, REG_KEY7, NumberOfMines, RegistryValueKind.DWord);
            // Not a UI settable option, can only be set in the registry if needed.
            // Registry.SetValue(REG_NAME, REG_KEY8, DebugOn, RegistryValueKind.DWord);
            Registry.SetValue(REG_NAME, REG_KEY9, ConfirmsOn, RegistryValueKind.DWord);
        }

        private void SetupContextMenu()
        {
            ContextMenu mnu = new ContextMenu();
            MenuItem mnuStats = new MenuItem("Game Statistics");
            MenuItem sep = new MenuItem("-");
            MenuItem mnuHelp = new MenuItem("Help");
            MenuItem mnuAbout = new MenuItem("About");

            mnuStats.Click += new EventHandler(MnuStats_Click);
            mnuHelp.Click += new EventHandler(MnuHelp_Click);
            mnuAbout.Click += new EventHandler(MnuAbout_Click);
            mnu.MenuItems.AddRange(new MenuItem[] { mnuStats, sep, mnuHelp, mnuAbout });
            // putting context menu only on option display panel because need to be able to mark
            // cells in tablelayoutpanel using right click
            PnlOptDisplay.ContextMenu = mnu;
        }

        private Stream GetResourceStream(string path)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            return asm.GetManifestResourceStream(path);
        }

        private void LoadSoundPlayers()
        {
            string path = SOUND_NAMESPACE + SOUND_BEEP;

            try
            {
                beepSound = new SoundPlayer(GetResourceStream(path));
                bSoundLoaded = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Wav (" + path + "): " + e.Message, this.Text);
                beepSound = null;
            }

            path = SOUND_NAMESPACE + SOUND_EXPLODE;
            try
            {
                explodeSound = new SoundPlayer(GetResourceStream(path));
                eSoundLoaded = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Wav (" + path + "): " + e.Message, this.Text);
                explodeSound = null;
            }

            path = SOUND_NAMESPACE + SOUND_NOPE;
            try
            {
                nopeSound = new SoundPlayer(GetResourceStream(path));
                nSoundLoaded = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Wav (" + path + "): " + e.Message, this.Text);
                nopeSound = null;
            }

            path = SOUND_NAMESPACE + SOUND_WELCOME;
            try
            {
                welcomeSound = new SoundPlayer(GetResourceStream(path));
                wSoundLoaded = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Wav (" + path + "): " + e.Message, this.Text);
                welcomeSound = null;
            }
        }

        private void DisplayParams(bool showNumBombs)
        {
            string yn;

            if (showNumBombs)
            {
                if (RandomBomb) yn = " (randomized)"; else yn = "";
                lblNumBombs.Text = string.Format("There are {0} bombs in this game{1}.", NumberOfMines, yn);
            }
            if (SafeGame) yn = "ON"; else yn = "OFF";
            lblSafeMode.Text = string.Format("Safe game is {0}.", yn);
            if (SoundsOn) yn = "ON"; else yn = "OFF";
            lblSounds.Text = string.Format("Sounds are {0}.", yn);
            if (VerboseMove) yn = "ON"; else yn = "OFF";
            lblVerbose.Text = string.Format("Verbose evaulation move is {0}.", yn);
            if (ConfirmsOn) yn = "ON"; else yn = "OFF";
            lblConfirms.Text = string.Format("User confirmations are {0}.", yn);
        }

        private int CountMines(int x, int y)
        {
            int cnt = 0;

            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    if (x + i >= 0 && x + i < GRID_X && y + j >= 0 && y + j < GRID_Y)
                    {
                        if (GameBoard[x + i, y + j] == CellType.b_Bomb ||
                            GameBoard[x + i, y + j] == CellType.b_Exploded)
                        {
                            cnt++;
                        }
                    }
                }

            return cnt;
        }

        private void DisplayCell(int c, int r)
        {
            TableLayoutCellPaintEventArgs arg;
            Rectangle rect;
            int x = 1, y = 1; // assuming single line cell borders

            // get column start in pixels
            if (c > 0)
                for (int i = 0; i < c; i++) x += (int)tblMineField.GetColumnWidths()[i];
            // get row start in pixels
            if (r > 0)
                for (int i = 0; i < r; i++) y += (int)tblMineField.GetRowHeights()[i];
            // rect width/height assumes single line cell borders
            rect = new Rectangle(x, y, (int)tblMineField.GetColumnWidths()[c] - 1,
                (int)tblMineField.GetRowHeights()[r] - 1);
            arg = new TableLayoutCellPaintEventArgs(tblMineField.CreateGraphics(), rect, rect, c, r);
            TblMineField_CellPaint(this, arg);
        }

        private void StartNewGame()
        {
            if (RandomBomb) SetupRandomNumberOfMines();
            DisplayParams(true);
            SetupBoard(true);
            if (!DebugOn) stats.StartGame(true);
        }

        private void Travel(int dx, int dy)
        {
            int nx = UserX + dx, ny = UserY + dy, cnt;
            bool GameOver = false, InvFlg = false;
            string tt, sfg = "";
            DialogResult res;

            // check if outside mine field bounds
            if (nx < 0 || nx >= GRID_X) InvFlg = true;
            if (ny < 0 || ny >= GRID_Y) InvFlg = true;
            // if safegame on and grid marked, stop user from going there
            if (!InvFlg && SafeGame && UserMarked[nx, ny])
            {
                InvFlg = true; sfg = " (you must unmark it)";
            }
            if (InvFlg)
            { // bad move dude!
                if (SoundsOn && nSoundLoaded) nopeSound.PlaySync();
                tt = "*** Invalid move. ***" + sfg;
                MessageBox.Show(this, tt, this.Text, MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            else
            {
                GameBoard[UserX, UserY] = CellType.b_Visited;
                DisplayCell(UserX, UserY);
                if (GameBoard[nx, ny] == CellType.b_Bomb)
                { // :-) hit a bomb/mine
                    GameBoard[nx, ny] = CellType.b_Exploded; GameOver = true;
                    DisplayCell(nx, ny);
                    if (SoundsOn && eSoundLoaded) explodeSound.PlaySync();
                    MessageBox.Show(this, "KABOOOOM!, you've stepped on a mine.", this.Text,
                        MessageBoxButtons.OK, MessageBoxIcon.None);
                    if (!DebugOn) stats.GameLost(0);
                }
                else
                { // just move
                    UserX = nx; UserY = ny;
                    GameBoard[UserX, UserY] = CellType.b_Current;
                    DisplayCell(UserX, UserY);
                    if (UserX == GRID_X-1 && UserY == GRID_Y-1)
                    { // you've won
                        GameOver = true;
                        tt = "*** You have won!! ***";
                        if (I_Helped) tt += " (With help)";
                        if (SoundsOn && wSoundLoaded) welcomeSound.PlaySync();
                        MessageBox.Show(this, tt, this.Text, MessageBoxButtons.OK, MessageBoxIcon.None);
                        if (!DebugOn) stats.GameWon(0);
                    }
                }
            }
            cnt = CountMines(UserX, UserY);
            lblMsgLine.Text = string.Format("There are {0} bombs around you.", cnt);
            if (GameOver)
            {
                ShowBombs = true;
                tblMineField.Refresh();
                if (SoundsOn && bSoundLoaded) beepSound.PlaySync();
                res = MessageBox.Show(this, "GAME OVER: Start a new game?", this.Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.None);
                if (res == DialogResult.Yes)
                    StartNewGame();
                else
                    Close();
            }
        }

        private bool CheckTheThree(int x, int y, bool St)
        {
            int k = 0;

            if (GameBoard[x - 1, y] == CellType.b_Bomb) k++;
            if (GameBoard[x, y - 1] == CellType.b_Bomb) k++;
            if (St)
            {
                if (GameBoard[x, y] == CellType.b_Bomb) k++;
            }
            else
            {
                if (GameBoard[x - 1, y - 1] == CellType.b_Bomb) k++;
            }

            return k >= 2;  // 2 of 3 spots already have bombs, don't allow the third...
        }

        private void SetupBoard(bool NewGameFlg)
        {
            Random rnd = new Random();
            int x = 0, y = 0;
            bool bDone;

            for (int i = 0; i < GRID_X; i++)
                for (int j = 0; j < GRID_Y; j++)
                    UserMarked[i, j] = false;
            for (int i = 0; i < GRID_X; i++)
                for (int j = 0; j < GRID_Y; j++)
                    GameBoard[i, j] = CellType.b_Empty;
            UserX = 0; UserY = 0; ShowBombs = DebugOn; I_Helped = false;
            GameBoard[0, 0] = CellType.b_Current;
            GameBoard[GRID_X - 1, GRID_Y - 1] = CellType.b_Finish;
            // place mines
            for (int i = 0; i < NumberOfMines; i++)
            {
                bDone = false;
                while (!bDone)
                {
                    x = rnd.Next(0, GRID_X); y = rnd.Next(0, GRID_Y);
                    if (GameBoard[x, y] == CellType.b_Empty) bDone = true;
                    if (bDone)
                    {
                        if (x <= 1 && y <= 1) // check if placing bomb around start (don't surround start)
                        {
                            bDone = !CheckTheThree(1, 1, true);
                        }
                        else  // check to make sure not surrounding home either
                        {
                            bDone = !CheckTheThree(GRID_X - 1, GRID_Y - 1, false);
                        }
                    }
                }
                GameBoard[x, y] = CellType.b_Bomb;
            }
            if (NewGameFlg) tblMineField.Refresh();
            Travel(0, 0);
        }

        private void MarkUnmarkAtPos(int col, int row)
        {
            int flg = 0;
            string tt;

            if (col >= UserX - 1 && col <= UserX + 1 &&
                row >= UserY - 1 && row <= UserY + 1)
            {
                if (col == UserX && row == UserY)
                {
                    flg = 1;
                }
                else
                {
                    if (UserMarked[col, row])
                        UserMarked[col, row] = false;
                    else
                        UserMarked[col, row] = true;
                    DisplayCell(col, row);
                }
            }
            else
            {
                flg = 2;
            }
            if (flg > 0)
            { // error marking board
                if (SoundsOn && bSoundLoaded) beepSound.PlaySync();
                if (flg == 1)
                    tt = "Cannot mark current position.";
                else
                    tt = "Can only mark cells around your position.";
                MessageBox.Show(this, tt, this.Text, MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        // Method adapted from: stackoverflow.com/a/15449969
        // - found at: https://stackoverflow.com/questions/15449504/how-do-i-determine-the-cell-being-clicked-on-in-a-tablelayoutpanel
        // - answered Jan 30 '17 at 12:03 by Peter Gordon
        // retuns the col/row index of a cell clicked on in a TableLayoutPanel
        private Point? GetCellIndex(TableLayoutPanel tlp, Point point)
        {
            if (point.X > tlp.Width || point.Y > tlp.Height)
                return null;

            int w = 0, h = 0;
            int[] widths = tlp.GetColumnWidths(), heights = tlp.GetRowHeights();

            int i;
            for (i = 0; i < widths.Length && point.X > w; i++)
            {
                w += widths[i];
            }
            int col = i - 1;

            for (i = 0; i < heights.Length && point.Y + tlp.VerticalScroll.Value > h; i++)
            {
                h += heights[i];
            }
            int row = i - 1;

            return new Point(col, row);
        }
        #endregion

        // --------------------------------------------------------------------

        #region Private Evaluation Methods
        private DialogResult DisplayMessage(string msg, MessageBoxButtons btns)
        {
            string tt = this.Text + " (evaluating)";

            if (SoundsOn && bSoundLoaded) beepSound.PlaySync();
            return MessageBox.Show(this, msg, tt, btns, MessageBoxIcon.None);
        }

        private void ComputeAdjacency(int x, int y)
        {
            int bCnt, cel;

            Application.DoEvents();
            if (!StopEvaluation && x >= 0 && x < GRID_X && y >= 0 && y < GRID_Y)
            {
                if (GameBoard[x, y] == CellType.b_Visited ||
                    GameBoard[x, y] == CellType.b_Current)
                {
                    bCnt = CountMines(x, y); cel = 0;
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            Application.DoEvents();
                            if (StopEvaluation) { dx = 2; dy = 2; } // break;
                            else
                            {
                                if (!(dx == 0 && dy == 0))
                                {
                                    if (x+dx >= 0 && x+dx < GRID_X &&
                                        y+dy >= 0 && y+dy < GRID_Y)
                                    {
                                        if (GameBoard[x+dx, y+dy] != CellType.b_Visited &&
                                            GameBoard[x+dx, y+dy] != CellType.b_Current)
                                        {
                                            if (UserMarked[x + dx, y + dy])
                                                bCnt--;
                                            else
                                            {
                                                AdjacentGroup[x, y].Cell[cel, 0] = x + dx;
                                                AdjacentGroup[x, y].Cell[cel, 1] = y + dy;
                                                cel++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    AdjacentGroup[x, y].BombCount = bCnt;
                    AdjacentGroup[x, y].CellCount = cel;
                }
                else
                { // don't need to compute
                    AdjacentGroup[x, y].CellCount = 0;
                    AdjacentGroup[x, y].BombCount = -1;
                }
            }
            Application.DoEvents();
        }

        private void MarkBombCell(int x, int y)
        {
            UserMarked[x, y] = true;
            DisplayCell(x, y);
            if (GameBoard[x, y] != CellType.b_Bomb)
            { // marked in error!!!
                if (VerboseMove)
                {
                    string tt = "LOGIC ERROR: phantom bomb tagged at " + x + ", " + y +
                        ".  Cancel to stop evaluation.";

                    if (DisplayMessage(tt, MessageBoxButtons.OKCancel) != DialogResult.OK)
                        StopEvaluation = true;
                }
                UserMarked[x, y] = false;
                DisplayCell(x, y);
            }
            Application.DoEvents();
        }

        private int AddToPositionList(int[,] pl, int plh, int x, int y)
        {
            bool foundF = false;
            int nIndex = 0;

            ComputeAdjacency(x, y);
            while (nIndex < plh && !foundF)
            {
                if (pl[nIndex, 0] == x && pl[nIndex, 1] == y) foundF = true;
                nIndex++;
            }
            if (!foundF)
            {
                pl[plh, 0] = x; pl[plh, 1] = y; plh++;
            }
            if (plh > (GRID_X * GRID_Y) && VerboseMove)
            {
                if (DisplayMessage("ERROR: PLH > Maximum PLH!  Cancel to stop evaluation.",
                    MessageBoxButtons.OKCancel) != DialogResult.OK)
                    StopEvaluation = true;
            }
            Application.DoEvents();

            return plh;
        }

        private int AddSurroundingToPositionList(int[,] pl, int plh, int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (x+dx >= 0 && x+dx < GRID_X && y+dy >= 0 && y+dy < GRID_Y)
                    {
                        if (GameBoard[x + dx, y + dy] == CellType.b_Visited ||
                            GameBoard[x + dx, y + dy] == CellType.b_Current)
                            plh = AddToPositionList(pl, plh, x + dx, y + dy);
                    }
                }
            Application.DoEvents();

            return plh;
        }

        private void VisitCell(int x, int y)
        {
            if (GameBoard[x, y] != CellType.b_Current)
            {
                if (GameBoard[x, y] == CellType.b_Bomb)
                { // whoops, walked on a bomb
                    if (VerboseMove)
                    {
                        string tt = "OUCH! walked on a bomb at " + x + ", " + y +
                            ".  Cancel to stop evaluation.";

                        if (DisplayMessage(tt, MessageBoxButtons.OKCancel) != DialogResult.OK)
                            StopEvaluation = true;
                    }
                }
                else
                {
                    GameBoard[x, y] = CellType.b_Visited;
                    DisplayCell(x, y);
                }
            }
            Application.DoEvents();
        }

        private bool FindPositionInAG(AdjacencyGroup pag, int x, int y)
        {
            bool foundF = false;

            for (int i = 0; i < pag.CellCount; i++)
                if (pag.Cell[i, 0] == x && pag.Cell[i, 1] == y) foundF = true;

            return foundF;
        }

        private int CountCommonCells(AdjacencyGroup pag1, AdjacencyGroup pag2)
        {
            int cnt = 0;

            for (int cel = 0; cel < pag1.CellCount; cel++)
                if (FindPositionInAG(pag2, pag1.Cell[cel, 0], pag1.Cell[cel, 1]))
                    cnt++;
            Application.DoEvents();

            return cnt;
        }

        private Rule3Tuple ProcessRule3(AdjacencyGroup curAG, AdjacencyGroup tempAG,
            int[,] pl, int plh)
        {
            int bCnt, cCnt, cHoldHead;
            int[,] cellHolder = new int[10, 2];
            Rule3Tuple retVal = new Rule3Tuple { Processed = false, NewPLH = plh };

            bCnt = curAG.BombCount; cCnt = curAG.CellCount;
            tempAG.CellCount = CountCommonCells(tempAG, curAG);
            if (tempAG.CellCount > 0)
            {
                bCnt -= tempAG.BombCount; cCnt -= tempAG.CellCount;
                if (cCnt > 0 && (bCnt == cCnt || bCnt == 0))
                {
                    retVal.Processed = true; cHoldHead = 0; cCnt = curAG.CellCount;
                    for (int x = 0; x < cCnt; x++)
                        if (!FindPositionInAG(tempAG, curAG.Cell[x, 0], curAG.Cell[x, 1]))
                        {
                            if (bCnt == 0)
                                VisitCell(curAG.Cell[x, 0], curAG.Cell[x, 1]);
                            else
                                MarkBombCell(curAG.Cell[x, 0], curAG.Cell[x, 1]);
                            cellHolder[cHoldHead, 0] = curAG.Cell[x, 0];
                            cellHolder[cHoldHead, 1] = curAG.Cell[x, 1];
                            cHoldHead++;
                        }
                    Application.DoEvents();
                    for (int x = 0; x < cHoldHead; x++)
                        retVal.NewPLH = AddSurroundingToPositionList(pl, retVal.NewPLH,
                            cellHolder[x, 0], cellHolder[x, 1]);
                }
            }

            return retVal;
        }

        private void Do_Eval()
        {
            bool Modified = true;
            int PositionListHead = 1, dx, dy, currentX, currentY, bCnt, cCnt;
            int[,] PositionList = new int[GRID_X * GRID_Y, 2];
            AdjacencyGroup tempAG;

            I_Helped = true; tempAG.Cell = new int[9, 2];
            PositionList[0, 0] = UserX; PositionList[0, 1] = UserY;
            for (int x = 0; x < GRID_X; x++)
                for (int y = 0; y < GRID_Y; y++)
                    ComputeAdjacency(x, y);
            Application.DoEvents();
            while (Modified && !StopEvaluation)
            {
                Modified = false;
                while (PositionListHead > 0 && !StopEvaluation)
                {
                    currentX = PositionList[0, 0]; currentY = PositionList[0, 1];
                    for (int x = 0; x < PositionListHead-1; x++)
                    { // move back
                        PositionList[x, 0] = PositionList[x + 1, 0];
                        PositionList[x, 1] = PositionList[x + 1, 1];
                    }
                    PositionListHead--;
                    ComputeAdjacency(currentX, currentY);
                    bCnt = AdjacentGroup[currentX, currentY].BombCount;
                    cCnt = AdjacentGroup[currentX, currentY].CellCount;
                    if (cCnt > 0 && bCnt > -1)
                    { // run rules
                        if (cCnt == bCnt)
                        { // rule 1: if number of bombs = number of cells, all are bombs
                            for (int cel = 0; cel < cCnt; cel++)
                            {
                                dx = AdjacentGroup[currentX, currentY].Cell[cel, 0];
                                dy = AdjacentGroup[currentX, currentY].Cell[cel, 1];
                                MarkBombCell(dx, dy);
                                if (StopEvaluation) { cel = cCnt; } // break
                                Modified = true;
                                PositionListHead = AddSurroundingToPositionList(PositionList,
                                    PositionListHead, dx, dy);
                            }
                        }
                        else if (cCnt > 0 && bCnt == 0)
                        { // rule 2: # bombs = 0, all cells ok
                            for (int cel = 0; cel <= cCnt; cel++)
                            {
                                dx = AdjacentGroup[currentX, currentY].Cell[cel, 0];
                                dy = AdjacentGroup[currentX, currentY].Cell[cel, 1];
                                VisitCell(dx, dy);
                                if (StopEvaluation) { cel = cCnt + 1; } // break
                                Modified = true;
                                PositionListHead = AddToPositionList(PositionList,
                                    PositionListHead, dx, dy);
                                PositionListHead = AddSurroundingToPositionList(PositionList,
                                    PositionListHead, dx, dy);
                            }
                        }
                        else
                        { // rule 3: if AGs overlap, subtract and ck rule 1 and 2
                            bool DoneF = false; int cel = 0;
                            while (cel < cCnt && !DoneF)
                            {
                                Application.DoEvents();
                                if (!StopEvaluation)
                                {
                                    int x = AdjacentGroup[currentX, currentY].Cell[cel, 0];
                                    int y = AdjacentGroup[currentX, currentY].Cell[cel, 1];
                                    dx = -1;
                                    while (dx <= 1 && !DoneF)
                                    {
                                        dy = -1;
                                        while (dy <= 1 && !DoneF)
                                        {
                                            if (x+dx >= 0 && x+dx < GRID_X && y+dy >= 0 && y+dy < GRID_Y)
                                            {
                                                tempAG = AdjacentGroup[x + dx, y + dy];
                                                if (tempAG.BombCount > 0)
                                                {
                                                    Rule3Tuple ret = ProcessRule3(AdjacentGroup[currentX, currentY],
                                                        tempAG, PositionList, PositionListHead);
                                                    DoneF = ret.Processed;
                                                    PositionListHead = ret.NewPLH;
                                                    if (DoneF) Modified = true;
                                                    if (StopEvaluation) { DoneF = true; } // break
                                                }
                                                AdjacentGroup[x + dx, y + dy] = tempAG;
                                            }
                                            dy++;
                                        }
                                        dx++;
                                    }
                                }
                                cel++;
                            }
                        }
                    }
                }
                if (Modified && !StopEvaluation)
                {
                    for (int x = 0; x < GRID_X; x++)
                        for (int y = 0; y < GRID_Y; y++)
                        {
                            Application.DoEvents();
                            if (StopEvaluation) { x = GRID_X; y = GRID_Y; } // break out of loops
                            else
                            {
                                if (GameBoard[x, y] == CellType.b_Visited ||
                                    GameBoard[x, y] == CellType.b_Current)
                                {
                                    PositionListHead = AddToPositionList(PositionList,
                                        PositionListHead, x, y);
                                }
                            }
                        }
                }
            }
            GameBoard[GRID_X - 1, GRID_Y - 1] = CellType.b_Finish;
            DisplayCell(GRID_X - 1, GRID_Y - 1);
            if (VerboseMove)
                DisplayMessage("Cannot deduce anything further, you're on your own.",
                    MessageBoxButtons.OK);
        }

        private void EnabDisButtons(bool Enab)
        {
            btnCancel.Visible = !Enab; btnCancel.Enabled = !Enab;
            btnUL.Enabled = Enab; btnUp.Enabled = Enab; btnUR.Enabled = Enab;
            btnLeft.Enabled = Enab; btnSolve.Enabled = Enab; btnRight.Enabled = Enab;
            btnDL.Enabled = Enab; btnDown.Enabled = Enab; btnDR.Enabled = Enab;
            btnNew.Enabled = Enab; btnOptions.Enabled = Enab; btnQuit.Enabled = Enab;
        }

        private void EvaluatePosition()
        {
            DialogResult res = DialogResult.Yes;

            if (ConfirmsOn)
            {
                if (SoundsOn && bSoundLoaded) beepSound.PlaySync();
                res = MessageBox.Show(this, "Are you sure you want the computer to evaluate?", this.Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.None);
            }
            if (res == DialogResult.Yes)
            {
                StopEvaluation = false;
                EnabDisButtons(false);
                lblMsgLine.Text = "Evaluating position, press cancel to abort.";
                Application.DoEvents();
                Do_Eval();
                EnabDisButtons(true);
                Travel(0, 0);  // reset msgline and position
            }
        }
        #endregion

        // --------------------------------------------------------------------

        #region Event Handlers
        private void MainWin_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < GRID_X; i++)
                for (int j = 0; j < GRID_Y; j++)
                    AdjacentGroup[i, j].Cell = new int[9, 2];
            SetupRandomNumberOfMines();
            LoadRegistryValues();
            SetupContextMenu();
            LoadSoundPlayers();
            DisplayParams(true);
            SetupBoard(false);
            stats.GameName = this.Text;
            if (!DebugOn) stats.StartGame(false);
        }

        private void MainWin_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                Registry.SetValue(REG_NAME, REG_KEY1, this.Location.X);
                Registry.SetValue(REG_NAME, REG_KEY2, this.Location.Y);
            }
        }

        private void MainWin_KeyUp(object sender, KeyEventArgs e)
        {
            int K = e.KeyValue;

            // change to set key value if using numpad w/o numlock
            switch (e.KeyCode)
            {
                case Keys.Home:
                    K = 103; break;
                case Keys.Up:
                    K = 104; break;
                case Keys.PageUp:
                    K = 105; break;
                case Keys.Left:
                    K = 100; break;
                case Keys.Clear:
                    K = 101; break;
                case Keys.Right:
                    K = 102; break;
                case Keys.End:
                    K = 97; break;
                case Keys.Down:
                    K = 98; break;
                case Keys.PageDown:
                    K = 99; break;
                default:
                    break;
            }

            if (K >= 97 && K <= 105)  // keypad 1 - 9
            {
                K -= 96; // 1 - 9
                e.Handled = true;
                switch (K)
                {
                    case 1:
                        btnDL.Focus();
                        if (e.Control)
                            MarkUnmarkAtPos(UserX - 1, UserY + 1);
                        else
                            BtnDL_Click(sender, EventArgs.Empty);
                        break;
                    case 2:
                        btnDown.Focus();
                        if (e.Control)
                            MarkUnmarkAtPos(UserX, UserY + 1);
                        else
                            BtnDown_Click(sender, EventArgs.Empty);
                        break;
                    case 3:
                        btnDR.Focus();
                        if (e.Control)
                            MarkUnmarkAtPos(UserX + 1, UserY + 1);
                        else
                            BtnDR_Click(sender, EventArgs.Empty);
                        break;
                    case 4:
                        btnLeft.Focus();
                        if (e.Control)
                            MarkUnmarkAtPos(UserX - 1, UserY);
                        else
                            BtnLeft_Click(sender, EventArgs.Empty);
                        break;
                    case 5:
                        btnSolve.Focus();
                        BtnSolve_Click(sender, EventArgs.Empty);
                        break;
                    case 6:
                        btnRight.Focus();
                        if (e.Control)
                            MarkUnmarkAtPos(UserX + 1, UserY);
                        else
                            BtnRight_Click(sender, EventArgs.Empty);
                        break;
                    case 7:
                        btnUL.Focus();
                        if (e.Control)
                            MarkUnmarkAtPos(UserX - 1, UserY - 1);
                        else
                            BtnUL_Click(sender, EventArgs.Empty);
                        break;
                    case 8:
                        btnUp.Focus();
                        if (e.Control)
                            MarkUnmarkAtPos(UserX, UserY - 1);
                        else
                            BtnUp_Click(sender, EventArgs.Empty);
                        break;
                    case 9:
                        btnUR.Focus();
                        if (e.Control)
                            MarkUnmarkAtPos(UserX + 1, UserY - 1);
                        else
                            BtnUR_Click(sender, EventArgs.Empty);
                        break;
                    default:
                        e.Handled = false;
                        break;
                }
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            DialogResult res = DialogResult.Yes;

            if (ConfirmsOn)
            {
                if (SoundsOn && bSoundLoaded) beepSound.PlaySync();
                res = MessageBox.Show(this, "Start new game (quit game in progress)?", this.Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.None);
            }

            if (res == DialogResult.Yes)
            {
                if (!DebugOn) stats.GameDone();
                StartNewGame();
            }
        }

        private void BtnOptions_Click(object sender, EventArgs e)
        {
            OptionsDlg opts = new OptionsDlg();

            opts.SafeGame = SafeGame;
            opts.SoundsOn = SoundsOn;
            opts.VerboseMove = VerboseMove;
            opts.RandomBombs = RandomBomb;
            opts.NumberOfBombs = NumberOfMines;
            opts.Confirmations = ConfirmsOn;

            if (opts.ShowDialog(this) == DialogResult.OK)
            {
                SafeGame = opts.SafeGame;
                SoundsOn = opts.SoundsOn;
                VerboseMove = opts.VerboseMove;
                RandomBomb = opts.RandomBombs;
                if (!RandomBomb) NumberOfMines = opts.NumberOfBombs;
                ConfirmsOn = opts.Confirmations;
                WriteRegistryValues();
                DisplayParams(false);
            }

            opts.Dispose();
        }

        private void BtnQuit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            StopEvaluation = true;
        }

        private void BtnUL_Click(object sender, EventArgs e)
        {
            Travel(-1, -1);
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {
            Travel(0, -1);
        }

        private void BtnUR_Click(object sender, EventArgs e)
        {
            Travel(1, -1);
        }

        private void BtnLeft_Click(object sender, EventArgs e)
        {
            Travel(-1, 0);
        }

        private void BtnRight_Click(object sender, EventArgs e)
        {
            Travel(1, 0);
        }

        private void BtnDL_Click(object sender, EventArgs e)
        {
            Travel(-1, 1);
        }

        private void BtnDown_Click(object sender, EventArgs e)
        {
            Travel(0, 1);
        }

        private void BtnDR_Click(object sender, EventArgs e)
        {
            Travel(1, 1);
        }
        private void BtnSolve_Click(object sender, EventArgs e)
        {
            EvaluatePosition();
        }

        private void TblMineField_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            { // right click on table
                Point cell = (Point)GetCellIndex((TableLayoutPanel)sender, e.Location);

                if (cell != null)
                { // have cell index (col, row)
                    MarkUnmarkAtPos(cell.X, cell.Y);
                }
            }
        }

        private void TblMineField_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.FillRectangle(new SolidBrush(tblMineField.BackColor), e.CellBounds);
            if (UserMarked[e.Column, e.Row])
            {
                g.DrawImage(imgsMineField.Images[14], e.CellBounds.X + 1, e.CellBounds.Y + 5);
                g.DrawImage(imgsMineField.Images[14], e.CellBounds.X + 24, e.CellBounds.Y + 5);
            }
            switch (GameBoard[e.Column, e.Row])
            {
                case CellType.b_Empty:
                    break;
                case CellType.b_Visited:
                    int c = CountMines(e.Column, e.Row);
                    g.DrawImage(imgsMineField.Images[c], e.CellBounds.X + 12, e.CellBounds.Y + 5);
                    break;
                case CellType.b_Bomb:
                    if (ShowBombs)
                        g.DrawImage(imgsMineField.Images[10], e.CellBounds.X + 12, e.CellBounds.Y + 5);
                    break;
                case CellType.b_Current:
                    g.DrawImage(imgsMineField.Images[11], e.CellBounds.X + 12, e.CellBounds.Y + 5);
                    break;
                case CellType.b_Finish:
                    g.DrawImage(imgsMineField.Images[13], e.CellBounds.X + 12, e.CellBounds.Y + 5);
                    break;
                case CellType.b_Exploded:
                    g.DrawImage(imgsMineField.Images[12], e.CellBounds.X + 12, e.CellBounds.Y + 5);
                    break;
                default:
                    break;
            }
        }

        private void MnuStats_Click(object sender, EventArgs e)
        {
            stats.ShowStatistics(this);
        }

        private void MnuHelp_Click(object sender, EventArgs e)
        {
            var asm = Assembly.GetEntryAssembly();
            var asmLocation = Path.GetDirectoryName(asm.Location);
            var htmlPath = Path.Combine(asmLocation, HTML_HELP_FILE);

            try
            {
                Process.Start(htmlPath);
            }
            catch (Exception ex)
            {
                if (SoundsOn && bSoundLoaded) beepSound.PlaySync();
                MessageBox.Show(this, "Cannot load help: " + ex.Message, this.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        private void MnuAbout_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();

            about.ShowDialog(this);
            about.Dispose();
        }
        #endregion
    }
}
