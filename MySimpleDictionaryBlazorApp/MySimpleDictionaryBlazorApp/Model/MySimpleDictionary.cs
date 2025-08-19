using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MudBlazor;
using MySimpleDictionaryBlazorApp.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;

namespace MySimpleDictionaryBlazorApp.Model
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

        private int[] buckets;
        private Entry[] entries;
        private int sizeOfBuckets; //ovo ce trebati ali ce se koristiti i kao sizeOfEntries jer su im iste dimenzije
        private int numberOfEntries;
        private int freeList; 
        private int freeCount; 
        private decimal loadFactor; 
        private decimal maxLoadFactor; 
        private int startFreeList; //pocetna vrednost za free list koja se koristi za racunanje pozicije sledeceg elementa u sledecoj listi
        private int totalNumberOfEntries; //ovde ide broj entrija koji nisu obrisani + broj entrija koji su obrisani (numberOfEntries + freeCount)
        private bool hasCustomComparer;
        private IEqualityComparer<TKey> comparer = EqualityComparer<TKey>.Default;

        //moja inicijalna ideja implementacije
        //private TKey keys;
        //public IEnumerable<TKey> Keys { get { return GetAllKeysIterator(); } }
        //private TValue values;
        //public IEnumerable<TValue> Values { get { return GetAllValuesIterator(); } }

        //poboljsanje po uzoru na .net implementaciju
        private MyKeyCollection keys;
        private MyValueCollection values;
        //properties
        public MyKeyCollection Keys
        { 
            get
            {
                if (keys != null)
                {
                    return keys;
                }
                else
                {
                    return new MyKeyCollection(this);
                }
            }
        }
        public MyValueCollection Values
        {
            get
            {
                if (values != null)
                {
                    return values;
                }
                else
                {
                    return new MyValueCollection(this);
                }
            }
        }
        
        public int[] Buckets { get { return buckets; } }
        public Entry[] Entries { get { return entries; } }
        public int SizeOfBuckets { get { return sizeOfBuckets; } }
        public int NumberOfEntries { get { return numberOfEntries; } }
        public int FreeList { get { return freeList; } }
        public int FreeCount { get { return freeCount; } }
        public decimal LoadFactor { get { return loadFactor; } }
        public decimal MaxLoadFactor { get { return maxLoadFactor; } }
        public int StartFreeList { get { return startFreeList; } }
        public int TotalNumberOfEntries { get { return totalNumberOfEntries; } }
        public int Count { get { return numberOfEntries; } }
        public int Capacity { get { return sizeOfBuckets; } }
        public IEqualityComparer<TKey> Comparer { get { return comparer; } }
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

        public MySimpleDictionary()
        {
            sizeOfBuckets = 11; //inicijalna vrednost je 11, ne mora da bude, moze i nula, al da bismo izbegli 2-3 resiza na pocetku stavljamo 11
            buckets = new int[sizeOfBuckets];
            entries = new Entry[sizeOfBuckets];
            freeList = -1;
            freeCount = 0;
            loadFactor = 0;
            numberOfEntries = 0;
            maxLoadFactor = 0.75m;
            totalNumberOfEntries = 0;
            startFreeList = -3;
            hasCustomComparer = false;
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
            hasCustomComparer = false;
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
            hasCustomComparer = false;
        }

        //konstruckor sa prosledjenim collectionom
        public MySimpleDictionary(IEnumerable<(TKey key, TValue value)> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Collection shouldn't be null.");
            }

            sizeOfBuckets = 11;
            buckets = new int[sizeOfBuckets];
            entries = new Entry[sizeOfBuckets];
            freeList = -1;
            freeCount = 0;
            loadFactor = 0;
            numberOfEntries = 0;
            maxLoadFactor = 0.75m;
            totalNumberOfEntries = 0;
            startFreeList = -3;
            hasCustomComparer = false;

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

        //konstruktor sa prosledjenim Equality comparerom
        public MySimpleDictionary(IEqualityComparer<TKey>? comparer)
        {
            sizeOfBuckets = 11; //inicijalna vrednost je 11, ne mora da bude, moze i nula, al da bismo izbegli 2-3 resiza na pocetku stavljamo 11
            buckets = new int[sizeOfBuckets];
            entries = new Entry[sizeOfBuckets];
            freeList = -1;
            freeCount = 0;
            loadFactor = 0;
            numberOfEntries = 0;
            maxLoadFactor = 0.75m;
            totalNumberOfEntries = 0;
            startFreeList = -3;
            if (comparer != null)
            {
                this.comparer = comparer;
                hasCustomComparer = true;
            }
            else
            {
                hasCustomComparer = false;
            }
        }

        //konstruktor kopije sa equality comparerom
        public MySimpleDictionary(MySimpleDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer)
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
            if (comparer != null)
            {
                this.comparer = comparer;
                hasCustomComparer = true;
            }
            else
            {
                hasCustomComparer = false;
            }
        }

        //konstruktor sa ienumerable kolekcijom i definisanim equality comparerom
        public MySimpleDictionary(IEnumerable<(TKey key, TValue value)> collection, IEqualityComparer<TKey>? comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Collection shouldn't be null.");
            }

            sizeOfBuckets = 11;
            buckets = new int[sizeOfBuckets];
            entries = new Entry[sizeOfBuckets];
            freeList = -1;
            freeCount = 0;
            loadFactor = 0;
            numberOfEntries = 0;
            maxLoadFactor = 0.75m;
            totalNumberOfEntries = 0;
            startFreeList = -3;
            if (comparer != null)
            {
                hasCustomComparer = true;
                this.comparer = comparer;
            }
            else
            {
                hasCustomComparer = false;
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

        //konstruktor sa prosledjenim kapacitetom i equality comparerom
        public MySimpleDictionary(int capacity, IEqualityComparer<TKey>? comparer)
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
            if (comparer != null)
            {
                hasCustomComparer = true;
                this.comparer = comparer;
            }
            else
            {
                hasCustomComparer = false;
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("Key shouldn't be null");
            }

            if (sizeOfBuckets == 0 || (freeCount == 0 && loadFactor >= maxLoadFactor))
            {
                Resize();
            }

            int hashCode = comparer.GetHashCode(key);
            uint bucketIndex = (uint)hashCode % (uint)sizeOfBuckets;

            int next = -1;

            int elementNext = buckets[bucketIndex] - 1;
            while (elementNext != -1)
            {
                Entry entryElementNext = entries[elementNext];
                if (hashCode == entryElementNext.HashCode && comparer.Equals(key, entryElementNext.Key))
                {
                    //bool equality = comparer.Equals(key, entries[elementNext].Key);

                    //if (equality)
                    //{
                    //    throw new ArgumentException("Argument with this key: " + key.ToString() + ", already exists in the dictionary");
                    //}
                    throw new ArgumentException("Argument with this key: " + key.ToString() + ", already exists in the dictionary");
                }
                elementNext = entryElementNext.next;
            }

            int resizedEntryIndex = -1;
            if (freeCount > 0 && freeList != -1)
            {
                next = buckets[bucketIndex] - 1;
                buckets[bucketIndex] = freeList + 1;
                resizedEntryIndex = freeList;

                freeCount--;
                freeList = -1 * entries[freeList].next + startFreeList;

                numberOfEntries++;
            }
            else
            {
                numberOfEntries++;

                totalNumberOfEntries++;

                next = buckets[bucketIndex] - 1;
                buckets[bucketIndex] = totalNumberOfEntries;
                resizedEntryIndex = totalNumberOfEntries - 1;
            }

            Entry entry;
            entry.HashCode = hashCode;
            entry.next = next;
            entry.Key = key;
            entry.Value = value;
            entries[resizedEntryIndex] = entry;
            loadFactor = (decimal)numberOfEntries / sizeOfBuckets;
        }

        private void Resize()
        {
            int newSize = sizeOfBuckets * 2;
            newSize = PrimeNumbersHelper.GetFirstNextPrime(newSize);
            int[] resizedBuckets = new int[newSize];
            Entry[] resizedEntries = new Entry[newSize];

            for (int i = 0; i < totalNumberOfEntries; i++)
            {
                
                if (entries[i].next > -2)
                {
                    Entry entry = entries[i];
                    //uint resizedBucketIndex = (uint)entry.HashCode % (uint)newSize;
                    //resizedEntries[i] = entry;
                    //resizedEntries[i].next = resizedBuckets[resizedBucketIndex] - 1;
                    //resizedBuckets[resizedBucketIndex] = i + 1;

                    uint resizedBucketIndex = (uint)entry.HashCode % (uint)newSize;
                    entry.next = resizedBuckets[resizedBucketIndex] - 1;
                    resizedEntries[i] = entry;
                    resizedBuckets[resizedBucketIndex] = i + 1;
                }
                else
                {
                    resizedEntries[i] = entries[i];
                }
            }

            buckets = resizedBuckets;
            entries = resizedEntries;

            sizeOfBuckets = newSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsKeyAlreadyInTheList(TKey key)
        {
            int hashCode = comparer.GetHashCode(key);
            uint bucketIndex = (uint)hashCode % (uint)sizeOfBuckets;

            int elementNext = buckets[bucketIndex] - 1;
            while (elementNext != -1)
            {
                if (hashCode == entries[elementNext].HashCode)
                {
                    bool equality = comparer.Equals(key, entries[elementNext].Key);

                    if (equality)
                    {
                        return true;
                    }
                }
                elementNext = entries[elementNext].next;
            }

            return false;
        }

        //provera da li postoji kljuc
        public bool ContainsKey(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("Key is null");
            }

            int hashCode = comparer.GetHashCode(key);
            uint bucketIndex = (uint)hashCode % (uint)sizeOfBuckets;

            int elementNext = buckets[bucketIndex] - 1;
            while (elementNext != -1)
            {
                Entry entry = entries[elementNext];
                if (hashCode == entry.HashCode && comparer.Equals(key, entry.Key))
                {
                    //bool equality = comparer.Equals(key, entries[elementNext].Key);

                    //if (equality)
                    //{
                    //    return true;
                    //}
                    return true;
                }
                //elementNext = entries[elementNext].next;
                elementNext = entry.next;
            }

            return false;
        }

        //provera da li postoji vrednost
        public bool ContainsValue(TValue value)
        {
            foreach (Entry entry in entries)
            {
                //if (entry.next > -2)
                //{
                //    if (entry.Value != null)
                //    {
                //        if (entry.Value.Equals(value))
                //        {
                //            return true;
                //        }
                //    }
                //}
                if (entry.next > -2 && entry.Value != null && entry.Value.Equals(value))
                {
                    return true;
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

            int hashCode = comparer.GetHashCode(key);

            uint bucketIndex = (uint)hashCode % (uint)sizeOfBuckets;

            int current = buckets[bucketIndex] - 1;
            int before = -1;

            //nema elementa koji treba obrisati
            if (current == -1)
            {
                return false;
            }

            Entry currentEntry = entries[current];
            if (currentEntry.HashCode == hashCode && comparer.Equals(currentEntry.Key, key))
            {
                buckets[bucketIndex] = currentEntry.next + 1;
                entries[current].next = startFreeList - freeList;
                freeCount++;
                freeList = current;
                numberOfEntries--;
                return true;
            }
            else
            {
                //current postaje before
                before = current;
                current = currentEntry.next;
            }

            while (current != -1)
            {
                currentEntry = entries[current];
                if (currentEntry.HashCode == hashCode && comparer.Equals(currentEntry.Key, key))
                {
                    entries[before].next = currentEntry.next;
                    entries[current].next = startFreeList - freeList;
                    freeCount++;
                    freeList = current;
                    numberOfEntries--;
                    return true;
                }
                before = current;
                current = currentEntry.next;
            }

            return false;
        }

        public bool Remove(TKey key, out TValue value)
        {
            value = default(TValue);

            if (Remove(key))
            {
                value = entries[freeList].Value;
                return true;
            }

            return false;
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
        }

        private int GetEntryByKey(TKey key)
        {
            int hashCode = comparer.GetHashCode(key);
            uint bucketIndex = (uint)hashCode % (uint)sizeOfBuckets;

            int next = buckets[bucketIndex] - 1;
            while (next != -1)
            {

                if (entries[next].HashCode == hashCode)
                {
                    bool equality = comparer.Equals(entries[next].Key, key);
                    if (equality)
                    {
                        return next;
                    }
                }
                next = entries[next].next;
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

        //TryAdd
        public bool TryAdd(TKey key, TValue value)
        {
            try
            {
                Add(key, value);
                return true;
            } 
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(ex.Message, ex);
            }
            catch (ArgumentException ex)
            {
                return false;
            }
        }

        //TryGetValue
        public bool TryGetValue(TKey key, out TValue value)
        {
            try
            {
                TValue getValue = this[key];
                value = getValue;
                return true;
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(ex.Message, ex);
            }
            catch (KeyNotFoundException ex)
            {
                value = default;
                return false;
            }
        }

        //moja inicijalna implementacija
        //private IEnumerable<TKey> GetAllKeysIterator()
        //{
        //    for (int i = 0; i < totalNumberOfEntries; i++)
        //    {
        //        if (entries[i].next > -2)
        //        {
        //            yield return entries[i].Key;
        //        }
        //    }
        //}

        //private IEnumerable<TValue> GetAllValuesIterator()
        //{
        //    for (int i = 0; i < totalNumberOfEntries; i++)
        //    {
        //        if (entries[i].next > -2)
        //        {
        //            yield return entries[i].Value;
        //        }
        //    }
        //}

        //poboljsanje po uzoru na .net implementaciju
        public sealed class MyKeyCollection : IEnumerable<TKey> 
        {
            private readonly MySimpleDictionary<TKey, TValue> _dictionary;
            public MyKeyCollection(MySimpleDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }

            public MyEnumerator GetEnumerator() => new MyEnumerator(_dictionary);

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<TKey>)this).GetEnumerator();
            }

            public struct MyEnumerator : IEnumerator<TKey>, IEnumerator
            {
                private readonly MySimpleDictionary<TKey, TValue> _mySimpleDictionary;
                private TKey? _currentKey;
                private int _index;

                internal MyEnumerator(MySimpleDictionary<TKey, TValue> mySimpleDictionary)
                {
                    _mySimpleDictionary = mySimpleDictionary;
                    _currentKey = default;
                    _index = 0;
                }
                public TKey Current => _currentKey;

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    while ((uint)_index < (uint)_mySimpleDictionary.totalNumberOfEntries)
                    {
                        ref Entry entry = ref _mySimpleDictionary.entries![_index++];

                        if (entry.next >= -1)
                        {
                            _currentKey = entry.Key;
                            return true;
                        }
                    }

                    _index = _mySimpleDictionary.totalNumberOfEntries + 1;
                    _currentKey = default;
                    return false;
                }

                public void Reset()
                {
                    _index = 0;
                    _currentKey = default;
                }
            }
        }

        public sealed class MyValueCollection : IEnumerable<TValue>
        {
            private readonly MySimpleDictionary<TKey, TValue> _dictionary;
            public MyValueCollection(MySimpleDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }

            public MyEnumerator GetEnumerator() => new MyEnumerator(_dictionary);

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<TValue>)this).GetEnumerator();
            }

            public struct MyEnumerator : IEnumerator<TValue>, IEnumerator
            {
                private readonly MySimpleDictionary<TKey, TValue> _mySimpleDictionary;
                private TValue? _currentValue;
                private int _index;

                internal MyEnumerator(MySimpleDictionary<TKey, TValue> mySimpleDictionary)
                {
                    _mySimpleDictionary = mySimpleDictionary;
                    _currentValue = default;
                    _index = 0;
                }
                public TValue Current => _currentValue;

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    while ((uint)_index < (uint)_mySimpleDictionary.totalNumberOfEntries)
                    {
                        ref Entry entry = ref _mySimpleDictionary.entries![_index++];

                        if (entry.next >= -1)
                        {
                            _currentValue = entry.Value;
                            return true;
                        }
                    }

                    _index = _mySimpleDictionary.totalNumberOfEntries + 1;
                    _currentValue = default;
                    return false;
                }

                public void Reset()
                {
                    _index = 0;
                    _currentValue = default;
                }
            }
        }
    }
}
