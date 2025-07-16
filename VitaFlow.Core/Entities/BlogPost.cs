using System;
using System.Collections.Generic;

namespace VitaFlow.Core.Entities
{
    // Represents a blog post.
    public class BlogPost
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public User Author { get; set; } = new User();
        public DateTime PublishedDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
