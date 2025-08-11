using BenchmarkDotNet.Attributes;
using MySimpleDictionary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySimpleDictionary.Helper
{
    public class MySimpleDictionaryBenchmarkHelper
    {
        //konstruktor
        private static MySimpleDictionary<int, string> myDictionary = new MySimpleDictionary<int, string>();

        //dodavanje novog elementa
        [Benchmark]
        public void AddNewElement()
        {
            //MySimpleDictionary<int, string> myDictionary = new MySimpleDictionary<int, string>();
            try
            {
                myDictionary.Add(1, "prvi element");
            }
            catch (Exception ex)
            {
            }
        }

        //uzimanje elementa na osnovu kljuca
        [Benchmark]
        public void GetElementByKey()
        {
            string value = myDictionary[1];
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
            foreach(var item in myDictionary)
            {
                int key = item.Key;
                string value = item.Value;
            }
        }
    }
}
