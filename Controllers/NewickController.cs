﻿using AutoMapper;
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
        [Route("InputData")]
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
        [Route("newick-db")]
        public async Task<IActionResult> Create([FromBody] AddNewickRequestDto addNewickRequestDto)
        {
            var operationId = Guid.NewGuid();
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (userName == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            var newickDomainModel = mapper.Map<Newick>(addNewickRequestDto);
            newickDomainModel.UserName = userName;
            newickDomainModel.OperationId = operationId;
            newickDomainModel.Timestamp = DateTime.UtcNow; 

            await newickRepositories.CreateAsync(newickDomainModel);

 
            var newickDto = mapper.Map<NewickDto>(newickDomainModel);
            var email = User.FindFirstValue(ClaimTypes.Email);

            Console.WriteLine($"UserId: {userName}, Email: {email}");
            return CreatedAtAction(nameof(GetById), new { id = newickDomainModel.Id }, new { newickDto, operationId });
        }


        [HttpPost]
        [Route("run-treecmp")]
        public async Task<IActionResult> RunTreeCmp([FromBody] TreeCmpRequestDto requestDto, [FromQuery] Guid operationId)
        {
           
            Console.WriteLine($"Received OperationId: {operationId}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Błędne dane wejściowe.");
                return BadRequest(ModelState);
            }

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

            var allMetrics = requestDto.rootedMetrics.Concat(requestDto.unrootedMetrics)
                                .Where(m => !string.IsNullOrEmpty(m))
                                .ToArray();
            Console.WriteLine("Łączenie metryk zakończone pomyślnie.");

            var treeCmpDomain = mapper.Map<TreeCmp>(requestDto);
            treeCmpDomain.Metrics = allMetrics;
            treeCmpDomain.InputFile = inputFilePath;
            treeCmpDomain.RefTreeFile = referenceFilePath;
            treeCmpDomain.OutputFile = outputFilePath;
            treeCmpDomain.OutputFile = outputFilePath;
            treeCmpDomain.UserName = User?.FindFirstValue(ClaimTypes.Email);

            Console.WriteLine($"Mapped TreeCmp Domain: Metrics Count = {treeCmpDomain.Metrics.Length}, InputFile = {treeCmpDomain.InputFile}, UserName = {treeCmpDomain.UserName}, OperationId = {operationId}");

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
                FileContent = Convert.ToBase64String(fileBytes),
                OperationId = operationId,
                FileGeneratedTimestamp = DateTime.UtcNow

            };
            Console.WriteLine($"Wartość UserName przed zapisem: {treeCmpDomain.UserName}");

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
                OperationId = operationId,
                InputFilePath = inputFilePath,
                ReferenceFilePath = referenceFilePath,
                OutputFilePath = outputFilePath
            });
        }


        [HttpGet("combined-newick")]
        public async Task<IActionResult> GetCombinedNewickData()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var combinedData = await newickRepositories.GetAllFinalRecordsAsync(userName);


            if (combinedData == null || !combinedData.Any())
            {
                return NotFound(new { message = "No records found" });
            }

            return Ok(combinedData);
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
