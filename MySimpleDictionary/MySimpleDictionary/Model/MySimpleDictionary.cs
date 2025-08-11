using Microsoft.Internal.VisualStudio.Shell;
using MySimpleDictionary.Helper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MySimpleDictionary.Model
{
    public class MySimpleDictionary<TKey, TValue> : IEnumerable<(TKey Key, TValue Value)>
    {
        public struct Entry
        {
            public int HashCode;
            public int next;
            public TKey Key;
            public TValue Value;
        }

        private int[] buckets; //mora se staviti inicijalna vrednost bucketa, obicno je 11
        private Entry[] entries; //treba mi za listu entrija u bucketu
        private int sizeOfBuckets; //ovo ce trebati ali ce se koristiti i kao sizeOfEntries jer su im iste dimenzije
        private int numberOfEntries; //i ovo cemo videti dal ce mi trebati, ili nece
        private int freeList; //sadrzi indeks prvog elementa u free listi
        private int freeCount; //sadrzi broj elemenata koji su prazni
        private decimal loadFactor; //load Factor na osnovu kojeg ce se resizovati dictionary
        private decimal maxLoadFactor; //gornja granica load factora kada se predje resizuje se recnik (povecava)
        private int startFreeList; //pocetna vrednost za free list koja se koristi za racunanje pozicije sledeceg elementa u sledecoj listi
        //kraj free liste oznacava -2
        private int totalNumberOfEntries; //ovde ide broj entrija koji nisu obrisani + broj entrija koji su obrisani (numberOfEntries + freeCount)
        public List<TKey> Keys { get; private set; } //lista svih kljuceva
        public List<TValue> Values { get; private set; } //lista svih vrednosti

        //properties
        public int[] Buckets { get { return buckets; } }
        public Entry[] Entries { get { return entries; } }
        public int SizeOfBuckets { get { return sizeOfBuckets; } }
        public int NumberOfEntries { get { return numberOfEntries; } }
        public int FreeList { get { return freeList; } }
        public int FreeCount { get { return freeCount; } }
        public decimal LoadFactor { get { return loadFactor; } }
        public decimal MaxLoadFactor { get { return maxLoadFactor; } }
        public int StartFreeList { get { return startFreeList; } }
        public int TotalNumberOfEntries { get { return  totalNumberOfEntries; } }
        public int Count { get { return numberOfEntries; } }

        public MySimpleDictionary()
        {
            sizeOfBuckets = 11; //inicijalna vrednost je 11, ne mora da bude, moze i nula, al da bismo izbegli 2-3 resiza na pocetku bas stavljamo 11
            buckets = new int[sizeOfBuckets];
            entries = new Entry[sizeOfBuckets];
            freeList = -1;
            freeCount = 0;
            loadFactor = 0;
            numberOfEntries = 0;
            maxLoadFactor = 0.75m;
            totalNumberOfEntries = 0;
            startFreeList = -3;
            Keys = new List<TKey>();
            Values = new List<TValue>();
        }

        //konstruktor kopije
        public MySimpleDictionary(MySimpleDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("Dictionary can't be null!");
            }

            this.buckets = dictionary.Buckets;
            this.entries = dictionary.Entries;
            this.sizeOfBuckets = dictionary.SizeOfBuckets;
            this.numberOfEntries = dictionary.NumberOfEntries;
            this.freeList = dictionary.FreeList;
            this.freeCount = dictionary.FreeCount; 
            this.loadFactor = dictionary.LoadFactor; 
            this.maxLoadFactor = dictionary.MaxLoadFactor; 
            this.startFreeList = dictionary.StartFreeList; 
            this.totalNumberOfEntries = dictionary.TotalNumberOfEntries;
            this.Keys = dictionary.Keys; 
            this.Values = dictionary.Values;
        }

        //konstruktor sa inicijalnim kapacitetom
        public MySimpleDictionary(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("Capacity can't be less than 0.");
            }

            sizeOfBuckets = capacity;
            buckets = new int[sizeOfBuckets];
            entries = new Entry[sizeOfBuckets];
            freeList = -1;
            freeCount = 0;
            loadFactor = 0;
            numberOfEntries = 0;
            maxLoadFactor = 0.75m;
            totalNumberOfEntries = 0;
            startFreeList = -3;
            Keys = new List<TKey>();
            Values = new List<TValue>();
        }

        //konstruckor sa prosledjenim collectionom
        public MySimpleDictionary(IEnumerable<(TKey key, TValue value)> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Collection shouldn't be null.");
            }

            foreach (var item in collection)
            {
                try
                {
                    Add(item.key, item.value);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("There is double key in collection.", ex);
                }
            }
        }

        //pristup elementu
        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("Key shouldn't be null!");
                }
                bool isKeyInTheDictionary = IsKeyAlreadyInTheList(key);
                if (isKeyInTheDictionary)
                {
                    int entryIndex = GetEntryByKey(key);
                    return entries[entryIndex].Value;
                }
                else
                {
                    throw new KeyNotFoundException("Key is not found.");
                }
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("Key shouldn't be null!");
                }
                bool isKeyInTheDictionary = IsKeyAlreadyInTheList(key);
                if (isKeyInTheDictionary)
                {
                    int entryIndex = GetEntryByKey(key);
                    entries[entryIndex].Value = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            //ovde treba da ide deo gde proverava da li je load factor veci od 0.75
            //ako je veci prvo se uradi resize
            //ako nije ide se dalje
            if ((loadFactor >= 1 && freeCount == 0) || sizeOfBuckets == 0)
            {
                Resize();
            }

            //racunanje hash koda od keya
            int hashCode = key.GetHashCode();
            int bucketIndex = hashCode % sizeOfBuckets;
            int next = -1;
            int pointerInBucket = 0;

            if (buckets[bucketIndex] == 0)
            {
                //dakle prvo provera da li postoji elemenata u freeListi, tj dal ima gapova u entries listi
                if (freeCount > 0 && freeList != -1)
                {
                    //ovde treba prvo zauzeti tu free poziciju (prethodno obrisanu)
                    buckets[bucketIndex] = freeList;
                    RemoveFromFreeList();

                    UpdateSizeOfEntries();
                }
                else
                {
                    //znaci linked lista na ovom bucketu je prazna
                    UpdateSizeOfEntries();

                    buckets[bucketIndex] = totalNumberOfEntries;
                }
            }
            else
            {
                //ovde ce trebati i provera da li vec postoji taj element isti u listi
                //moze samo da se prodje kroz linked listu entrija
                //ako se nalazi vec isti element u listi, tj isti key, onda treba baciti exception
                bool alreadyInTheList = IsKeyAlreadyInTheList(key);
                if (alreadyInTheList)
                {
                    throw new ArgumentException("Argument with this key: " + key.ToString() + ", already exists in the dictionary");
                }

                if (freeCount > 0 && freeList != -1)
                {
                    next = buckets[bucketIndex] - 1;
                    buckets[bucketIndex] = freeList;

                    RemoveFromFreeList();

                    UpdateSizeOfEntries();
                }
                else
                {
                    UpdateSizeOfEntries();

                    //ovo ako vec ima linked lista u bucketu
                    next = buckets[bucketIndex] - 1;
                    buckets[bucketIndex] = totalNumberOfEntries;
                }
            }

            int entriesIndex = buckets[bucketIndex] - 1;
            AddEntry(hashCode, next, key, value, entriesIndex);
            loadFactor = Math.Round((decimal)numberOfEntries / sizeOfBuckets, 2);
        }

        private void Resize()
        {
            //za sada cu staviti da se size udupla, pa cu posle implementirati hashhelpers metodu
            int newSize = sizeOfBuckets * 2;
            newSize = PrimeNumbersHelper.GetFirstNextPrime(newSize);
            int[] resizedBuckets = new int[newSize];
            Entry[] resizedEntries = new Entry[newSize];
            int resizedNumberOfEntries = 0;
            int resizedNext = -1;

            foreach (var entry in entries)
            {
                //ovde treba da se dodaju elementi u resizedBuckets i resizedEntries
                int hashCode = entry.HashCode;
                int resizedBucketIndex = hashCode % newSize;
                if (resizedBuckets[resizedBucketIndex] == 0)
                {
                    resizedNumberOfEntries++;
                    resizedBuckets[resizedBucketIndex] = resizedNumberOfEntries;
                }
                else
                {
                    resizedNext = resizedBuckets[resizedBucketIndex] - 1;
                    resizedNumberOfEntries++;
                    resizedBuckets[resizedBucketIndex] = resizedNumberOfEntries;
                }
                int resizedIndex = resizedNumberOfEntries - 1;

                Entry resizedEntry;
                resizedEntry.HashCode = entry.HashCode;
                resizedEntry.next = resizedNext;
                resizedEntry.Key = entry.Key;
                resizedEntry.Value = entry.Value;
                resizedEntries[resizedIndex] = resizedEntry;
            }

            //onda na kraju treba da se kopiraju u array buckets i entries
            buckets = new int[newSize];
            entries = new Entry[newSize];

            for (int i = 0; i < newSize; i++)
            {
                buckets[i] = resizedBuckets[i];
                entries[i] = resizedEntries[i];
            }

            sizeOfBuckets = newSize;
        }

        private void UpdateSizeOfEntries()
        {
            //povecamo broj entries
            numberOfEntries++;

            //povecavamo broj totalNumberOfEntries
            totalNumberOfEntries = numberOfEntries + freeCount;
        }

        private bool IsKeyAlreadyInTheList(TKey key)
        {
            int hashCode = key.GetHashCode();
            int bucketIndex = hashCode % sizeOfBuckets;
            int elementNext = buckets[bucketIndex] - 1;
            while (elementNext != -1)
            {
                if (hashCode == entries[elementNext].HashCode)
                {
                    if (key.Equals(entries[elementNext].Key))
                    {
                        return true;
                        //throw new ArgumentException("Argument with this key: " + key.ToString() + ", already exists in the dictionary");
                    }
                }
                elementNext = entries[elementNext].next;
            }

            return false;
        }

        private void RemoveFromFreeList()
        {
            //ovde se obracunava freeCount
            freeCount--; //mora se smanjiti broj u freeCount
            freeList = Math.Abs(entries[freeList].next) + startFreeList; //popunjava se trenutna vrednost freeListe pomera se pokazivac onda
        }

        //funkcija za dodavanje entrija
        private void AddEntry(int hashCode, int next, TKey key, TValue value, int entriesIndex)
        {
            Entry entry;
            entry.HashCode = hashCode;
            entry.next = next;
            entry.Key = key;
            entry.Value = value;
            entries[entriesIndex] = entry;
            Keys.Add(key);
            Values.Add(value);
        }

        //jedna test funkcija za ispis elemenata cisto da vidimo kako radi dal se dodaju i brisu i ostalo
        public void WriteAllElementsFromDictionary()
        {
            for (int i = 0; i < numberOfEntries; i++)
            {
                //preskace obrisane elemente
                if (entries[i].next < -1)
                {
                    continue;
                }
                //ispisuje samo elemente koji nisu obrisani
                Console.WriteLine("Key: " + entries[i].Key + " Value: " + entries[i].Value);
            }
        }

        //provera da li postoji kljuc
        public bool ContainsKey(TKey key)
        {
            int hashCode = key.GetHashCode();
            int bucketIndex = hashCode % sizeOfBuckets;
            bool containsKey = IsKeyAlreadyInTheList(key);
            return containsKey;
        }

        //provera da li postoji vrednost
        public bool ContainsValue(TValue value)
        {
            foreach (Entry entry in entries)
            {
                if (entry.next > -2)
                {
                    if (entry.Value != null)
                    {
                        if (entry.Value.Equals(value))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("Key is null");
            }

            int hashCode = key.GetHashCode();
            int bucketIndex = hashCode % sizeOfBuckets;
            bool containsKey = IsKeyAlreadyInTheList(key);

            if (!containsKey)
            {
                return false;
            }

            int current = buckets[bucketIndex] - 1;
            int before = -1;
            //prvo proverimo prvi element on ako nije bice pokazivac na before, ako jeste samo ce buckets[bucketIndex] = entries[next].next
            if (entries[current].HashCode == hashCode && entries[current].Key.Equals(key))
            {
                RemoveEntry(bucketIndex, current, key);
                return true;
            }
            else
            {
                //current postaje before
                before = current;
                current = entries[current].next;
            }

            while (current != -1)
            {
                if (entries[current].HashCode == hashCode && entries[current].Key.Equals(key))
                {
                    RemoveEntry(bucketIndex, current, key);
                    break;
                }
                before = current;
                current = entries[current].next;
            }

            return true;
        }

        private void RemoveEntry(int bucketIndex, int current, TKey key)
        {
            buckets[bucketIndex] = entries[current].next + 1;
            entries[current].next = startFreeList - freeList;
            freeCount++;
            freeList = current;
            Keys.Remove(key);
            Values.Remove(entries[current].Value);
        }

        public bool Remove(TKey key, out TValue value)
        {
            value = default(TValue);
            bool isRemoved = Remove(key);

            if (isRemoved)
            {
                value = entries[freeList].Value;
            }

            return isRemoved;
        }

        public void Clear()
        {
            //sve se resetuje osim size
            buckets = new int[sizeOfBuckets];
            entries = new Entry[sizeOfBuckets];
            freeList = -1;
            freeCount = 0;
            loadFactor = 0;
            numberOfEntries = 0;
            totalNumberOfEntries = 0;
            Keys = new List<TKey>();
            Values = new List<TValue>();
        }

        //broj elemenata u dictionary
        //public int Count()
        //{
        //    return numberOfEntries;
        //}

        private int GetEntryByKey(TKey key)
        {
            int hashCode = key.GetHashCode();
            int bucketIndex = hashCode % sizeOfBuckets;
            int next = buckets[bucketIndex] - 1;
            while (next != -1)
            {
                if (entries[next].HashCode == hashCode && entries[next].Key.Equals(key))
                {
                    return next;
                }
            }
            return -1;
        }

        //iteriranje kroz dictionary
        public IEnumerator<(TKey Key, TValue Value)> GetEnumerator()
        {
            for (int i = 0; i < totalNumberOfEntries; i++)
            {
                if (entries[i].next > -2)
                {
                    yield return (entries[i].Key, entries[i].Value);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
