using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TextAnalytics
{
    public class HostedService : IHostedService
    {
        private readonly ILogger<HostedService> _logger;
        private readonly string _imageUrl;
        private readonly string _subscriptionKey;
        private readonly string _endpoint;
        private readonly string _phrase;

        public HostedService(ILogger<HostedService> logger, IOptions<AzureSettings> config) {
            this._logger = logger;
            if (string.IsNullOrEmpty(config.Value.Endpoint) || string.IsNullOrEmpty(config.Value.AzureKeyCredential)) {
                _logger.LogWarning("Hey hey hey.. Fill in your credentials first!");
            } else {
                this._subscriptionKey = config.Value.AzureKeyCredential;
                this._endpoint = config.Value.Endpoint;
                this._imageUrl = config.Value.ImageUrl;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Computer Vision");
            ComputerVisionClient client = Authenticate(_endpoint, _subscriptionKey);

            var analysisResults = await AnalyzeImageUrl(client, _imageUrl);
            ImageDescription(analysisResults);
            ImageCategories(analysisResults);
            ImageTags(analysisResults);
            ImageObjects(analysisResults);
            Brands(analysisResults);
            Faces(analysisResults);
            AdultMaterial(analysisResults);
            ColourScheme(analysisResults);
            DomainSpecificContent(analysisResults);
            ImageType(analysisResults);

            // Read Text!
            await BatchReadFileUrl(client);
        }

        public async Task BatchReadFileUrl(ComputerVisionClient client)
        {
            _logger.LogInformation("----------------------------------------------------------");
            _logger.LogInformation("BATCH READ FILE - URL IMAGE");

            // Read text from URL
            BatchReadFileHeaders textHeaders = await client.BatchReadFileAsync(_imageUrl);
            // After the request, get the operation location (operation ID)
            string operationLocation = textHeaders.OperationLocation;

            // Retrieve the URI where the recognized text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            var operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Extract the text
            // Delay is between iterations and tries a maximum of 10 times.
            int i = 0;
            int maxRetries = 10;
            ReadOperationResult results;
            _logger.LogInformation($"Extracting text from URL image {Path.GetFileName(_imageUrl)}...");
            do
            {
                results = await client.GetReadOperationResultAsync(operationId);
                _logger.LogInformation("Server status: {0}, waiting {1} seconds...", results.Status, i);
                await Task.Delay(1000);
                if (i == 9) 
                { 
                    _logger.LogInformation("Server timed out."); 
                }
            }
            while ((results.Status == TextOperationStatusCodes.Running ||
                results.Status == TextOperationStatusCodes.NotStarted) && i++ < maxRetries);

            Action<TextRecognitionResult> writeResult = (arg1) =>
            {
                arg1.Lines
                .ToObservable()
                .Subscribe(x => _logger.LogInformation($"Result: {x.Text}"));
            };

            results.RecognitionResults
            .ToObservable()
                .Subscribe(x => writeResult(x));
        }

        public async Task<ImageAnalysis> AnalyzeImageUrl(ComputerVisionClient client, string imageUrl)
        {
            _logger.LogInformation("----------------------------------------------------------");
            _logger.LogInformation("ANALYZE IMAGE - URL");

            // Creating a list that defines the features to be extracted from the image. 
            var features = new List<VisualFeatureTypes>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

            _logger.LogInformation($"Analyzing the image {Path.GetFileName(imageUrl)}...");
            return await client.AnalyzeImageAsync(imageUrl, features);
        }

        /// The following code gets the list of generated captions for the image
        private void ImageDescription(ImageAnalysis analysis) {
            _logger.LogInformation("Summary: **************************");
            analysis.Description.Captions
                .ToObservable()
                .Subscribe(caption => 
                    _logger.LogInformation($"{caption.Text} with confidence {caption.Confidence}"));
        }

        /// Get the identified categories
        private void ImageCategories(ImageAnalysis analysis) {
            _logger.LogInformation("Categories: **************************");
            analysis.Categories
                .ToObservable()
                .Subscribe(category => 
                    _logger.LogInformation($"{category.Name} with confidence {category.Score}"));
        }

        /// Identify recognisable objects with tags
        private void ImageTags(ImageAnalysis analysis) {
            _logger.LogInformation("Tags: **************************");
            analysis.Tags
                .ToObservable()
                .Subscribe(x => 
                    _logger.LogInformation($"{x.Name} with confidence {x.Confidence}"));
        }

        /// Identify objects - locations
        private void ImageObjects(ImageAnalysis analysis) {
            _logger.LogInformation("Objects: **************************");
            analysis.Objects
                .ToObservable()
                .Subscribe(x => 
                    _logger.LogInformation($"{x.ObjectProperty} with confidence {x.Confidence} at location {x.Rectangle.X} {x.Rectangle.X + x.Rectangle.W}, {x.Rectangle.Y}, {x.Rectangle.Y + x.Rectangle.H}"));
        }

        /// Detect identified Brands
        private void Brands(ImageAnalysis analysis) {
            _logger.LogInformation("Brands: **************************");
            analysis.Brands
                .ToObservable()
                .Subscribe(x => 
                    _logger.LogInformation($"{x.Name} with confidence {x.Confidence} at location {x.Rectangle.X} {x.Rectangle.X + x.Rectangle.W}, {x.Rectangle.Y}, {x.Rectangle.Y + x.Rectangle.H}"));
        }

        /// Detect faces - Includes Co-ordinates
        private void Faces(ImageAnalysis analysis) {
            _logger.LogInformation("Faces: **************************");
            analysis.Faces
                .ToObservable()
                .Subscribe(x => 
                    _logger.LogInformation($"A {x.Gender} of age {x.Age} at location {x.FaceRectangle.Left}, {x.FaceRectangle.Left}, {x.FaceRectangle.Top + x.FaceRectangle.Width}, {x.FaceRectangle.Top + x.FaceRectangle.Height}"));
        }

        /// Identify Adult / Racy or Gory Content
        private void AdultMaterial(ImageAnalysis analysis) {
            _logger.LogInformation("Adult Material: **************************");
            _logger.LogInformation($"Has Adult Content: {analysis.Adult.IsAdultContent} with confidence {analysis.Adult.AdultScore}");
            _logger.LogInformation($"Has Racy Content: {analysis.Adult.IsRacyContent} with confidence {analysis.Adult.RacyScore}");
        }

        /// Colour scheme
        private void ColourScheme(ImageAnalysis analysis) {
            _logger.LogInformation("Colour Scheme: **************************");
            _logger.LogInformation($"Is black and white?: {analysis.Color.IsBWImg}");
            _logger.LogInformation($"Accent color: {analysis.Color.AccentColor}");
            _logger.LogInformation($"Dominant background color: {analysis.Color.DominantColorBackground}");
            _logger.LogInformation($"Dominant foreground color: {analysis.Color.DominantColorForeground}");
            _logger.LogInformation($"Dominant colors: {string.Join(",", analysis.Color.DominantColors)}");
        }

        /// Domain Specific Content - Celebs and Landmarks
        private void DomainSpecificContent(ImageAnalysis analysis) {
            _logger.LogInformation("Domain Specific: Celebs and Landmarks: **************************");
            analysis.Categories
                .ToObservable()
                .Subscribe(x => {
                    if (x.Detail?.Celebrities != null) {
                        x.Detail?.Celebrities
                        .ToObservable()
                        .Subscribe(y => {
                            _logger.LogInformation($"{y.Name} with confidence {y.Confidence } at location {y.FaceRectangle.Left}, " +
                            $"{y.FaceRectangle.Top}, {y.FaceRectangle.Height}, {y.FaceRectangle.Width}");
                        });
                    }

                    if (x.Detail?.Landmarks != null) {
                        x.Detail?.Landmarks
                        .ToObservable()
                        .Subscribe(y => {
                            _logger.LogInformation($"{y.Name} with confidence {y.Confidence }");
                        });
                    }
                });
        }

        /// Image Type — whether it is clip art or a line drawing.
        private void ImageType(ImageAnalysis analysis) {
            _logger.LogInformation("Image Type: **************************");
            _logger.LogInformation($"Clip art type: {analysis.ImageType.ClipArtType}");
            _logger.LogInformation($"Line drawring Type: {analysis.ImageType.LineDrawingType}");
        }

        // Create the authentication client
        private ComputerVisionClient Authenticate(string endpoint, string key)
        {
            return new ComputerVisionClient(new ApiKeyServiceClientCredentials(key)) {
                Endpoint = endpoint 
            };
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}