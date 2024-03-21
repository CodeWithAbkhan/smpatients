using System.Diagnostics;
using DtronixCommon;
using DtronixPdf;
using DtronixPdf.ImageSharp;
using SixLabors.ImageSharp;


namespace smpatients
{
    public class PdfImageConverter
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly Stopwatch sw = new Stopwatch();

        public PdfImageConverter(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<int> ConvertPdfToImages(string pdfFilePath, int iterations, int appId)
        {
            try{
                string outputDirectory = Path.Combine("wwwroot", "images");
                var localFilePath = Path.Combine( "wwwroot","files", $"{Path.GetFileName(pdfFilePath)}");
                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);
                Console.WriteLine($"RenderViewport Benchmark {pdfFilePath}");

                using (var document = PdfDocument.Load(localFilePath, null))
                {
                    if (document == null)
                    {
                        Console.WriteLine($"Failed to load PDF document.{pdfFilePath}");
                        return 0; // Or handle the error appropriately
                    }
                    for (int pageIndex = 0; pageIndex < document.Pages; pageIndex++)
                    {
                        for (int iteration = 0; iteration < iterations; iteration++)
                        {
                            using (var page = document.GetPage(pageIndex))
                            {
                                if (page == null)
                                {
                                    Console.WriteLine($"Failed to load page {pageIndex + 1}.");
                                    continue; // Skip to the next iteration
                                }

                                float scale = 20 * 0.25f;
                                Point center = new Point(0, 0);

                                var viewport = new BoundaryF(
                                    0,
                                    0,
                                    page.Width + 2700,
                                    page.Height + 3700);

                                using (var result = page.Render(new PdfPageRenderConfig()
                                {
                                    Viewport = viewport,
                                    Scale = scale,
                                    BackgroundColor = uint.MaxValue
                                }))
                                {
                                    if (result?.GetImage() != null)
                                    {
                                        int number = pageIndex + 1;
                                        var path =pdfFilePath.Replace(appId.ToString()+"-","");
                                        result.GetImage().SaveAsPng(Path.Combine(outputDirectory, $"{appId}-{Path.GetFileNameWithoutExtension(path)}-{number}.png"));
                                        Console.WriteLine($"{sw.ElapsedMilliseconds:##,###} Milliseconds ",$"{appId}-{Path.GetFileNameWithoutExtension(pdfFilePath)}-{number}.png");
                                        sw.Restart();
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Failed to render image for page {pageIndex + 1}.");
                                    }
                                }
                            }
                        }
                    }

                    sw.Stop();
                    return document.Pages;
                }
                
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error in ConvertPdfToImages: {ex.Message}");
                return 0; // Or handle the error appropriately
            }
        }
         public async Task DownloadPdfFile(string uri,int appId)
        {
            
            var fileName = Path.Combine("wwwroot","files",$"{appId}-{Path.GetFileName(uri)}");
             using (HttpClient httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetStreamAsync(uri))
                {

                    using (var fileStream = File.Create(fileName))
                    {
                        await response.CopyToAsync(fileStream);
                    }
                }
            }
        }
        
        
    }
}
