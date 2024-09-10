using System;
using System.Collections.Generic;
using System.IO;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "C:\\Users\\Artem Zabiev\\source\\repos\\LabsC#\\First_ex\\data.txt";
            HashSet<char> delimiters = new HashSet<char> { ' ', ',', '.', '!', '\n', '\t' };

           
            using (TextTokenizer tokenizer = new TextTokenizer(delimiters, filePath))
            {
                foreach (string token in tokenizer)
                {
                    Console.WriteLine(token);
                }
            }
        }
    }

    public class TextTokenizer : IEnumerable<string>, IDisposable
    {
        private HashSet<char> _delimiters;
        private string _filePath;

        
        private StreamReader _reader;

       
        private bool _disposed = false;

        public TextTokenizer(HashSet<char> delimiters, string filePath)
        {
            _delimiters = delimiters ?? throw new ArgumentNullException(nameof(delimiters));
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException($"File not found: {_filePath}");
            }

            
            _reader = new StreamReader(_filePath);
        }

        
        ~TextTokenizer()
        {
            
            Dispose(false);
        }

       
        public void Dispose()
        {
           
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    
                    _reader?.Dispose();
                }
                
                _disposed = true;
            }
        }

        
        public IEnumerator<string> GetEnumerator()
        {
            if (_reader == null)
            {
                throw new ObjectDisposedException("TextTokenizer");
            }

            
            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                
                var tokens = SplitIntoTokens(line);
                foreach (var token in tokens)
                {
                    yield return token;
                }
            }
        }

        
        private IEnumerable<string> SplitIntoTokens(string line)
        {
            
            List<string> tokens = new List<string>();
            int start = 0;  

            
            for (int i = 0; i < line.Length; i++)
            {
               
                if (_delimiters.Contains(line[i]))
                {
                   
                    if (i > start)
                    {
                        tokens.Add(line.Substring(start, i - start));
                    }
                   
                    start = i + 1;
                }
            }

           
            if (start < line.Length)
            {
                tokens.Add(line.Substring(start));
            }

            return tokens;  
        }

        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();  
        }
    }
}
