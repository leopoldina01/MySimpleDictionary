using Microsoft.Internal.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MySimpleDictionary.Model
{
    public class MySimpleDictionary<TKey, TValue>
    {
        private struct Entry
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
        private decimal minLoadFactor; //donja granica load factora kada se predje resizuje se recnik (smanjuje)
        private int startFreeList; //pocetna vrednost za free list koja se koristi za racunanje pozicije sledeceg elementa u sledecoj listi
        //kraj free liste oznacava -2
        private int totalNumberOfEntries; //ovde ide broj entrija koji nisu obrisani + broj entrija koji su obrisani (numberOfEntries + freeCount)

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
            minLoadFactor = 0.25m;
            totalNumberOfEntries = 0;
        }

        public void Add(TKey key, TValue value)
        {
            //ovde treba da ide deo gde proverava da li je load factor veci od 0.75
            //ako je veci prvo se uradi resize
            //ako nije ide se dalje
            if (loadFactor >= 1 && freeCount == 0)
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
                IsKeyAlreadyInTheList(hashCode, bucketIndex, key);

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
            loadFactor = numberOfEntries / sizeOfBuckets;
        }

        private void Resize()
        {
            //za sada cu staviti da se size udupla, pa cu posle implementirati hashhelpers metodu
            int newSize = sizeOfBuckets * 2;
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
        }

        private void UpdateSizeOfEntries()
        {
            //povecamo broj entries
            numberOfEntries++;

            //povecavamo broj totalNumberOfEntries
            totalNumberOfEntries = numberOfEntries + freeCount;
        }

        private void IsKeyAlreadyInTheList(int hashCode, int bucketIndex, TKey key)
        {
            int elementNext = buckets[bucketIndex] - 1;
            while (elementNext != -1)
            {
                if (hashCode == entries[elementNext].HashCode)
                {
                    if (key.Equals(entries[elementNext].Key))
                    {
                        throw new ArgumentException("Argument with this key: " + key.ToString() + ", already exists in the dictionary");
                    }
                }
                elementNext = entries[elementNext].next;
            }
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
    }
}
