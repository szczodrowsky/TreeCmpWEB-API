using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TreeCmpWebAPI.Data;
using TreeCmpWebAPI.Models.Domain;
using TreeCmpWebAPI.Models.DTO;
using TreeCmpWebAPI.Repositories;
using TreeCmpWebAPI.Services;



///// Dodanie drugiego controlera modelu jako output konsolowej apki/obejrzec to o upload fotek myśle?? dodać walidacje dla endpointów ew potem ||| filtering może potem ale teraz nie widze takiego sensu
/// Dodanie logów to tez raczej potem jako zabawa ew, jak uznamy ze warto 

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
        public async Task<IActionResult> GetAll()
        {
            var newickDomain = await newickRepositories.GetAllAsync(); 
            return Ok(mapper.Map<List<NewickDto>>(newickDomain));
        }

        [HttpGet]
        [Route("{id:Guid}")]
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var newickDomain = await newickRepositories.GetByIdAsync(id);
            if (newickDomain == null) { 
                return NotFound();
            }
            return Ok(mapper.Map<List<NewickDto>>(newickDomain));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddNewickRequestDto addNewickRequestDto)
        {
            var newickDomainModel = mapper.Map<Newick>(addNewickRequestDto);
            await newickRepositories.CreateAsync(newickDomainModel);
            var newickDto =mapper.Map<NewickDto>(newickDomainModel);
            return CreatedAtAction(nameof(GetById), new { id = newickDomainModel.Id }, newickDomainModel);
        }



        [HttpPost]
        [Route("run-treecmp")]
        public IActionResult RunTreeCmp([FromBody] TreeCmpRequestDto requestDto)
        {
          
            var uploadDirectory = Path.Combine("C:", "Users", "adams", "eng", "worker", "WorkerApp", "WorkerApp", "UploadedFiles", "TreeCmp", "bin");
            Directory.CreateDirectory(uploadDirectory); 

  
            var inputFilePath = Path.Combine(uploadDirectory, "newick_first_tree.newick");
            System.IO.File.WriteAllText(inputFilePath, requestDto.NewickFirstString);

   
            string? referenceFilePath = null;
            if (requestDto.ComparisonMode == "-r" && !string.IsNullOrEmpty(requestDto.NewickSecondString))
            {
                referenceFilePath = Path.Combine(uploadDirectory, "newick_second_tree.newick");
                System.IO.File.WriteAllText(referenceFilePath, requestDto.NewickSecondString);
            }

 
            var outputFilePath = Path.Combine(uploadDirectory, "Output.txt");

    
            requestDto.InputFile = inputFilePath;  
            requestDto.RefTreeFile = referenceFilePath; 
            requestDto.OutputFile = outputFilePath;  


            var allMetrics = requestDto.RootedMetrics.Concat(requestDto.UnrootedMetrics).Where(m => !string.IsNullOrEmpty(m)).ToArray();
            requestDto.Metrics = allMetrics;

            string command = _commandBuilder.BuildCommand(requestDto);


            _rabbitMqService.SendCommand(command);

            return Ok(new
            {
                Message = "Zadanie zostało wysłane do przetwarzania",
                InputFilePath = inputFilePath,
                ReferenceFilePath = referenceFilePath,
                OutputFilePath = outputFilePath
            });
        }


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
    }
}
