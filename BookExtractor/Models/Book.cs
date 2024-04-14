namespace BookExtractor.Models
{
    public class Book
    {
        public int id { get; set; }
        public string display_name { get; set; }
        public string parent_name { get; set; }
        public Meta meta { get; set; }
    }
}
