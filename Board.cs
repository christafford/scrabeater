using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Scrabeater
{
	public class Board
	{
		// 1 = dw
		// 2 = tw
		// 3 = dl
		// 4 = tl
		// 5 = start
		private static readonly string[] WordsmithBoard = {
			"2  3   2   3  2",
			"  1   3 3   1  ",
			" 1   1   1   1 ",
			"3   4  3  4   3",
			"   1       1   ",
			"  4   4 4   4  ",
			" 3           3 ",
			"2      5      2",
			" 3  1     1  3 ",
			"  4   4 4   4  ",
			"   1       1   ",
			"3   4  3  4   3",
			" 1   1   1   1 ",
			"  1   3 3   1  ",
			"2  3   2   3  2",
		};

		private static readonly string[] WordsWithFriendsBoard = {
			"   2  4 4  2   ",
			"  3  1   1  3  ",
			" 3  3     3  3 ",
			"2  4   1   4  2",
			"  3   3 3   3  ",
			" 1   4   4   1 ",
			"4   3     3   4",
			"   1   5   1   ",
			"4   3     3   4",
			" 1   4   4   1 ",
			"  3   3 3   3  ",
			"2  4   1   4  2",
			" 3  3     3  3 ",
			"  3  1   1  3  ",
			"   2  4 4  2   "
		};

		public enum BoardType { WordsWithFriends, Wordsmith };
	
		public bool IsNewBoard = true;

		public SpaceOnBoard[][] BoardSpots;
		public SpaceOnBoard[][] BlankBoard;

		public int Width
		{
			get { return BoardSpots[0].Length; }
		}

		public int Height
		{
			get { return BoardSpots.Length; }
		}

		public Board() { }

		public Board(BoardType board)
		{
			string[] layout = null;

			switch (board)
			{
				case BoardType.WordsWithFriends:
					layout = WordsWithFriendsBoard;
					break;
				case BoardType.Wordsmith:
					layout = WordsmithBoard;
					break;
			}

			BoardSpots = new SpaceOnBoard[layout.Length][];
			for (int y = 0; y < layout.Length; y++)
			{
				BoardSpots[y] = new SpaceOnBoard[layout[y].Length];
				for (int x = 0; x < layout[y].Length; x++)
				{
					BoardSpots[y][x] = new SpaceOnBoard();

					switch (layout[y][x])
					{
						case '1':
							BoardSpots[y][x].SpotType = SpaceOnBoard.Type.DoubleWord;
							break;
						
						case '2':
							BoardSpots[y][x].SpotType = SpaceOnBoard.Type.TripleWord;
							break;
					
						case '3':
							BoardSpots[y][x].SpotType = SpaceOnBoard.Type.DoubleLetter;
							break;
					
						case '4':
							BoardSpots[y][x].SpotType = SpaceOnBoard.Type.TripleLetter;
							break;

						case '5':
							BoardSpots[y][x].SpotType = SpaceOnBoard.Type.Start;
							break;

						default:
							BoardSpots[y][x].SpotType = SpaceOnBoard.Type.Nothing;
							break;
					}
				}
			}

			// stupid, but it works
			BlankBoard = new JavaScriptSerializer().Deserialize<SpaceOnBoard[][]>(new JavaScriptSerializer().Serialize(BoardSpots));
		}

		public void Print()
		{
			Console.WriteLine("\n.=BLANK, 1=DW, 2=TW, 3=DL, 4=TL, 5=START\n");

			for (int y = 0; y < Height; y++)
			{
				if (y < 9)
					Console.Write(" ");

				Console.Write((y + 1) + ": ");

				for (int x = 0; x < Width; x++)
				{
					switch (BoardSpots[y][x].SpotType)
					{
						case SpaceOnBoard.Type.Nothing:
							Console.Write(".");
							break;
						case SpaceOnBoard.Type.DoubleWord:
							Console.Write("1");
							break;
						case SpaceOnBoard.Type.TripleWord:
							Console.Write("2");
							break;
						case SpaceOnBoard.Type.DoubleLetter:
							Console.Write("3");
							break;
						case SpaceOnBoard.Type.TripleLetter:
							Console.Write("4");
							break;
						case SpaceOnBoard.Type.Start:
							Console.Write("5");
							break;
						default:
							Console.Write(BoardSpots[y][x].Letter);
							break;
					}
				}

				Console.WriteLine();
			}
		}


		// return false if not allowed
		public bool Place(string word, bool across, int x, int y)
		{
			if (across && x + word.Length > Width)
				return false;

			if (! across && y + word.Length > Height)
				return false;

			for (int i = 0; i < word.Length; i++)
			{
				if (word[i] == '.')
					BoardSpots[y][x].SpotType = BlankBoard[y][x].SpotType;
				else
				{
					// don't actually change the type to used until after we test everything
					if (BoardSpots[y][x].SpotType != SpaceOnBoard.Type.Used)
						BoardSpots[y][x].Letter = word[i];

					else if (BoardSpots[y][x].Letter != word[i])
						return false;
				}
				if (across)
					x++;
				else
					y++;
			}

			// passed test - set spots to used now
			foreach (var t in word)
			{
				if (across)
					x--;
				else
					y--;

				if (t >= 'A' && t <= 'Z')
					BoardSpots[y][x].SpotType = SpaceOnBoard.Type.Used;
			}
			
			IsNewBoard = false;

			return true;
		}
	}
}