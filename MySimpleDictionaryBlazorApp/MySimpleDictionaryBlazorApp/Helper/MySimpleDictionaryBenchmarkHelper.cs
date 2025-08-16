using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MySimpleDictionaryBlazorApp.Model;

namespace MySimpleDictionaryBlazorApp.Helper
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class MySimpleDictionaryBenchmarkHelper
    {
        //konstruktor
        //rivate static MySimpleDictionary<int, string> myDictionary = new MySimpleDictionary<int, string>();
        private MySimpleDictionary<int, string> myDictionary = new MySimpleDictionary<int, string>();
        private Dictionary<int, string> dictionary = new Dictionary<int, string>();

        [IterationSetup]
        public void BeforeEach()
        {
            dictionary.Clear();
            myDictionary.Clear();
            myDictionary.Add(1, "prvi element");
        }

        //dodavanje novog elementa
        [Benchmark]
        public void AddNewElement()
        {
            //MySimpleDictionary<int, string> myDictionary = new MySimpleDictionary<int, string>();
            
            myDictionary.Add(2, "drugi element");
        }

        //provera postojanja kljuca
        [Benchmark]
        public void ContainsKey()
        {
            myDictionary.ContainsKey(1);
        }

        //provera postojanja vrednosti
        [Benchmark]
        public void ContainsValue()
        {
            myDictionary.ContainsValue("prvi element");
        }

        //uklanjanje pojedinacnog elementa
        [Benchmark]
        public void RemoveValue()
        {
            myDictionary.Remove(1);
        }

        //uklanjanje pojedinacnog elementa sa vracanjem vrednosti
        [Benchmark]
        public void RemoveValueReturnValue()
        {
            string value;
            myDictionary.Remove(1, out value);
        }

        //brisanje celog sadrzaja
        [Benchmark]
        public void Clear()
        {
            myDictionary.Clear();
        }

        //iteriranje kroz recnik
        [Benchmark]
        public void Iterator()
        {
            foreach (var item in myDictionary)
            {
                int key = item.Key;
                string value = item.Value;
            }
        }

        //Dodavanje 1000 Elemenata u moj dictionary
        [Benchmark]
        public void Add100ElementsMySimpleDictionary()
        {
            for (int i = 2; i < 102; i++)
            {
                myDictionary.Add(i, "element");
            }
        }

        //Dodavanje 1000 Elemenata u dictionary za poredjenje
        [Benchmark]
        public void Add100ElementsDictionary()
        {
            for (int i = 2; i < 102; i++)
            {
                dictionary.Add(i, "element");
            }
        }
    }
}
