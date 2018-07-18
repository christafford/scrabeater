using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Scrabeater
{
	public class Searcher
	{
		private static readonly int[] LetterValues = new [] {
		                                                    	1, // a
		                                                    	4, // b
		                                                    	4, // c
		                                                    	2, // d
		                                                    	1, // e
		                                                    	4, // f
		                                                    	3, // g
		                                                    	3, // h
		                                                    	1, // i
		                                                    	10, // j
		                                                    	5, // k
		                                                    	2, // l
		                                                    	4, // m
		                                                    	2, // n
		                                                    	1, // o
		                                                    	4, // p
		                                                    	10, // q
		                                                    	1, // r
		                                                    	1, // s
		                                                    	1, // t
		                                                    	2, // u
		                                                    	5, // v
		                                                    	4, // w
		                                                    	8, // x
		                                                    	3, // y
		                                                    	10, // z
		                                                    };

		public SpaceOnBoard[][] BoardSpots;
		private readonly List<string> _allCombos;
		private readonly int _x;
		private readonly int _y;
		private readonly List<WordPlace> _placements;
		private readonly HashSet<string> _realWords;
		private readonly bool _isNewBoard;

		public bool Done;

		public Searcher(Board board, List<string> allCombos, List<WordPlace> placements, HashSet<string> realWords, int x, int y)
		{
			BoardSpots = new JavaScriptSerializer().Deserialize<SpaceOnBoard[][]>(new JavaScriptSerializer().Serialize(board.BoardSpots));
			_allCombos = allCombos;
			_x = x;
			_y = y;
			_placements = placements;
			_realWords = new HashSet<string>();
			_isNewBoard = board.IsNewBoard;
			_realWords = realWords;
		}

		private int Width
		{
			get { return BoardSpots[0].Length; }
		}

		private int Height
		{
			get { return BoardSpots.Length; }
		}

		public void Run()
		{
			foreach (var combo in _allCombos)
			{
				var placed = Test(combo, true, _x, _y);

				if (placed != null)
					_placements.Add(new WordPlace
					{
						Across = true,
						Score = placed.Score,
						Word = placed.WordOnBoard,
						X = placed.X,
						Y = placed.Y
					});

					placed = Test(combo, false, _x, _y);

					if (placed != null)
						_placements.Add(new WordPlace
										{
										    Across = false,
										    Score = placed.Score,
										    Word = placed.WordOnBoard,
										    X = placed.X,
										    Y = placed.Y
										});
			}

			if (_y == 14)
				Console.Write(".");
			
			Done = true;
		}

		// returns null if invalid due to not-a-word or doesn't fit
		// places letters sequentially in specified direction, skipping 
		public PlacedLetters Test(string letters, bool across, int x, int y)
		{
			// rewind to start of word, if applicable
			if (across)
			{
				while (x > 0 && BoardSpots[y][x - 1].SpotType == SpaceOnBoard.Type.Used)
					x--;
			}
			else
			{
				while (y > 0 && BoardSpots[y - 1][x].SpotType == SpaceOnBoard.Type.Used)
					y--;
			}

			var currentX = x;
			var currentY = y;

			var score = 0;

			var currentLetterIndex = 0;
			var doubleWord = false;
			var tripleWord = false;
			var startUsed = false;
			
			var wordMade = "";

			var max = letters.Length;

			while (currentLetterIndex < max)
			{
				var spot = BoardSpots[currentY][currentX];

				if (spot.SpotType == SpaceOnBoard.Type.Used)
					score += LetterValues[(char) spot.Letter - 'A'];
				else
				{
					spot.Letter = letters[currentLetterIndex++];
					
					var letterVal = spot.Letter > 'Z' ? 0 : LetterValues[(char) spot.Letter - 'A'];
					
					if (spot.SpotType ==  SpaceOnBoard.Type.DoubleLetter)
						letterVal *= 2;
					
					else if (spot.SpotType ==  SpaceOnBoard.Type.TripleLetter)
						letterVal *= 3;

					else if (spot.SpotType == SpaceOnBoard.Type.DoubleWord)
						doubleWord = true;

					else if (spot.SpotType == SpaceOnBoard.Type.TripleWord)
						tripleWord = true;

					else if (spot.SpotType == SpaceOnBoard.Type.Start)
						startUsed = true;

					score += letterVal;
				}
				
				if (across)
					currentX++;
				else
					currentY++;

				if (currentY == Height || currentX == Width)
					return null;

				wordMade += spot.Letter;
			}

			// add on extra letters
			while (currentY < Height && currentX < Width && BoardSpots[currentY][currentX].SpotType == SpaceOnBoard.Type.Used)
			{
				score += LetterValues[(char) BoardSpots[currentY][currentX].Letter - 'A'];
				
				wordMade += BoardSpots[currentY][currentX].Letter;

				if (across)
					currentX++;
				else
					currentY++;
			}

			if (_isNewBoard && ! startUsed)
				return null;

			var adjacentRequired = false;
			var adjacentFound = false;

			if (wordMade.Length == letters.Length)
				adjacentRequired = true;

			if (! _realWords.Contains(wordMade.ToUpper()))
				return null;

			if (doubleWord)
				score *= 2;

			if (tripleWord)
				score *= 3;

			// go back and look in other direction for adjacent 
			currentX = x;
			currentY = y;

			for (int i = 0; i < wordMade.Length; i++)
			{
				// temporarily make our letter 'real' for the test
				var priorValue = BoardSpots[currentY][currentX].SpotType;

				// skip stuff already laid out
				if (priorValue != SpaceOnBoard.Type.Used)
				{
					BoardSpots[currentY][currentX].SpotType = SpaceOnBoard.Type.Used;

					var adjacentTest = TestAdjacent(currentX, currentY, ! across);

					BoardSpots[currentY][currentX].SpotType = priorValue;

					if (adjacentTest == null)
						return null;

					if (adjacentTest != 0)
					{
						adjacentFound = true;
						
						var adjSpot = BoardSpots[currentY][currentX];
						var letterVal = LetterValues[(char) (adjSpot.Letter > 'Z' ? adjSpot.Letter - 'a' : adjSpot.Letter - 'A')];

						if (adjSpot.SpotType == SpaceOnBoard.Type.DoubleLetter)
							adjacentTest += letterVal;

						else if (adjSpot.SpotType == SpaceOnBoard.Type.TripleLetter)
							adjacentTest += letterVal*2;

						if (adjSpot.SpotType == SpaceOnBoard.Type.DoubleWord)
							adjacentTest *= 2;

						else if (adjSpot.SpotType == SpaceOnBoard.Type.TripleWord)
							adjacentTest *= 3;

						score += (int) adjacentTest;
					}
				}

				if (across)
					currentX++;
				else
					currentY++;
			}

			if (adjacentRequired && ! adjacentFound)
				return null;

			return new PlacedLetters { Score = score, WordOnBoard = wordMade, X = x, Y = y };
		}

		private int? TestAdjacent(int x, int y, bool testAcross)
		{
			var letterFound = false;

			// anything to check?
			if (testAcross)
			{
				if (x > 0)
					letterFound = BoardSpots[y][x - 1].SpotType == SpaceOnBoard.Type.Used;

				if (x < Width - 1)
					letterFound |= BoardSpots[y][x + 1].SpotType == SpaceOnBoard.Type.Used;
			}
			else
			{
				if (y > 0)
					letterFound = BoardSpots[y - 1][x].SpotType == SpaceOnBoard.Type.Used;

				if (y < Height - 1)
					letterFound |= BoardSpots[y + 1][x].SpotType == SpaceOnBoard.Type.Used;
			}

			if (! letterFound)
				return 0;

			while (true)
			{
				if (testAcross)
				{
					if (x == 0)
						break;
					
					if (BoardSpots[y][x-1].SpotType != SpaceOnBoard.Type.Used)
						break;
					
					x--;
				}
				else
				{
					if (y == 0)
						break;
					
					if (BoardSpots[y - 1][x].SpotType != SpaceOnBoard.Type.Used)
						break;
					
					y--;
				}
			}

			var wordToTest = "";
			var score = 0;
			
			while (true)
			{
				if (BoardSpots[y][x].Letter <= 'Z')
					score += LetterValues[(char) BoardSpots[y][x].Letter - 'A'];
				
				wordToTest += BoardSpots[y][x].Letter;

				if (testAcross)
				{
					if (x == Width - 1)
						break;

					if (BoardSpots[y][x + 1].SpotType != SpaceOnBoard.Type.Used)
						break;
					x++;
				}
				else
				{
					if (y == Height - 1)
						break;

					if (BoardSpots[y+1][x].SpotType != SpaceOnBoard.Type.Used)
						break;
					y++;
				}
			}

			if (! _realWords.Contains(wordToTest.ToUpper()))
				return null;

			return score;
		}
	}
}
