using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using FileDownloadExplorer.Models;

namespace FileDownloadExplorer.Pages
{

    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IndexModel(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public List<string> Names { get; set; }
        public List<string> CompanyIds { get; set; }
        public List<string> FileExtensions { get; set; }
        public string SelectedInstance { get; set; }
        public string SelectedCompanyId { get; set; }
        public string SelectedDate { get; set; }
        public string EnteredKeyword { get; set; }
        public string SelectedFileExtension { get; set; }
        public string PrefixDefaultConcatenated { get; set; }
        public string Folder { get; set; }
        public List<string> FilteredFiles { get; set; }
        public bool ShowWarning { get; set; }

        public async Task OnGet()
        {
            await LoadConfig();
        }

        public async Task OnPost(string instances, string companyIds, string date, string keyword, string fileExtensions)
        {
            await LoadConfig();
            SelectedInstance = instances;
            SelectedCompanyId = companyIds;
            SelectedDate = date;
            EnteredKeyword = keyword;
            SelectedFileExtension = fileExtensions;

            if (!string.IsNullOrEmpty(instances) && !string.IsNullOrEmpty(keyword))
            {
                var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "source", "config.json");
                var json = await System.IO.File.ReadAllTextAsync(filePath);
                var config = JsonSerializer.Deserialize<ConfigModel>(json);
                var instance = config.Instances.FirstOrDefault(i => i.Name == instances);
                if (instance != null)
                {
                    Folder = instance.Folder;
                    PrefixDefaultConcatenated = instance.PrefixDefault + keyword;

                    if (Directory.Exists(Folder))
                    {
                        var files = Directory.GetFiles(Folder, $"*{keyword}*.{fileExtensions}");
                        FilteredFiles = files.ToList();
                    }
                }
            }

            ShowWarning = string.IsNullOrEmpty(date) || string.IsNullOrEmpty(keyword);
        }

        public IActionResult OnPostDownload(string filepath)
        {
            if (System.IO.File.Exists(filepath))
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(filepath, FileMode.Open))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;
                var contentType = "APPLICATION/octet-stream";
                var fileName = Path.GetFileName(filepath);
                return File(memory, contentType, fileName);
            }
            return RedirectToPage();
        }

        private async Task LoadConfig()
        {
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "source", "config.json");
            var json = await System.IO.File.ReadAllTextAsync(filePath);
            var config = JsonSerializer.Deserialize<ConfigModel>(json);
            Names = config.Instances.Select(i => i.Name).ToList();
            CompanyIds = config.Instances.Select(i => i.CompanyId).Distinct().ToList();
            FileExtensions = config.FileExtension.Split(';').ToList();
        }
    }

}
