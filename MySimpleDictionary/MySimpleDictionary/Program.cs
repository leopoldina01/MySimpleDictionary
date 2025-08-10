// See https://aka.ms/new-console-template for more information
using MySimpleDictionary.Model;

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
mySimpleDictionary.WriteAllElementsFromDictionary();
Console.WriteLine("Hello, World!");
