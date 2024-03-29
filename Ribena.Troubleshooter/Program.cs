using Sharprompt;

// This is a simple application that will handle cases of the application needing
// to be troubleshooted.

Dictionary<string, Action> troubleShootOptions = new()
{
    ["Install Ribena"] = () =>
    {

    },
    ["Module failing to load due to \"Ribena.dll\""] = () =>
    {
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var ribenaModuleFolder = Path.Combine(appDataFolder, "Ribena", "modules");

        if (!Directory.Exists(ribenaModuleFolder))
        {
            Console.WriteLine("Ribena module folder does not exist.");
            return;
        }

        // Firstly, check the root folder and make sure "Ribena.dll" exists.
        // NOTE: This executable is bundled with the main executable, so the DLL
        // should be in this directory.
        var actualDll = new FileInfo(Path.Combine(Environment.CurrentDirectory, "Ribena.dll"));
        if (!actualDll.Exists)
        {
            Console.WriteLine("This executable is in the wrong location. It must be in the same directory as Ribena.dll");
            return;
        }

        // Make sure to copy it to the root.
        try
        {
            actualDll.CopyTo(ribenaModuleFolder);
        }
        catch (IOException)
        {
            // Assume it already exists there.
            Console.WriteLine($"The required DLL exists in the root folder.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong copying Ribena.dll to the root module folder.");
            Console.WriteLine($"  Message: {e.Message}");
            return;
        }

        var allChildDirs = Directory.GetDirectories(ribenaModuleFolder);

        foreach (var childDir in allChildDirs)
        {
            var folderName = new DirectoryInfo(childDir).Name;
            // Copy the file to all child directorys
            try
            {
                actualDll.CopyTo(childDir);
            }
            catch (IOException)
            {
                // Assume it already exists there.
                Console.WriteLine($"The required DLL exists in the \"{folderName}\" folder.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something went wrong copying Ribena.dll to the \"{folderName}\" folder.");
                Console.WriteLine($"  Message: {e.Message}");
                return;
            }
        }
    },
};

bool running = true;

while (running)
{
    Console.Clear();

    var input = Prompt.Select("Select an option", [.. troubleShootOptions.Keys, "exit"]);
    if (input == "exit")
        break;
    var actionToComplete = troubleShootOptions[input];
    actionToComplete();

    if (!Prompt.Confirm("Continue?", true))
        break;
}
