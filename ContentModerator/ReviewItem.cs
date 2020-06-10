namespace ContentModerator
{
    public class ReviewItem
    {
        // The media type for the item to review. 
        public string Type;
        // The URL of the item to review.
        public string Url;
        // The internal content ID for the item to review.
        public string ContentId;
        // The ID that the service assigned to the review.
        public string ReviewId;
    }
}