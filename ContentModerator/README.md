# Content Moderation

Content Moderator is a cognitive service that checks text, image, and video content for material that is potentially offensive, risky, or otherwise undesirable. When such material is found, the service applies appropriate labels (flags) to the content. Your app can then handle flagged content to comply with regulations or maintain the intended environment for users.

You can use the Content Moderator .NET client library to feed content into the [Review tool](https://contentmoderator.cognitive.microsoft.com/) so that human moderators can review it.


[Quick start to create a review site](https://docs.microsoft.com/en-us/azure/cognitive-services/content-moderator/quick-start)

[Online Review Page For Moderators](https://uksouth.contentmoderator.cognitive.microsoft.com/dashboard)

* Online marketplaces that moderate product catalogs and other user-generated content.
* Gaming companies that moderate user-generated game artifacts and chat rooms.
* Social messaging platforms that moderate images, text, and videos added by their users.
* Enterprise media companies that implement centralized moderation for their content.
* K-12 education solution providers filtering out content that is inappropriate for students and educators.

[Content Moderator Overview](https://docs.microsoft.com/en-gb/azure/cognitive-services/Content-Moderator/overview)


## Text Moderation

Indicates that a review is recommended and displays three categories with score values. The categories are pertaining to the text content that may be undesirable.

* Category 1 - content could be sexually explicit or adult related
* Category 2 - language may be considered sexually suggestive or mature in certain situations
* Category 3 - potentially offensive language

## Image Moderation

To moderate images for adult and racy content. Scan images for text content and extract that text, and detect faces. 

```json
"ImageModeration": {
  .............
  "adultClassificationScore": 0.019196987152099609,
  "isImageAdultClassified": false,
  "racyClassificationScore": 0.032390203326940536,
  "isImageRacyClassified": false,
  ............
},
```

* isImageAdultClassified represents the potential presence of images that may be considered sexually explicit or adult in certain situations.
* isImageRacyClassified represents the potential presence of images that may be considered sexually suggestive or mature in certain situations.
* The scores are between 0 and 1. The higher the score, the higher the model is predicting that the category may be applicable. This preview relies on a statistical model rather than manually coded outcomes. We recommend testing with your own content to determine how each category aligns to your requirements.
* The boolean values are either true or false depending on the internal score thresholds. Customers should assess whether to use this value or decide on custom thresholds based on their content policies.

#### Optical Character Recognition (OCR)

The Optical Character Recognition (OCR) operation predicts the presence of text content in an image and extracts it for text moderation, among other uses. You can specify the language. If you do not specify a language, the detection defaults to English.

The response includes the following information:

The original text.
The detected text elements with their confidence scores.

#### Custom Lists of Blocked Content

* There is a maximum limit of 5 image lists with each list to not exceed 10,000 images.

In many online communities, after users upload images or other type of content, offensive items may get shared multiple times over the following days, weeks, and months. The costs of repeatedly scanning and filtering out the same image or even slightly modified versions of the image from multiple places can be expensive and error-prone.

Instead of moderating the same image multiple times, you add the offensive images to your custom list of blocked content. That way, your content moderation system compares incoming images against your custom lists and stops any further processing.

# Video Moderation
> [Requires Azure media Services Account](https://docs.microsoft.com/en-us/azure/cognitive-services/content-moderator/video-moderation-api)

This will enable you to upload/ steam and moderate video. Sepearte dummy app created for this. > Media Services

## Media Services Explorer
The Azure Media Services Explorer is a user-friendly frontend for AMS. Use it to browse your AMS account, upload videos, and scan content with the Content Moderator media processor. Download and install it from [GitHub](https://github.com/Azure/Azure-Media-Services-Explorer/releases), or see the [Azure Media Services Explorer](https://azure.microsoft.com/blog/managing-media-workflows-with-the-new-azure-media-services-explorer-tool/) blog post for more information.
