# Text Analytics

Key Features include:

* Sentiment analysis
    > Evaluates text and returns sentiment scores and labels for each sentence
* Language detection
    > Identify the language used 
* Entity recognition
  > Lets you takes unstructured text and returns a list of disambiguated entities, with links to more information on the web. The API supports both named entity recognition (NER) and entity linking.
* Key phrase extraction
    > The Key Phrase Extraction API evaluates unstructured text, and for each JSON document, returns a list of key phrases.

This capability is useful if you need to quickly identify the main points in a collection of documents. For example, given input text "The food was delicious and there were wonderful staff", the service returns the main talking points: "food" and "wonderful staff".

You can send text to the API synchronously, or asynchronously. The response object will contain the analysis information for each document you send.

# Entity Linking
Entity linking is the ability to identify and disambiguate the identity of an entity found in text (for example, determining whether an occurrence of the word "Mars" refers to the planet, or to the Roman god of war). This process requires the presence of a knowledge base in an appropriate language, to link recognized entities in text. Entity Linking uses Wikipedia as this knowledge base.

# Named Entity Recognition (NER)
Named Entity Recognition (NER) is the ability to identify different entities in text and categorize them into pre-defined classes or types such as: person, location, event, product and organization.
Tpyes supported: 
https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/named-entity-types?tabs=general


# Key Phrase Extraction
The Key Phrase Extraction API evaluates unstructured text, and for each JSON document, returns a list of key phrases.

This capability is useful if you need to quickly identify the main points in a collection of documents. For example, given input text "The food was delicious and there were wonderful staff", the service returns the main talking points: "food" and "wonderful staff".