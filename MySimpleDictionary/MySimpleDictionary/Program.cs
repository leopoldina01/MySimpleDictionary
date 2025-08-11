// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using MySimpleDictionary.Helper;
using MySimpleDictionary.Model;

BenchmarkRunner.Run<MySimpleDictionaryBenchmarkHelper>();

MySimpleDictionary<int, string> mySimpleDictionary = new MySimpleDictionary<int, string>();
mySimpleDictionary.Add(1, "Prvi element");
mySimpleDictionary.Add(2, "Drugi element");
mySimpleDictionary.Add(3, "Drugi element");
mySimpleDictionary.Add(4, "Drugi element");
mySimpleDictionary.Add(5, "Drugi element");
mySimpleDictionary.Add(6, "Drugi element");
mySimpleDictionary.Add(7, "Drugi element");
mySimpleDictionary.Add(8, "Drugi element");
mySimpleDictionary.Add(9, "Drugi element");
mySimpleDictionary.Add(10, "Drugi element");
mySimpleDictionary.Add(11, "Drugi element");
mySimpleDictionary.Add(12, "Drugi element");
string value = mySimpleDictionary[12];
mySimpleDictionary[2] = "Petnaesti element";
mySimpleDictionary.WriteAllElementsFromDictionary();
if (mySimpleDictionary.ContainsKey(2))
{
    Console.WriteLine("Ima element sa kljucem 2");
}

if (mySimpleDictionary.ContainsValue("Drugi element"))
{
    Console.WriteLine("Ima vrednost 'Drugi element'");
}

int numOfElementsInDictionary = mySimpleDictionary.Count();
Console.WriteLine("There are " + numOfElementsInDictionary + " elements in dictionary.");

Console.WriteLine("Iteriranje kroz dictionary");
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
Console.WriteLine("Iteriranje kroz dictionary");
mySimpleDictionary.Clear();
Console.WriteLine("Hello, World!");

MySimpleDictionary<int, string> ienumerableDictionary = new MySimpleDictionary<int, string>
{
    { 1, "prvi" },
    { 2, "drugi" }
};

Console.WriteLine("Iteriranje kroz ienumerable dictionary");
foreach (var item in ienumerableDictionary)
{
    Console.WriteLine(item.Key + ": " + item.Value);
}