// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using MySimpleDictionary.Helper;
using MySimpleDictionary.Model;

BenchmarkRunner.Run<MySimpleDictionaryBenchmarkHelper>();

MySimpleDictionary<int, string> mySimpleDictionary = new MySimpleDictionary<int, string>();
mySimpleDictionary.Add(1, "First element");
mySimpleDictionary.Add(2, "Second element");
mySimpleDictionary.Add(3, "Third element");
mySimpleDictionary.Add(4, "Fourth element");
mySimpleDictionary.Add(5, "Fifth element");
mySimpleDictionary.Add(6, "Sixth element");
mySimpleDictionary.Add(7, "Seventh element");
mySimpleDictionary.Add(8, "Eighth element");
mySimpleDictionary.Add(9, "Nineth element");
mySimpleDictionary.Add(10, "Tenth element");
mySimpleDictionary.Add(11, "Eleventh element");
mySimpleDictionary.Add(12, "Twelveth element");
string value = mySimpleDictionary[12];
mySimpleDictionary[2] = "Fifteenth element";
mySimpleDictionary.WriteAllElementsFromDictionary();
if (mySimpleDictionary.ContainsKey(2))
{
    Console.WriteLine("There is element with key 2");
}

if (mySimpleDictionary.ContainsValue("Fifth element"))
{
    Console.WriteLine("There is value 'Fifth element'");
}

int numOfElementsInDictionary = mySimpleDictionary.Count();
Console.WriteLine("There are " + numOfElementsInDictionary + " elements in dictionary.");

Console.WriteLine("Iteration through dictionary by foreach");
foreach (var item in mySimpleDictionary)
{
    Console.WriteLine(item.Key + ": " + item.Value);
}

mySimpleDictionary.Remove(1);
string removedValue;
MySimpleDictionary<int, string> copyOfMySimpleDictionary = new MySimpleDictionary<int, string>(mySimpleDictionary);
MySimpleDictionary<int, string> capacityDictionary = new MySimpleDictionary<int, string>(4);
int number = copyOfMySimpleDictionary.Count;
List<int> allKeys = mySimpleDictionary.Keys;
List<string> allValues = mySimpleDictionary.Values;
mySimpleDictionary.Remove(2, out removedValue);
mySimpleDictionary.Clear();

MySimpleDictionary<int, string> ienumerableDictionary = new MySimpleDictionary<int, string>
{
    { 1, "prvi" },
    { 2, "drugi" }
};

string vrednost = ienumerableDictionary[2];
Console.WriteLine("Iterating through IEnumberable dictionary");
foreach (var item in ienumerableDictionary)
{
    Console.WriteLine(item.Key + ": " + item.Value);
}