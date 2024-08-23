using Microsoft.AspNetCore.Mvc;
using SwiftExample.Database;
using System.Text.RegularExpressions;

namespace SwiftExample.Controllers
{
    public class SwiftFileController : ControllerBase
    {
        private readonly DbContext _dbContext;
        public SwiftFileController(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.CreateDatabase();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                string content;
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    content = await reader.ReadToEndAsync();
                }

                ProcessFileContent(content);

                return Ok($"File received with {content.Length} characters.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private void ProcessFileContent(string content)
        {
            var parsedData = ParseSwiftMessage(content);

            if (parsedData.Count > 0)
            {
                StoreData(parsedData);
            }
            else
            {
                Console.WriteLine("No valid SWIFT data could be parsed from the file.");
            }
        }

        private void StoreData(Dictionary<string, string> data)
        {
            try
            {
                _dbContext.InsertMessage(data["ReferenceNumber"], data["RelatedReference"], data["Narrative"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error storing data: {ex.Message}");
            }
        }

        private Dictionary<string, string> ParseSwiftMessage(string content)
        {
            var data = new Dictionary<string, string>();

            var referenceNumberPattern = new Regex(":20:(?<refNumber>.+?)\\r?\\n");
            var relatedReferencePattern = new Regex(":21:(?<relRef>.+?)\\r?\\n");
            var narrativePattern = new Regex(":79:(?<narrative>.+?)(?=\\r?\\n:|$)", RegexOptions.Singleline);

            var refMatch = referenceNumberPattern.Match(content);
            if (refMatch.Success)
                data["ReferenceNumber"] = refMatch.Groups["refNumber"].Value.Trim();

            var relRefMatch = relatedReferencePattern.Match(content);
            if (relRefMatch.Success)
                data["RelatedReference"] = relRefMatch.Groups["relRef"].Value.Trim();

            var narMatch = narrativePattern.Match(content);
            if (narMatch.Success)
                data["Narrative"] = narMatch.Groups["narrative"].Value.Trim();

            return data;
        }
    }
}

