# Media Services

Azure Media Services lets you encode your media files into formats that play on a wide variety of browsers and devices.

* Package content
* Stream videos on-demand
* Broadcast live
* Analyze your videos 

Media Services v3 lets you extract insights from your video and audio files with Video Indexer.

This consists of Audio analysis, Speech recognition, Face Detection.

* Audio transcription: A transcript of the spoken words with timestamps. Multiple languages are supported.
* Speaker indexing: A mapping of the speakers and the corresponding spoken words.
* Speech sentiment analysis: The output of sentiment analysis performed on the audio transcription.
* Keywords: Keywords that are extracted from the audio transcription.

* Face tracking: The time during which faces are present in the video. Each face has a face ID and a corresponding collection of thumbnails.
* Visual text: The text that's detected via optical character recognition. The text is time stamped and also used to extract keywords (in addition to the audio transcript).
* Keyframes: A collection of keyframes extracted from the video.
* Visual content moderation: The portion of the videos flagged as adult or racy in nature.
* Annotation: A result of annotating the videos based on a pre-defined object model

```json
"transcript": [
{
    "id": 0,
    "text": "Hi I'm Doug from office.",
    "language": "en-US",
    "instances": [
    {
        "start": "00:00:00.5100000",
        "end": "00:00:02.7200000"
    }
    ]
},
{
    "id": 1,
    "text": "I have a guest. It's Michelle.",
    "language": "en-US",
    "instances": [
    {
        "start": "00:00:02.7200000",
        "end": "00:00:03.9600000"
    }
    ]
}
] 
```

[Full Media Upload Example And Others](https://docs.microsoft.com/en-gb/azure/media-services/latest/stream-files-tutorial-with-api)

[Live stream example](https://docs.microsoft.com/en-gb/azure/media-services/latest/stream-live-tutorial-with-api)