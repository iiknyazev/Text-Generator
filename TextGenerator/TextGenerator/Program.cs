using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        public static void ProcessingSentence(
            string[] sentences,
            List<List<string>> sentencesList,
            StringBuilder newWord, int i)
        {
            for (int j = 0; j < sentences[i].Length; j++)
            {
                if (char.IsLetter(sentences[i][j]) || sentences[i][j] == '\'')
                {
                    newWord.Append(sentences[i][j].ToString());
                }
                else
                {
                    if (newWord.Length > 0)
                    {
                        sentencesList[i].Add(newWord.ToString().ToLower());
                    }
                    newWord.Clear();
                }
                if (j == sentences[i].Length - 1 && newWord.Length > 0)
                {
                    sentencesList[i].Add(newWord.ToString().ToLower());
                    newWord.Clear();
                }

            }
        }

        public static List<List<string>> GetSentencesList(string[] sentences)
        {
            List<List<string>> sentencesList = new List<List<string>>();
            var newWord = new StringBuilder();

            for (int i = 0; i < sentences.Length; i++)
            {
                sentencesList.Add(new List<string>());
                ProcessingSentence(sentences, sentencesList, newWord, i);
            }
            return sentencesList;
        }

        public static List<List<string>> ParseSentences(string text)
        {
            var separators = new char[9] { '.', '!', '?', ';', ':', '(', ')', '«', '»' };
            var sentences = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            var sentencesList = GetSentencesList(sentences);
            sentencesList.RemoveAll(x => x == null || x.Count == 0);

            return sentencesList;
        }

        public static void CreateNewBiGrammOnSentence(
            SortedDictionary<Tuple<string, string>, int> biGramm,
            List<string> sentences)
        {
            for (int i = 0; i < sentences.Count - 1; i++)
            {
                string biGrammKey = sentences[i];
                string biGrammValue = sentences[i + 1];
                Tuple<string, string> keyValuePair = new Tuple<string, string>(biGrammKey, biGrammValue);

                if (biGramm.ContainsKey(keyValuePair))
                    biGramm[keyValuePair]++;
                else
                    biGramm.Add(keyValuePair, 1);
            }
        }

        public static void CreateNewThreeGrammOnSentence(
            SortedDictionary<Tuple<string, string>, int> threeGramm,
            List<string> sentences)
        {
            for (int i = 0; i < sentences.Count - 2; i++)
            {
                string threeGrammKey = sentences[i] + " " + sentences[i + 1];
                string threeGrammValue = sentences[i + 2];
                Tuple<string, string> keyValuePair = new Tuple<string, string>(threeGrammKey, threeGrammValue);

                if (threeGramm.ContainsKey(keyValuePair))
                    threeGramm[keyValuePair]++;
                else
                    threeGramm.Add(keyValuePair, 1);
            }
        }

        public static void AddMostPopularGramms(
            SortedDictionary<Tuple<string, string>, int> gramm,
            Dictionary<string, string> result)
        {
            foreach (var elem in gramm)
            {
                if (!result.ContainsKey(elem.Key.Item1))
                    result.Add(elem.Key.Item1, elem.Key.Item2);
                else
                {
                    Tuple<string, string> grammKey = new Tuple<string, string>(elem.Key.Item1, result[elem.Key.Item1]);
                    if (gramm[grammKey] < gramm[elem.Key])
                        result[elem.Key.Item1] = elem.Key.Item2;
                    else if (gramm[grammKey] == gramm[elem.Key])
                    {
                        if (string.CompareOrdinal(result[elem.Key.Item1], elem.Key.Item2) > 0)
                            result[elem.Key.Item1] = elem.Key.Item2;
                    }
                }
            }
        }

        public static Dictionary<string, string> GetMostFrequentNextWords(List<List<string>> text)
        {
            var result = new Dictionary<string, string>();
            var biGramm = new SortedDictionary<Tuple<string, string>, int>();
            var threeGramm = new SortedDictionary<Tuple<string, string>, int>();

            for (int i = 0; i < text.Count; i++)
            {
                CreateNewBiGrammOnSentence(biGramm, text[i]);
                CreateNewThreeGrammOnSentence(threeGramm, text[i]);
            }

            AddMostPopularGramms(biGramm, result);
            AddMostPopularGramms(threeGramm, result);

            return result;
        }

        public static string ContinuePhrase(
            Dictionary<string, string> nextWords,
            string phraseBeginning,
            int wordsCount)
        {
            for (int i = 0; i < wordsCount; i++)
            {
                string[] wordsPhraseBeginning = phraseBeginning.Split(' ');
                string keyShort = "";
                string keyLong = "";

                if ((wordsPhraseBeginning.Length == 1))
                    keyShort = phraseBeginning;
                else
                {
                    keyLong = wordsPhraseBeginning[wordsPhraseBeginning.Length - 2] + " "
                            + wordsPhraseBeginning[wordsPhraseBeginning.Length - 1];
                    keyShort = wordsPhraseBeginning[wordsPhraseBeginning.Length - 1];
                }

                if (nextWords.ContainsKey(keyLong) && keyLong != "")
                    phraseBeginning = string.Concat(phraseBeginning, " ", nextWords[keyLong]);
                else if (nextWords.ContainsKey(keyShort))
                    phraseBeginning = string.Concat(phraseBeginning, " ", nextWords[keyShort]);
                else
                    break;
            }

            return phraseBeginning;
        }

        public static void Main()
        {
            // путь: ...TextGenerator\TextGenerator\bin\Debug\net6.0
            var text = File.ReadAllText(@"HarryPotterText.txt");
            var frequency = GetMostFrequentNextWords(ParseSentences(text));

            while (true)
            {
                Console.Write("Введите первое слово (например, Фуко): ");
                var beginning = Console.ReadLine();
                if (string.IsNullOrEmpty(beginning)) return;
                var phrase = ContinuePhrase(frequency, beginning.ToLower(), 10);
                Console.WriteLine(phrase);
            }
        }
    }
}
