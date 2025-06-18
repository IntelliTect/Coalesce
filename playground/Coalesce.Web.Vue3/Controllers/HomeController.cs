using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Coalesce.Web.Vue3.KernelPlugins;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Coalesce.Web.Vue.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment hostingEnvironment;

        public HomeController(IWebHostEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {

            IFileProvider provider = new PhysicalFileProvider(hostingEnvironment.WebRootPath);
            IFileInfo fileInfo = provider.GetFileInfo("index.html");
            if (!fileInfo.Exists)
            {
                return Ok("index.html not found. HMR build is probably still running for the first time. Keep refreshing...");
            }
            var readStream = fileInfo.CreateReadStream();

            return File(readStream, "text/html");
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }

        [HttpGet("/PersonChatTest")]
        public async Task<string> PersonChatTest([FromServices] Kernel kernel, [FromServices] IChatCompletionService ccs, string query)
        {
            // Enable planning
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            // Create a history store the conversation
            var history = new ChatHistory();

            // Add user input
            history.AddSystemMessage($"The current date is {DateTimeOffset.Now}");
            history.AddSystemMessage("When listing data, make additional function calls to gather subsequent pages using the page parameter. If you neglect to do this, you must state that in your response.");
            history.AddUserMessage(query);

            // Get the response from the AI
            var result = await ccs.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            return "PROMPT:\n" + query + "\n\nRESULT:\n"+ result.ToString();
        }
    }
}
