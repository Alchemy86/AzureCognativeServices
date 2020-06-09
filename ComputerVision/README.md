# Computer Vision

Analyze an image for tags, text description, faces, adult content, and more.
Recognize printed and handwritten text with the Batch Read API.

# Tag visual features

Computer Vision can analyze an image and generate a human-readable sentence that describes its contents. The algorithm actually returns several descriptions based on different visual features, and each description is given a confidence score. The final output is a list of descriptions ordered from highest to lowest confidence.

```json
{
    "description": {
        "tags": ["outdoor", "building", "photo", "city", "white", "black", "large", "sitting", "old", "water", "skyscraper", "many", "boat", "river", "group", "street", "people", "field", "tall", "bird", "standing"],
        "captions": [
            {
                "text": "a black and white photo of a city",
                "confidence": 0.95301952483304808
            },
            {
                "text": "a black and white photo of a large city",
                "confidence": 0.94085190563213816
            },
            {
                "text": "a large white building in a city",
                "confidence": 0.93108362931954824
            }
        ]
    },
    "requestId": "b20bfc83-fb25-4b8d-a3f8-b2a1f084b159",
    "metadata": {
        "height": 300,
        "width": 239,
        "format": "Jpeg"
    }
}
```

[In addition we can also return categories. 86 in total](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/concept-categorizing-images)

```json
{
    "categories": [
        {
            "name": "people_",
            "score": 0.81640625
        }
    ],
    "requestId": "bae7f76a-1cc7-4479-8d29-48a694974705",
    "metadata": {
        "height": 200,
        "width": 300,
        "format": "Jpeg"
    }
}
```

# Tags
[Tags](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/concept-tagging-images) based on thousands of recognizable objects, living beings, scenery, and actions. When tags are ambiguous or not common knowledge, the API response provides 'hints' to clarify the meaning of the tag in context of a known setting

```json
{
    "tags": [
        {
            "name": "grass",
            "confidence": 0.9999995231628418
        },
        {
            "name": "outdoor",
            "confidence": 0.99992108345031738
        },
        {
            "name": "house",
            "confidence": 0.99685388803482056
        },
        {
            "name": "sky",
            "confidence": 0.99532157182693481
        },
        {
            "name": "building",
            "confidence": 0.99436837434768677
        },
        {
            "name": "tree",
            "confidence": 0.98880356550216675
        },
        {
            "name": "lawn",
            "confidence": 0.788884699344635
        },
        {
            "name": "green",
            "confidence": 0.71250593662261963
        },
        {
            "name": "residential",
            "confidence": 0.70859086513519287
        },
        {
            "name": "grassy",
            "confidence": 0.46624681353569031
        }
    ],
    "requestId": "06f39352-e445-42dc-96fb-0a1288ad9cf1",
    "metadata": {
        "height": 200,
        "width": 300,
        "format": "Jpeg"
    }
}
```

# Detect Objects

Object detection is similar to tagging, but the API returns the bounding box coordinates (in pixels) for each object found. For example, if an image contains a dog, cat and person, the Detect operation will list those objects together with their coordinates in the image.
It also lets you determine whether there are multiple instances of the same tag in an image!

## Limitations
 * Objects are generally not detected if they're small (less than 5% of the image).
 * Objects are generally not detected if they're arranged closely together (a stack of plates, for example).
 * Objects are not differentiated by brand or product names (different types of sodas on a store shelf, for example). However, you can get brand information from an image by using the Brand detection feature.

# Brand Detection
Brand detection is a specialized mode of object detection that uses a database of thousands of global logos to identify commercial brands in images or video
> If you find that the brand you're looking for is not detected by the Computer Vision service, you may be better served creating and training your own logo detector using the Custom Vision service.

```json
"brands":[  
   {  
      "name":"Microsoft",
      "rectangle":{  
         "x":20,
         "y":97,
         "w":62,
         "h":52
      }
   }
]
```

# Adult Content
Computer Vision can detect adult material in images so that developers can restrict the display of these images in their software. Content flags are applied with a score between zero and one so that developers can interpret the results according to their own preferences.

Much of this functionality is offered by the **Azure Content Moderator service**. See this alternative for solutions to more rigorous content moderation scenarios, such as text moderation and human review workflows.

## [Azure Content Moderator](https://docs.microsoft.com/en-us/azure/cognitive-services/content-moderator/overview)
Azure Content Moderator is a cognitive service that checks text, image, and video content for material that is potentially offensive, risky, or otherwise undesirable. When this material is found, the service applies appropriate labels (flags) to the content. Your app can then handle flagged content in order to comply with regulations or maintain the intended environment for users. See the Moderation APIs section to learn more about what the different content flags indicate.

### Where it's used
The following are a few scenarios in which a software developer or team would use Content Moderator:

> * Online marketplaces that moderate product catalogs and other user-generated content.
> * Gaming companies that moderate user-generated game artifacts and chat rooms.
> * Social messaging platforms that moderate images, text, and videos added by their users.
> * Enterprise media companies that implement centralized moderation for their content.
> * K-12 education solution providers filtering out content that is inappropriate for students and educators.

# Colour Scheme
Computer Vision analyzes the colors in an image to provide three different attributes: the dominant foreground color, the dominant background color, and the set of dominant colors for the image as a whole. Returned colors belong to the set: black, blue, brown, gray, green, orange, pink, purple, red, teal, white, and yellow.

Computer Vision also extracts an accent color, which represents the most vibrant color in the image, based on a combination of dominant colors and saturation. The accent color is returned as a hexadecimal HTML color code.

Computer Vision also returns a boolean value indicating whether an image is in black and white.

```json
{
    "color": {
        "dominantColorForeground": "Black",
        "dominantColorBackground": "Black",
        "dominantColors": ["Black", "White"],
        "accentColor": "BB6D10",
        "isBwImg": false
    },
    "requestId": "0dc394bf-db50-4871-bdcc-13707d9405ea",
    "metadata": {
        "height": 202,
        "width": 300,
        "format": "Jpeg"
    }
}
```


# Domain Specific Content
In addition to tagging and high-level categorization, Computer Vision also supports further domain-specific analysis using models that have been trained on specialized data.

There are two ways to use the domain-specific models: by themselves (scoped analysis) or as an enhancement to the categorization feature.

You can also use domain-specific models to supplement general image analysis. You do this as part of high-level categorization by specifying domain-specific models in the details parameter of the Analyze API call.

**Celebrity recognition**, supported for images classified in the people_ category

**Landmark recognition**, supported for images classified in the outdoor_ or building_ categories

# Read printed and handwritten text
Computer Vision can read visible text in an image and convert it to a character stream.
Line by line this can be extracted.