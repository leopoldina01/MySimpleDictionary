using BenchmarkDotNet.Running;
using MudBlazor;
using MudBlazor.Services;
using MySimpleDictionaryBlazorApp.Components;
using MySimpleDictionaryBlazorApp.Helper;
using MySimpleDictionaryBlazorApp.Model;

BenchmarkRunner.Run<MySimpleDictionaryBenchmarkHelper>();

//provera da li radi u konzoli----------------------------------------------------------------------
MySimpleDictionary<string, string> equalityDictionary = new MySimpleDictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
equalityDictionary.Add("prvi", "First element");
equalityDictionary.Add("drugi", "Second element");
equalityDictionary.Add("treci", "Third element");
equalityDictionary.Add("cetvrti", "Fourth element");
//bool trying = equalityDictionary.TryAdd(null, "should throw an error");
string tryGetValue;
bool tryingValue = equalityDictionary.TryGetValue("prvi", out tryGetValue);

List<string> values = equalityDictionary.Values.ToList();

foreach (var key in values)
{
    Console.WriteLine(key);
}

//provera KeyCollection
var keys = equalityDictionary.Keys;
var newValues = equalityDictionary.Values;
//provera KeyCollection
foreach (var key in keys)
{
    Console.WriteLine(key);
}
Console.WriteLine("values");
foreach (var value in newValues)
{
    Console.WriteLine(value);
}
equalityDictionary.Add("peti", "Peti element");
equalityDictionary.Remove("prvi");
foreach (var key in keys)
{
    Console.WriteLine(key);
}
Console.WriteLine("values");
foreach (var value in newValues)
{
    Console.WriteLine(value);
}
Console.WriteLine(equalityDictionary.Count);


foreach (var value in newValues)
{
    Console.WriteLine(value);
}
//provera u konzoli---------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;

    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


