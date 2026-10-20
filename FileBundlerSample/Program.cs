using System.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

static string QuoteArgument(string value)
{
    return "\"" + value.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"") + "\"";
}

var rootCommand = new RootCommand("Code file bundling tool");

var bundleCommand = new Command("bundle", "Bundle code files");
var languageOption = new Option<string[]>("--language")
{
    Description = "Programming languages to include",
    Required = true,
    AllowMultipleArgumentsPerToken = true
};

languageOption.Aliases.Add("-l");

bundleCommand.Options.Add(languageOption);


var outputOption = new Option<string>("--output")
{
    Description = "Output bundle file path"
};

outputOption.Aliases.Add("-o");

bundleCommand.Options.Add(outputOption);


var noteOption = new Option<bool>("--note")
{
    Description = "Include source file information as comments"
};

noteOption.Aliases.Add("-n");

bundleCommand.Options.Add(noteOption);


var sortOption = new Option<string>("--sort")
{
    Description = "Sort files by name or type",
    DefaultValueFactory = _ => "name"
};

sortOption.Aliases.Add("-s");

bundleCommand.Options.Add(sortOption);


var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines")
{
    Description = "Remove empty lines from source files"
};

removeEmptyLinesOption.Aliases.Add("-r");

bundleCommand.Options.Add(removeEmptyLinesOption);


var authorOption = new Option<string>("--author")
{
    Description = "Name of the bundle author"
};

authorOption.Aliases.Add("-a");

bundleCommand.Options.Add(authorOption);


rootCommand.Subcommands.Add(bundleCommand);

var createRspCommand = new Command("create-rsp", "Create a response file");


createRspCommand.SetAction(parseResult =>
{
    Console.WriteLine("Enter languages (for example: cs or all):");
    var languages = Console.ReadLine();

    Console.WriteLine("Enter output file path:");
    var output = Console.ReadLine();

    Console.WriteLine("Include source notes? (y/n):");
    var note = Console.ReadLine();

    Console.WriteLine("Sort by name or type:");
    var sort = Console.ReadLine();

    Console.WriteLine("Remove empty lines? (y/n):");
    var removeEmptyLines = Console.ReadLine();

    Console.WriteLine("Enter author name (or leave empty):");
    var author = Console.ReadLine();

    // Validate required values
    if (string.IsNullOrWhiteSpace(languages) ||
        string.IsNullOrWhiteSpace(output))
    {
        Console.WriteLine(
            "Error: Languages and output are required.");
        return;
    }

    languages = languages.Trim();
    output = output.Trim();

    // Split and validate languages
    var languageList = languages.Split(
        new[] { ' ', ',', ';' },
        StringSplitOptions.RemoveEmptyEntries |
        StringSplitOptions.TrimEntries);

    if (languageList.Length == 0)
    {
        Console.WriteLine(
            "Error: At least one language is required.");
        return;
    }

    bool containsAll = languageList.Any(
        lang => string.Equals(
            lang, "all",
            StringComparison.OrdinalIgnoreCase));

    if (containsAll && languageList.Length > 1)
    {
        Console.WriteLine(
            "Error: 'all' cannot be combined with other languages.");
        return;
    }

    var supportedLanguages = new HashSet<string>(
        new[]
        {
            "cs", "csharp",
            "js", "javascript",
            "ts", "typescript",
            "py", "python",
            "java", "html", "css",
            "c", "cpp"
        },
        StringComparer.OrdinalIgnoreCase);

    var invalidLanguages = languageList
        .Where(lang =>
            !string.Equals(
                lang, "all",
                StringComparison.OrdinalIgnoreCase) &&
            !supportedLanguages.Contains(lang))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    if (invalidLanguages.Length > 0)
    {
        Console.WriteLine(
            $"Error: Unsupported language(s): " +
            $"{string.Join(", ", invalidLanguages)}");
        return;
    }

    // Validate yes/no answers
    bool IsYes(string? value) =>
        string.Equals(
            value?.Trim(), "y",
            StringComparison.OrdinalIgnoreCase);

    bool IsNo(string? value) =>
        string.Equals(
            value?.Trim(), "n",
            StringComparison.OrdinalIgnoreCase);

    if (!IsYes(note) && !IsNo(note))
    {
        Console.WriteLine(
            "Error: Please enter y or n for source notes.");
        return;
    }

    if (!IsYes(removeEmptyLines) &&
        !IsNo(removeEmptyLines))
    {
        Console.WriteLine(
            "Error: Please enter y or n for empty lines.");
        return;
    }

    // Validate sort
    sort = string.IsNullOrWhiteSpace(sort)
        ? "name"
        : sort.Trim().ToLowerInvariant();

    if (sort != "name" && sort != "type")
    {
        Console.WriteLine(
            "Error: Sort must be name or type.");
        return;
    }

    // Get response file name
    Console.WriteLine("Enter response file name:");
    var fileName = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(fileName))
    {
        Console.WriteLine(
            "Error: Response file name is required.");
        return;
    }

    fileName = fileName.Trim();

    if (!fileName.EndsWith(
        ".rsp", StringComparison.OrdinalIgnoreCase))
    {
        fileName += ".rsp";
    }

    // Build response-file command
    var languageArguments = string.Join(
        " ",
        languageList.Select(QuoteArgument));

    var responseCommand =
        $"bundle --language {languageArguments}" +
        $" --output {QuoteArgument(output)}";

    if (IsYes(note))
        responseCommand += " --note";

    responseCommand += $" --sort {sort}";

    if (IsYes(removeEmptyLines))
        responseCommand += " --remove-empty-lines";

    if (!string.IsNullOrWhiteSpace(author))
    {
        responseCommand +=
            $" --author {QuoteArgument(author.Trim())}";
    }

    // Create response file
    try
    {
        File.WriteAllText(fileName, responseCommand);

        Console.WriteLine(
            "Response file created successfully: " +
            $"{Path.GetFullPath(fileName)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Error creating response file: {ex.Message}");
    }
});




/*createRspCommand.SetAction(parseResult =>
{
    Console.WriteLine("Enter languages (for example: cs or all):");
    var languages = Console.ReadLine();

    Console.WriteLine("Enter output file path:");
    var output = Console.ReadLine();

    Console.WriteLine("Include source notes? (y/n):");
    var note = Console.ReadLine();

    Console.WriteLine("Sort by name or type:");
    var sort = Console.ReadLine();

    Console.WriteLine("Remove empty lines? (y/n):");
    var removeEmptyLines = Console.ReadLine();

    Console.WriteLine("Enter author name (or leave empty):");
    var author = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(languages) ||
        string.IsNullOrWhiteSpace(output))
    {
        Console.WriteLine("Error: Languages and output are required.");
        return;
    }

    if (string.IsNullOrWhiteSpace(sort))
    {
        sort = "name";
    }

    if (!string.Equals(sort, "name",
        StringComparison.OrdinalIgnoreCase) &&
    !string.Equals(sort, "type",
        StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Error: Sort must be name or type.");
        return;
    }

    sort = sort.ToLowerInvariant();

    Console.WriteLine("Enter response file name:");
    var fileName = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(fileName))
    {
        Console.WriteLine("Error: Response file name is required.");
        return;
    }

    if (!fileName.EndsWith(".rsp",
    StringComparison.OrdinalIgnoreCase))
    {
        fileName += ".rsp";
    }

    var languageList = languages.Split(
    new[] { ' ', ',', ';' },
    StringSplitOptions.RemoveEmptyEntries |
    StringSplitOptions.TrimEntries);

    var languageArguments = string.Join(
        " ",
        languageList.Select(QuoteArgument));

    var responseCommand =
        $"bundle --language {languageArguments}" +
        $" --output {QuoteArgument(output)}";

    if (string.Equals(note, "y", StringComparison.OrdinalIgnoreCase))
        responseCommand += " --note";

    responseCommand += $" --sort {sort}";

    if (string.Equals(removeEmptyLines, "y",
        StringComparison.OrdinalIgnoreCase))
        responseCommand += " --remove-empty-lines";

    if (!string.IsNullOrWhiteSpace(author))
    {
        responseCommand += $" --author {QuoteArgument(author.Trim())}";
    }


    try
    {
        File.WriteAllText(fileName, responseCommand);

        Console.WriteLine(
            $"Response file created successfully: {Path.GetFullPath(fileName)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Error creating response file: {ex.Message}");
    }
});*/



/*createRspCommand.SetAction(parseResult =>
{
    Console.WriteLine("Enter languages (for example: cs or all):");
    var languages = Console.ReadLine();

    Console.WriteLine("Enter output file path:");
    var output = Console.ReadLine();

    Console.WriteLine("Include source notes? (y/n):");
    var note = Console.ReadLine();

    Console.WriteLine("Sort by name or type:");
    var sort = Console.ReadLine();

    Console.WriteLine("Remove empty lines? (y/n):");
    var removeEmptyLines = Console.ReadLine();

    Console.WriteLine("Enter author name (or leave empty):");
    var author = Console.ReadLine();

    // Validate required values
    if (string.IsNullOrWhiteSpace(languages) ||
        string.IsNullOrWhiteSpace(output))
    {
        Console.WriteLine(
            "Error: Languages and output are required.");
        return;
    }

    languages = languages.Trim();
    output = output.Trim();

    // Split the language list
    var languageList = languages.Split(
        new[] { ' ', ',', ';' },
        StringSplitOptions.RemoveEmptyEntries |
        StringSplitOptions.TrimEntries);

    // Validate the language list
    if (languageList.Length == 0)
    {
        Console.WriteLine("Error: At least one language is required.");
        return;
    }

    bool containsAll = languageList.Any(
        language => string.Equals(
            language, "all",
            StringComparison.OrdinalIgnoreCase));

    if (containsAll && languageList.Length > 1)
    {
        Console.WriteLine(
            "Error: 'all' cannot be combined with other languages.");
        return;
    }

    // Validate yes/no answers
    bool IsYes(string? value) =>
        string.Equals(
            value?.Trim(), "y",
            StringComparison.OrdinalIgnoreCase);

    bool IsNo(string? value) =>
        string.Equals(
            value?.Trim(), "n",
            StringComparison.OrdinalIgnoreCase);

    if (!IsYes(note) && !IsNo(note))
    {
        Console.WriteLine("Error: Please enter y or n for source notes.");
        return;
    }

    if (!IsYes(removeEmptyLines) && !IsNo(removeEmptyLines))
    {
        Console.WriteLine("Error: Please enter y or n for empty lines.");
        return;
    }

    // Validate sorting option
    if (string.IsNullOrWhiteSpace(sort))
    {
        sort = "name";
    }

    sort = sort.Trim().ToLowerInvariant();

    if (sort != "name" && sort != "type")
    {
        Console.WriteLine("Error: Sort must be name or type.");
        return;
    }

    // Get response file name
    Console.WriteLine("Enter response file name:");
    var fileName = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(fileName))
    {
        Console.WriteLine("Error: Response file name is required.");
        return;
    }

    fileName = fileName.Trim();

    if (!fileName.EndsWith(
        ".rsp", StringComparison.OrdinalIgnoreCase))
    {
        fileName += ".rsp";
    }

    // Build the response-file command
    var responseCommand =
        $"bundle --language {string.Join(" ", languageList)}" +
        $" --output {QuoteArgument(output)}";

    if (IsYes(note))
    {
        responseCommand += " --note";
    }

    responseCommand += $" --sort {sort}";

    if (IsYes(removeEmptyLines))
    {
        responseCommand += " --remove-empty-lines";
    }

    if (!string.IsNullOrWhiteSpace(author))
    {
        responseCommand += $" --author {QuoteArgument(author.Trim())}";
    }

    // Create the response file
    try
    {
        File.WriteAllText(fileName, responseCommand);

        Console.WriteLine(
            $"Response file created successfully: " +
            $"{Path.GetFullPath(fileName)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Error creating response file: {ex.Message}");
    }
});
*/

rootCommand.Subcommands.Add(createRspCommand);



bundleCommand.SetAction(parseResult =>
{
    var language = parseResult.GetValue(languageOption);
    var outputPath = parseResult.GetValue(outputOption);

    if (string.IsNullOrWhiteSpace(outputPath))
    {
        Console.WriteLine(
            "Error: Please specify an output path using --output.");
        return;
    }

    string fullOutputPath;

    try
    {
        fullOutputPath = Path.GetFullPath(outputPath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: Invalid output path: {ex.Message}");
        return;
    }
    var currentDirectory = Directory.GetCurrentDirectory();

    var excludedDirectories = new[] { "bin", "obj", "debug" };


    var languageExtensions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["cs"] = new[] { ".cs" },
        ["csharp"] = new[] { ".cs" },
        ["js"] = new[] { ".js" },
        ["javascript"] = new[] { ".js" },
        ["ts"] = new[] { ".ts" },
        ["typescript"] = new[] { ".ts" },
        ["py"] = new[] { ".py" },
        ["python"] = new[] { ".py" },
        ["java"] = new[] { ".java" },
        ["html"] = new[] { ".html", ".htm" },
        ["css"] = new[] { ".css" },
        ["c"] = new[] { ".c" },
        ["cpp"] = new[] { ".cpp", ".h", ".hpp" }
    };


    if (language == null || language.Length == 0)
    {
        Console.WriteLine("Error: At least one language is required.");
        return;
    }

    var hasAll = language.Any(lang =>
    lang.Equals("all", StringComparison.OrdinalIgnoreCase));

    if (hasAll && language.Length > 1)
    {
        Console.WriteLine(
            "Error: 'all' cannot be combined with other languages.");
        return;
    }

    if (!hasAll)
    {
        var invalidLanguages = language
            .Where(lang => !languageExtensions.ContainsKey(lang))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (invalidLanguages.Length > 0)
        {
            Console.WriteLine(
                $"Error: Unsupported language(s): {string.Join(", ", invalidLanguages)}");
            return;
        }
    }



    var files = Directory
        .GetFiles(currentDirectory, "*", SearchOption.AllDirectories)
         .Where(file => !string.Equals(
        Path.GetFullPath(file),
        fullOutputPath,
        StringComparison.OrdinalIgnoreCase))
        .Where(file =>
            !Path.GetRelativePath(currentDirectory, file)
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(part => excludedDirectories.Contains(
                    part, StringComparer.OrdinalIgnoreCase)))
        .Where(file =>
        {
            var extension = Path.GetExtension(file);

            var isCodeFile = languageExtensions.Values
                .SelectMany(extensions => extensions)
                .Contains(extension, StringComparer.OrdinalIgnoreCase);

            if (language.Any(lang =>
                lang.Equals("all", StringComparison.OrdinalIgnoreCase)))
            {
                return isCodeFile;
            }

            return language
                .Where(lang => languageExtensions.ContainsKey(lang))
                .SelectMany(lang => languageExtensions[lang])
                .Contains(extension, StringComparer.OrdinalIgnoreCase);
        })
        .ToArray();

    if (files.Length == 0)
    {
        Console.WriteLine("No matching code files were found.");
        return;
    }


    var sort = parseResult.GetValue(sortOption);

    if (string.Equals(sort, "name", StringComparison.OrdinalIgnoreCase))
    {
        files = files
            .OrderBy(file => Path.GetFileName(file),
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
    else if (string.Equals(
        sort,
        "type",
        StringComparison.OrdinalIgnoreCase))
    {
        files = files
            .OrderBy(file => Path.GetExtension(file),
                StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => Path.GetFileName(file),
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
    else
    {
        Console.WriteLine(
            "Error: sort must be 'name' or 'type'.");
        return;
    }


    var bundleContent = new System.Text.StringBuilder();

    var author = parseResult.GetValue(authorOption);

    if (!string.IsNullOrWhiteSpace(author))
    {
        bundleContent.AppendLine($"// Author: {author}");
        bundleContent.AppendLine();
    }


    var includeNote = parseResult.GetValue(noteOption);

    foreach (var file in files)
    {
        if (includeNote)
        {
            var relativePath = Path.GetRelativePath(currentDirectory, file);
            var fileName = Path.GetFileName(file);

            bundleContent.AppendLine($"// File: {fileName}");
            bundleContent.AppendLine($"// Path: {relativePath}");
        }


        string content;

        try
        {
            content = File.ReadAllText(file);
        }
        catch (IOException ex)
        {
            Console.WriteLine(
                $"Error reading file '{file}': {ex.Message}");
            continue;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine(
                $"Access denied for file '{file}': {ex.Message}");
            continue;
        }
        var removeEmptyLines = parseResult.GetValue(removeEmptyLinesOption);

        if (removeEmptyLines)
        {
            content = string.Join(
                Environment.NewLine,
                content.Split(new[] { "\r\n", "\n", "\r" },
                    StringSplitOptions.None)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
            );
        }

        bundleContent.AppendLine(content);
        bundleContent.AppendLine();
    }



    try
    {
        var outputDirectory = Path.GetDirectoryName(fullOutputPath);

        if (!string.IsNullOrEmpty(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        File.WriteAllText(fullOutputPath, bundleContent.ToString());

        Console.WriteLine($"Bundle created successfully: {fullOutputPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating bundle: {ex.Message}");
    }


    Console.WriteLine($"Found {files.Length} files.");

});



await rootCommand.Parse(args).InvokeAsync();