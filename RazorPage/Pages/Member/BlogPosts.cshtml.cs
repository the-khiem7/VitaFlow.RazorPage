using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Services;
using Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace RazorPage.Pages.Member
{
    public class BlogPostsModel : PageModel
    {
        private readonly IBlogService _blogService;
        
        public BlogPostsModel(IBlogService blogService)
        {
            _blogService = blogService;
        }
        
        public List<BlogResponseDto> Blogs { get; set; } = new List<BlogResponseDto>();
        
        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalPages { get; set; }
        
        public async Task OnGetAsync()
        {
            try
            {
                // Get all blog posts
                var allBlogs = await _blogService.GetAllBlogsAsync();
                var blogsList = allBlogs.ToList();
                
                // Pagination
                TotalPages = (int)Math.Ceiling(blogsList.Count / (double)PageSize);
                if (CurrentPage < 1) CurrentPage = 1;
                if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;
                
                // Get paginated blog posts
                Blogs = blogsList
                    .OrderByDescending(b => b.PublishDate)
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }
    }
}