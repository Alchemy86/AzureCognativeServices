using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace TextAnalytics
{
    public class HostedService : IHostedService
    {
        private readonly ILogger<HostedService> _logger;
        private readonly string _imageUrl;
        private readonly string _subscriptionKey;
        private readonly string _endpoint;
        private readonly string _textToModerate;
        private readonly string _reviewTeamName;
        private readonly string _moderatorEndpoint;

        public HostedService(ILogger<HostedService> logger, IOptions<AzureSettings> config) {
            this._logger = logger;
            if (string.IsNullOrEmpty(config.Value.Endpoint) || string.IsNullOrEmpty(config.Value.AzureKeyCredential)) {
                _logger.LogWarning("Hey hey hey.. Fill in your credentials first!");
            } else {
                this._subscriptionKey = config.Value.AzureKeyCredential;
                this._endpoint = config.Value.Endpoint;
                this._imageUrl = config.Value.ImageUrl;
                this._textToModerate = config.Value.TextToModerate;
                this._moderatorEndpoint = config.Value.ModeratorEndpoint;
                this._reviewTeamName = config.Value.ReviewTeamName;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Computer Vision");
            ContentModeratorClient client = Authenticate(_subscriptionKey, _endpoint);
            client.Endpoint = _endpoint;

            ModerateText(client);
            ModerateImage(client);
            await CreateReview(client);

            await Task.CompletedTask;
        }

        public static ContentModeratorClient Authenticate(string key, string endpoint)
        {
            ContentModeratorClient client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(key));
            client.Endpoint = endpoint;

            return client;
        }
        /// Perform moderation on provided text
        public void ModerateText(ContentModeratorClient client)
        {
            _logger.LogInformation("--------------------------------------------------------------");
            _logger.LogInformation("TEXT MODERATION");

            // Remove carriage returns
            var text = _textToModerate.Replace(Environment.NewLine, " ");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            // Screen the input text: check for profanity, classify the text into three categories,
            // do autocorrect text, and check for personally identifying information (PII)
            _logger.LogInformation("Autocorrect typos, check for matching terms, PII, and classify.");

            // Moderate the text
            var screenResult = client.TextModeration.ScreenText("text/plain", stream, "eng", true, true, null, true);
            _logger.LogInformation(JsonConvert.SerializeObject(screenResult, Formatting.Indented));
        }

        /// Automatically moderate an image
        public void ModerateImage(ContentModeratorClient client)
        {
            _logger.LogInformation("--------------------------------------------------------------");
            _logger.LogInformation("Image MODERATION");
            var imageUrl = new BodyModel("URL", _imageUrl.Trim());

            var adultDetection = client.ImageModeration.EvaluateUrlInput("application/json", imageUrl, true);
            var imageText = client.ImageModeration.OCRUrlInput("eng", "application/json", imageUrl, true);
            var faceDetection = client.ImageModeration.FindFacesUrlInput("application/json", imageUrl, true);

            _logger.LogInformation("Adult Content *************************");
            _logger.LogInformation(JsonConvert.SerializeObject(adultDetection, Formatting.Indented));
            _logger.LogInformation("Image Text Content *************************");
            _logger.LogInformation(JsonConvert.SerializeObject(imageText, Formatting.Indented));
            _logger.LogInformation("Face Detection Content *************************");
            _logger.LogInformation(JsonConvert.SerializeObject(faceDetection, Formatting.Indented));
        }

        /// Send an image for manual moderation
        public async Task CreateReview(ContentModeratorClient client) 
        {
            _logger.LogInformation("--------------------------------------------------------------");
            _logger.LogInformation("CREATE HUMAN IMAGE REVIEWS");

            var imagesToreview = new [] {_imageUrl };

            // The minimum amount of time, in milliseconds, to wait between calls to the Image List API.
            const int throttleRate = 2000;
            // The media type for the item to review. Valid values are "image", "text", and "video".
            const string MediaType = "image";

            // The metadata key to initially add to each review item. This is short for 'score'.
            // It will enable the keys to be 'a' (adult) and 'r' (racy) in the response,
            // with a value of true or false if the human reviewer marked them as adult and/or racy.
            const string MetadataKey = "sc";
            // The metadata value to initially add to each review item.
            const string MetadataValue = "true";

            // Create the structure to hold the request body information.
            List<CreateReviewBodyItem> requestInfo = new List<CreateReviewBodyItem>();

            // Create some standard metadata to add to each item.
            List<CreateReviewBodyItemMetadataItem> metadata =
                new List<CreateReviewBodyItemMetadataItem>(new CreateReviewBodyItemMetadataItem[]
                { new CreateReviewBodyItemMetadataItem(MetadataKey, MetadataValue) });

            // Cache the local information with which to create the review.
            var itemInfo = new ReviewItem()
            {
                Type = MediaType,
                ContentId = _imageUrl,
                Url = _imageUrl,
                ReviewId = null
            };

            _logger.LogInformation($" {Path.GetFileName(itemInfo.Url)} with id = {itemInfo.ContentId}.");

            // Add the item informaton to the request information.
            requestInfo.Add(new CreateReviewBodyItem(itemInfo.Type, itemInfo.Url, itemInfo.ContentId, _moderatorEndpoint, metadata));

            var reviewResponse = client.Reviews.CreateReviewsWithHttpMessagesAsync("application/json", _reviewTeamName, requestInfo);
            // Update the local cache to associate the created review IDs with the associated content.
            var reviewIds = reviewResponse.Result.Body;
            for (int i = 0; i < reviewIds.Count; i++) { itemInfo.ReviewId = reviewIds[i]; }

            _logger.LogInformation(JsonConvert.SerializeObject(reviewIds, Formatting.Indented));

            _logger.LogInformation("Getting review details:");
            var reviewDetail = client.Reviews.GetReviewWithHttpMessagesAsync(_reviewTeamName, itemInfo.ReviewId);
            _logger.LogInformation($"Review {itemInfo.ReviewId} for item ID {itemInfo.ContentId} is " +
                $"{reviewDetail.Result.Body.Status}.", true);
            _logger.LogInformation(JsonConvert.SerializeObject(reviewDetail.Result.Body, Formatting.Indented));
            Thread.Sleep(throttleRate);

            _logger.LogInformation("Perform manual reviews on the Content Moderator site.");
            _logger.LogInformation("Then, press any key to continue.");
            Console.ReadKey();

            // Get details from the human review.
            _logger.LogInformation("Getting review details:");
            var reviewDoneDetail = client.Reviews.GetReviewWithHttpMessagesAsync(_reviewTeamName, itemInfo.ReviewId);
            _logger.LogInformation($"Review {itemInfo.ReviewId} for item ID {itemInfo.ContentId} is " + $"{reviewDoneDetail.Result.Body.Status}.", true);
            _logger.LogInformation(JsonConvert.SerializeObject(reviewDoneDetail.Result.Body, Formatting.Indented));

            Thread.Sleep(throttleRate);

            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}