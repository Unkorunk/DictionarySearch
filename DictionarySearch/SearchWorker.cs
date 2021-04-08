using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DictionarySearch
{
    public class SearchWorker
    {
        private Thread _thread;
        private StreamReader _streamReader;
        private string _searchString = string.Empty;
        private bool _isSeqSearch = true;
        private readonly object _searchLock = new object();

        public bool IsFinished { get; private set; }
        public bool IsFresh { get; set; }
        public object ResultLock { get; } = new object();
        public Queue<string> Result { get; private set; } = new Queue<string>();

        public SearchWorker()
        {
            _streamReader = new StreamReader("words.txt");
            _thread = new Thread(Run) {IsBackground = true};
            _thread.Start();
        }

        public void UpdateSearchString(string searchString, bool isSeqSearch)
        {
            lock (_searchLock)
            {
                IsFinished = false;

                _streamReader.Close();
                _streamReader = new StreamReader("words.txt");

                _searchString = searchString;
                _isSeqSearch = isSeqSearch;

                lock (ResultLock)
                {
                    IsFresh = true;
                    Result.Clear();
                }
            }
        }

        private void Run()
        {
            while (true)
            {
                lock (_searchLock)
                {
                    if (_streamReader.EndOfStream)
                    {
                        IsFinished = true;
                        continue;
                    }

                    var target = _streamReader.ReadLine();
                    if (!MyCompare(_searchString, target)) continue;

                    lock (ResultLock)
                    {
                        Result.Enqueue(target);
                    }
                }
            }
        }

        private bool MyCompare(string input, string target)
        {
            return _isSeqSearch ? MyCompareSeq(input, target) : MyCompareSubStr(input, target);
        }

        private static bool MyCompareSubStr(string input, string target) => target.Contains(input);

        private static bool MyCompareSeq(string input, string target)
        {
            var i = 0;
            foreach (var t in input)
            {
                while (i < target.Length && t != target[i]) i++;
                if (i >= target.Length) return false;
                i++;
            }

            return true;
        }
    }
}