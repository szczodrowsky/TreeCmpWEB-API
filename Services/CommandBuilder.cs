using TreeCmpWebAPI.Models.DTO;
using TreeCmpWebAPI.Models.Domain;

public class CommandBuilder
{
    private readonly string jarFilePath = "treeCmp.jar"; // Ścieżka do JAR

    public string BuildCommand(TreeCmp request)
    {
        // Budowanie komendy dla trybu porównania
        string comparisonModeCommand = GetComparisonModeCommand(request);

        string inputFile = "newick_first_tree.newick"; 
        string outputFile = "Output.txt";  
        string? refTreeFile = request.comparisionMode == "-r" ? "newick_second_tree.newick" : null; 

        string metrics = string.Join(" ", request.Metrics.Where(m => !string.IsNullOrEmpty(m)));


        var command = $"java -jar {jarFilePath} {comparisonModeCommand}";

        if (request.comparisionMode == "-r" && !string.IsNullOrEmpty(refTreeFile))
        {
            command += $" {refTreeFile}";
        }

        command += $" -d {metrics} -i {inputFile} -o {outputFile}";


        if (request.normalizedDistances == true)
        {
            command += " -N";  
        }

        if (request.pruneTrees == true)
        {
            command += " -P";  
        }

        if (request.includeSummary == true)
        {
            command += " -I";  
        }

        if (request.zeroWeightsAllowed == true)
        {
            command += " -W";  
        }

        return command;
    }

    private string GetComparisonModeCommand(TreeCmp request)
    {
        switch (request.comparisionMode)
        {
            case "-s":
                return "-s"; 

            case "-w":
                if (string.IsNullOrEmpty(request.windowWidth))
                {
                    throw new ArgumentException("Rozmiar okna (WindowWidth) jest wymagany dla trybu -w");
                }
                return $"-w {request.windowWidth}"; 

            case "-m":
                return "-m"; 

            case "-r":
                if (string.IsNullOrEmpty(request.newickSecondString))
                {
                    throw new ArgumentException("Plik referencyjny (NewickSecondString) jest wymagany dla trybu -r");
                }
                return "-r";

            default:
                throw new ArgumentException("Nieprawidłowy tryb porównania");
        }
    }
}
