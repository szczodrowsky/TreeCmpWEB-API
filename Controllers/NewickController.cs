using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Text.Json;
using TreeCmpWebAPI.Data;
using TreeCmpWebAPI.Models.Domain;
using TreeCmpWebAPI.Models.DTO;
using TreeCmpWebAPI.Repositories;
using TreeCmpWebAPI.Services;


namespace TreeCmpWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewickController : ControllerBase
    {
        private readonly NewickDbContext dbContext;
        private readonly INewickRepositories newickRepositories;
        private readonly IMapper mapper;
        private readonly ILogger<NewickController> logger;
        private readonly RabbitMqService _rabbitMqService;
        private readonly CommandBuilder _commandBuilder;


        public NewickController(NewickDbContext dbContext,
            INewickRepositories newickRepositories,
            IMapper mapper,
            ILogger<NewickController> logger,
            RabbitMqService rabbitMqService,
            CommandBuilder commandBuilder)
        {
            this.dbContext = dbContext;
            this.newickRepositories = newickRepositories;
            this.mapper = mapper;
            this.logger = logger;
            _rabbitMqService = rabbitMqService;
            _commandBuilder = commandBuilder;
        }
       
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (userName == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            var newickDomain = await newickRepositories.GetAllAsync();

            var filteredNewickDomain = newickDomain.Where(tree => tree.UserName == userName).ToList();
            return Ok(mapper.Map<List<NewickDto>>(filteredNewickDomain));
        }

        [HttpPost]

        public async Task<IActionResult> Create([FromBody] AddNewickRequestDto addNewickRequestDto)
        {

            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (userName == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            var newickDomainModel = mapper.Map<Newick>(addNewickRequestDto);
            newickDomainModel.UserName = userName;

            await newickRepositories.CreateAsync(newickDomainModel);

 
            var newickDto = mapper.Map<NewickDto>(newickDomainModel);
            var email = User.FindFirstValue(ClaimTypes.Email);

            Console.WriteLine($"UserId: {userName}, Email: {email}");
            return CreatedAtAction(nameof(GetById), new { id = newickDomainModel.Id }, newickDto);
        }


        [HttpPost]
        [Route("run-treecmp")]
        public async Task<IActionResult> RunTreeCmp([FromBody] TreeCmpRequestDto requestDto)
        {

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Błędne dane wejściowe.");
                return BadRequest(ModelState);
            }

            Console.WriteLine("Dane requestDto są prawidłowe. Kontynuujemy.");
            var uploadDirectory = Path.Combine("C:", "Users", "adams", "eng", "TreeCmp-API", "TreeCmpWebAPI", "TreeCmpWebAPI", "UploadedFiles", "TreeCmp", "bin");
            Directory.CreateDirectory(uploadDirectory);


            var inputFilePath = Path.Combine(uploadDirectory, "newick_first_tree.newick");
            System.IO.File.WriteAllText(inputFilePath, requestDto.newickFirstString);

            string? referenceFilePath = null;
            if (requestDto.comparisionMode == "-r" && !string.IsNullOrEmpty(requestDto.newickSecondString))
            {
                referenceFilePath = Path.Combine(uploadDirectory, "newick_second_tree.newick");
                System.IO.File.WriteAllText(referenceFilePath, requestDto.newickSecondString);
            }

            var outputFilePath = Path.Combine(uploadDirectory, "Output.txt");

            // Połączenie metryk
            var allMetrics = requestDto.rootedMetrics.Concat(requestDto.unrootedMetrics)
                                .Where(m => !string.IsNullOrEmpty(m))
                                .ToArray();
            Console.WriteLine("Łączenie metryk zakończone pomyślnie.");

            // Generowanie komendy
            var treeCmpDomain = mapper.Map<TreeCmp>(requestDto);
            treeCmpDomain.Metrics = allMetrics;
            treeCmpDomain.InputFile = inputFilePath;
            treeCmpDomain.RefTreeFile = referenceFilePath;
            treeCmpDomain.OutputFile = outputFilePath;
            treeCmpDomain.UserName = User?.FindFirstValue(ClaimTypes.Email);

            string command = _commandBuilder.BuildCommand(treeCmpDomain);

            _rabbitMqService.SendCommand(command);
            Console.WriteLine("Zadanie wysłane.");


            int maxWaitTime = 60000;
            int waitInterval = 5000;
            int elapsedTime = 0;

            Console.WriteLine("Czekamy na wygenerowanie pliku wynikowego...");
            while (!System.IO.File.Exists(outputFilePath) && elapsedTime < maxWaitTime)
            {
                await Task.Delay(waitInterval);
                elapsedTime += waitInterval;
            }

            if (!System.IO.File.Exists(outputFilePath))
            {
                return NotFound(new { Message = "Plik wynikowy nie został znaleziony po upływie limitu czasu." });
            }


            var fileBytes = await System.IO.File.ReadAllBytesAsync(outputFilePath);
            Console.WriteLine("Odczytano plik wynikowy.");

            var fileDto = new NewickResponseFile
            {
                FileName = Path.GetFileName(outputFilePath),
                FileExtension = Path.GetExtension(outputFilePath),
                FilePath = outputFilePath,
                FileContent = Convert.ToBase64String(fileBytes)
            };
            Console.WriteLine($"Wartość UserName przed zapisem: {treeCmpDomain.UserName}");
            // Jeśli użytkownik jest zalogowany, zapisujemy plik w bazie danych
            if (!string.IsNullOrEmpty(treeCmpDomain.UserName))
            {
                fileDto.UserName = treeCmpDomain.UserName;
                await newickRepositories.SaveOutputFileAsync(fileDto);
                Console.WriteLine("Plik zapisany w bazie danych.");
            }
            else
            {
                Console.WriteLine("UserName jest pusty lub null, nie zapisujemy pliku w bazie danych.");
            }

            try
            {
                if (System.IO.File.Exists(inputFilePath))
                    System.IO.File.Delete(inputFilePath);

                if (!string.IsNullOrEmpty(referenceFilePath) && System.IO.File.Exists(referenceFilePath))
                    System.IO.File.Delete(referenceFilePath);

                if (System.IO.File.Exists(outputFilePath))
                    System.IO.File.Delete(outputFilePath);

                Console.WriteLine("Pliki tymczasowe usunięte.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas usuwania plików: {ex.Message}");
            }

            return Ok(new
            {
                Message = "Zadanie zostało przetworzone",
                FileName = fileDto.FileName,
                FileContent = fileDto.FileContent,
                InputFilePath = inputFilePath,
                ReferenceFilePath = referenceFilePath,
                OutputFilePath = outputFilePath
            });
        }

        /// Może zostanie
        [HttpDelete]
        [Authorize]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var newickDomainModel = await newickRepositories.DeleteAsync(id);

            if (newickDomainModel == null)
            {
                return NoContent();
            }
            return Ok(mapper.Map<NewickDto>(newickDomainModel));
        }
        
        //Raczej zbędne i polecą 
        [HttpPut]
        [Authorize]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateNewickRequestDto updateNewickRequestDto)
        {

            var newickDomainModel = mapper.Map<Newick>(updateNewickRequestDto);
            newickDomainModel = await newickRepositories.UpdateAsync(id,newickDomainModel);
            if (newickDomainModel == null)
            { 
                return NotFound();
            }
            return Ok(mapper.Map<NewickDto>(newickDomainModel));
        }


        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var newickDomain = await newickRepositories.GetByIdAsync(id);
            if (newickDomain == null)
            {
                return NotFound();
            }
            return Ok(mapper.Map<List<NewickDto>>(newickDomain));
        }

    }
}
