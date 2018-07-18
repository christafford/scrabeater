using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace Scrabeater
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var realWords = new HashSet<string>();
			var everything = File.ReadAllText("..\\..\\AllWords.txt").Replace("\r", "");

			foreach (var word in everything.Split('\n'))
				realWords.Add(word);

			Console.WriteLine("1 - Start New Game");
			Console.WriteLine("2 - Restore Saved Game");

			Console.Write("\nEnter choice: ");
			var input = Console.ReadLine();
			
			string file;
			Board board;

			switch (input)
			{
				case "1":
					Console.WriteLine("1 - Words With Friends");
					Console.WriteLine("2 - Wordsmith");
					Console.Write("\nEnter choice: ");
					input = Console.ReadLine();
					
					if (input == "1")
						board = new Board(Board.BoardType.WordsWithFriends);
					else if (input == "2")
						board = new Board(Board.BoardType.Wordsmith);
					else
						return;

					Console.Write("Enter filename: ");
					file = Console.ReadLine();
					break;
				
				case "2":
					Console.Write("Enter filename: ");
					file = Console.ReadLine();
					board = new JavaScriptSerializer().Deserialize<Board>(File.ReadAllText("..\\..\\" + file));
					break;
				
				default:
					return;
			}

			while (true)
			{
				File.WriteAllText("..\\..\\" + file, new JavaScriptSerializer().Serialize(board));

				board.Print();

				try
				{
					Console.WriteLine("\n1 - Place word");
					Console.WriteLine("2 - Find words");
					Console.Write("\nEnter choice: ");
					var argument = Console.ReadLine();

					if (argument != "1" && argument != "2" && argument != "3" && argument != "4")
						continue;

					if (argument == "1")
					{
						Console.Write("Enter word: ");
						var word = Console.ReadLine().ToUpper();

						Console.Write("Enter x: ");
						int x = int.Parse(Console.ReadLine()) - 1;

						Console.Write("Enter y: ");
						var y = int.Parse(Console.ReadLine()) - 1;

						Console.Write("Layout Across: ");
						var across = Console.ReadLine().ToUpper() == "Y";

						var result = board.Place(word, across, x, y);

						if (! result)
							Console.WriteLine("NOT ALLOWED!!!");
					}
					else if (argument == "2")
					{
						Console.Write("Enter letters in hand (SPACE for blank tile): ");
						var inHand = Console.ReadLine().ToUpper();

						List<string> allCombos;
						
						if (inHand.IndexOf(" ") >= 0)
						{
							// unfinished - doesn't allow for two blanks
							allCombos = new List<string>();

							for (int i = 'a'; i <= 'z'; i++)
							{
								var deblanked = inHand.Replace(" ", ((char) i).ToString());
								
								allCombos.AddRange(CalculateWordPermutations(deblanked.ToCharArray().Select(x => x.ToString()).ToArray()));
							}
						}
						else
							allCombos = CalculateWordPermutations(inHand.ToCharArray().Select(x => x.ToString()).ToArray());

						allCombos = allCombos.Distinct().ToList();

						var placements = new List<WordPlace>();
						var searchers = new List<Searcher>();

						// this would get a failing grade
						for (var x = 0; x < board.Width; x++)
							for (var y = 0; y < board.Height; y++)
							{
								var searcher = new Searcher(board, allCombos, placements, realWords, x, y);
								var thread = new Thread(searcher.Run);
								thread.Start();
								searchers.Add(searcher);
							}

						while (true)
							if (searchers.Where(x => ! x.Done).Any())
								Thread.Sleep(1000);
							else
								break;

						Console.WriteLine();

						placements.Sort((x, y) => x.Score.CompareTo(y.Score));

						var distinct = new List<WordPlace>();

						foreach (var placed in placements)
							if (! distinct.Exists(x => x.Across == placed.Across &&
							                           x.Score == placed.Score &&
							                           x.Word == placed.Word &&
							                           x.X == placed.X &&
							                           x.Y == placed.Y))
								distinct.Add(placed);

						foreach (var placement in distinct)
							Console.WriteLine(placement.Word + " (" + (placement.X + 1) + "," + (placement.Y + 1) + " - " +
							                  (placement.Across ? "across" : "down") +
							                  ") => " + placement.Score);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}
			}
		}

		private static List<string> CalculateWordPermutations(string[] letters)
		{
			var uniqueLetters = new string[letters.Length];
			
			for (int i = 0; i < letters.Length; i++)
				uniqueLetters[i] = ('0' + i).ToString();
			
			var returned = CalculateWordPermutationsImpl(uniqueLetters, new List<string>(), 0);

			for (int i = 0; i < returned.Count; i++)
				for (int j = 0; j < letters.Length; j++)
					returned[i] = returned[i].Replace(('0' + j).ToString(), letters[j]);

			return returned;
		}

		private static List<string> CalculateWordPermutationsImpl(string[] letters, List<string> words, int index)
        {
            bool finished = true;
            var newWords = new List<string>();

            if (words.Count == 0)
            {
                foreach (string letter in letters)
                {
                    words.Add(letter);
                }
            }

            for(int j=index; j<words.Count; j++)
            {
                string word = (string)words[j];
                for(int i =0; i<letters.Length; i++)
                {
                    if(!word.Contains(letters[i]))
                    {
                        finished = false;
                        string newWord = (string)word.Clone();
                        newWord += letters[i];
                        newWords.Add(newWord);
                    }
                }
            }

            foreach (string newWord in newWords)
            {   
                words.Add(newWord);
            }

            if(finished  == false)
            {
                CalculateWordPermutationsImpl(letters, words, words.Count - newWords.Count);
            }
            return words;
        }
	}
}
