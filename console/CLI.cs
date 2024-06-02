using PCLStorage;

using RoboZZle.Core;
using RoboZZle.ViewModels;

var roamingSettings = new InMemorySettings();
var never = new TaskCompletionSource<ILocalSolutions>();
string cachePath = Path.Join(
	Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
	"RoboZZle",
	"Cache");
Directory.CreateDirectory(cachePath);
var cacheFolder = new FileSystemFolder(cachePath);
var app = new ApplicationViewModel(roamingSettings, never.Task, cacheFolder: cacheFolder);
await app.Initialize();