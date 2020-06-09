using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TextAnalytics
{
    public class HostedService : IHostedService
    {
        private readonly ILogger<HostedService> _logger;
        private readonly AzureKeyCredential _credentials;
        private readonly Uri _endpoint;
        private readonly string _phrase;

        public HostedService(ILogger<HostedService> logger, IOptions<AzureSettings> config) {
            this._logger = logger;
            if (string.IsNullOrEmpty(config.Value.Endpoint) || string.IsNullOrEmpty(config.Value.AzureKeyCredential)) {
                _logger.LogWarning("Hey hey hey.. Fill in your credentials first!");
            } else {
                this._credentials = new AzureKeyCredential(config.Value.AzureKeyCredential);
                this._endpoint = new Uri(config.Value.Endpoint);
                this._phrase = config.Value.PhraseToTest;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Made it");

            var client = new TextAnalyticsClient(_endpoint, _credentials);

            SentimentAnalysisExample(client);
            LanguageDetectionExample(client);
            EntityRecognitionExample(client);
            EntityLinkingExample(client);
            KeyPhraseExtractionExample(client);

            await Task.CompletedTask;
        }

        /// Evaluates text and returns sentiment scores and labels for each sentence
        private void SentimentAnalysisExample(TextAnalyticsClient client)
        {
            DocumentSentiment documentSentiment = client.AnalyzeSentiment(_phrase);
            _logger.LogInformation($"Document sentiment: {documentSentiment.Sentiment}\n");

            foreach (var sentence in documentSentiment.Sentences)
            {
                _logger.LogInformation($"Text: \"{sentence.Text}\"");
                _logger.LogInformation($"Sentence sentiment: {sentence.Sentiment}");
                _logger.LogInformation($"Positive score: {sentence.ConfidenceScores.Positive:0.00}");
                _logger.LogInformation($"Negative score: {sentence.ConfidenceScores.Negative:0.00}");
                _logger.LogInformation($"Neutral score: {sentence.ConfidenceScores.Neutral:0.00}\n");
            }
        }

        /// The returned Response<DetectedLanguage> object will contain the detected language along with its name and ISO-6391 code
        private void LanguageDetectionExample(TextAnalyticsClient client)
        {
            DetectedLanguage detectedLanguage = client.DetectLanguage(_phrase);
            _logger.LogInformation("Language:");
            _logger.LogInformation($"{detectedLanguage.Name},ISO-6391: {detectedLanguage.Iso6391Name}\n");
        }

        /// Ability to identify different entities in text and categorize 
        /// them into pre-defined classes or types such as: 
        /// person, location, event, product and organization.
        private void EntityRecognitionExample(TextAnalyticsClient client)
        {
            var response = client.RecognizeEntities(_phrase);
            _logger.LogInformation("Named Entities:");
            foreach (var entity in response.Value)
            {
                _logger.LogInformation($"Text: {entity.Text},\tCategory: {entity.Category},\tSub-Category: {entity.SubCategory}");
                _logger.LogInformation($"Score: {entity.ConfidenceScore:F2}\n");
            }
        }

        ///  Ability to identify and disambiguate the identity of an entity found in text 
        /// (for example, determining whether an occurrence of the word "Mars" refers to 
        /// the planet, or to the Roman god of war)
        private void EntityLinkingExample(TextAnalyticsClient client)
        {
            var response = client.RecognizeLinkedEntities(_phrase);
            _logger.LogInformation("Linked Entities:");
            foreach (var entity in response.Value)
            {
                _logger.LogInformation($"Name: {entity.Name},\tID: {entity.DataSourceEntityId},\tURL: {entity.Url}\tData Source: {entity.DataSource}");
                _logger.LogInformation("Matches:");
                foreach (var match in entity.Matches)
                {
                    _logger.LogInformation($"Text: {match.Text}");
                    _logger.LogInformation($"Score: {match.ConfidenceScore:F2}\n");
                }
            }
        }

        // Topic identiifiers within a given phrase.
        private void KeyPhraseExtractionExample(TextAnalyticsClient client)
        {
            var response = client.ExtractKeyPhrases(_phrase);
            _logger.LogInformation("Key phrases:");

            foreach (string keyphrase in response.Value)
            {
                _logger.LogInformation(keyphrase);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}