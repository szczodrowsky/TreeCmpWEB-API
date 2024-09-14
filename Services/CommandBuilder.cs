using TreeCmpWebAPI.Models.DTO;

public class CommandBuilder
{
    private readonly string jarFilePath = "treeCmp.jar"; // Ścieżka do JAR

    public string BuildCommand(TreeCmpRequestDto requestDto)
    {
        // Budowanie komendy dla trybu porównania
        string comparisonModeCommand = GetComparisonModeCommand(requestDto);

        // Nazwy plików, które zostały zapisane na dysku
        string inputFile = "newick_first_tree.newick"; // Plik wejściowy
        string outputFile = "Output.txt";              // Plik wynikowy
        string? refTreeFile = requestDto.ComparisonMode == "-r" ? "newick_second_tree.newick" : null; // Plik referencyjny, jeśli tryb -r

        // Łączenie metryk w jeden ciąg znaków, oddzielone spacjami
        string metrics = string.Join(" ", requestDto.Metrics.Where(m => !string.IsNullOrEmpty(m)));

        // Budowanie podstawowej komendy
        var command = $"java -jar {jarFilePath} {comparisonModeCommand}";

        // Dodanie pliku referencyjnego tylko raz w odpowiednim miejscu (jeśli tryb to -r)
        if (requestDto.ComparisonMode == "-r" && !string.IsNullOrEmpty(refTreeFile))
        {
            command += $" {refTreeFile}";
        }

        // Dodanie pozostałych argumentów (metryki, pliki wejściowe/wyjściowe)
        command += $" -d {metrics} -i {inputFile} -o {outputFile}";

        // Dodanie flag booleanowych
        if (requestDto.normalizedDistances == true)
        {
            command += " -N";  // Znormalizowane odległości
        }

        if (requestDto.pruneTrees == true)
        {
            command += " -P";  // Prune compared trees
        }

        if (requestDto.includeSummary == true)
        {
            command += " -I";  // Include summary
        }

        if (requestDto.zeroWeightsAllowed == true)
        {
            command += " -W";  // Allow zero value weights
        }

        return command;
    }

    // Metoda pomocnicza do generowania komendy trybu porównania
    private string GetComparisonModeCommand(TreeCmpRequestDto requestDto)
    {
        switch (requestDto.ComparisonMode)
        {
            case "-s":
                return "-s"; // Tryb -s (overlapping pair comparison)

            case "-w":
                if (string.IsNullOrEmpty(requestDto.windowWidth))
                {
                    throw new ArgumentException("Rozmiar okna (WindowWidth) jest wymagany dla trybu -w");
                }
                return $"-w {requestDto.windowWidth}"; // Tryb -w z szerokością okna

            case "-m":
                return "-m"; // Tryb -m (matrix comparison)

            case "-r":
                if (string.IsNullOrEmpty(requestDto.NewickSecondString))
                {
                    throw new ArgumentException("Plik referencyjny (NewickSecondString) jest wymagany dla trybu -r");
                }
                return "-r"; // Tryb -r (reference tree mode)

            default:
                throw new ArgumentException("Nieprawidłowy tryb porównania");
        }
    }
}
