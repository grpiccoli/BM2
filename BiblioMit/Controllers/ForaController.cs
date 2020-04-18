﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BiblioMit.Models;
using BiblioMit.Models.ForumViewModels;
using BiblioMit.Models.PostViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
//using Amazon.S3;
using System.IO;
//using Amazon.S3.Model;
using BiblioMit.Services;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class ForaController : Controller
    {
        private readonly IForum _forumService;
        private readonly IPost _postService;

        public ForaController(
            IForum forumService,
            IPost postService)
        {
            _forumService = forumService;
            _postService = postService;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var forums = _forumService.GetAll()
                .Select(f => new ForumListingModel {
                    Id = f.Id,
                    Name = f.Title,
                    Description = f.Description,
                    NumberOfPosts = f.Posts?.Count() ?? 0,
                    NumberOfUsers = _forumService.GetActiveUsers(f.Id).Count(),
                    ImageUrl = f.ImageUrl,
                    HasRecentPost = _forumService.HasRecentPost(f.Id)
                });

            var model = new ForumIndexModel
            {
                ForumListing = forums.OrderBy(f => f.Name)
            };

            return View(model);
        }

        public IActionResult Topic(int id, string searchQuery)
        {
            var forum = _forumService.GetbyId(id);

            var posts = _postService.GetFilteredPosts(forum, searchQuery).ToList();

            var postListings = posts.Select(p => new PostListingModel
            {
                Id = p.Id,
                AuthorId = p.User.Id,
                AuthorRating = p.User.Rating,
                AuthorName = p.User.UserName,
                Title = p.Title,
                DatePosted = p.Created.ToString(CultureInfo.InvariantCulture),
                RepliesCount = p.Replies.Count(),
                Forum = BuildForumListing(p)
            });

            var model = new ForumTopicModel
            {
                Post = postListings,
                Forum = BuildForumListing(forum)
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Search(int id, string searchQuery)
        {
            return RedirectToAction("Topic", new { id, searchQuery });
        }

        [Authorize(Roles = "Administrador",Policy ="Foros")]
        public IActionResult Create()
        {
            var model = new AddForumModel();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador", Policy = "Foros")]
        public async Task<IActionResult> AddForum(AddForumModel model)
        {
            var imageUri = "/images/ico/bibliomit.svg";

            if(model?.ImageUpload != null)
            {
                imageUri = UploadForumImage(model.ImageUpload);
            }

            var forum = new Forum
            {
                Title = model.Title,
                Description = model.Description,
                Created = DateTime.Now,
                ImageUrl = new Uri(imageUri)
            };

            await _forumService.Create(forum).ConfigureAwait(false);

            return RedirectToAction("Index", "Fora");
        }

        private static string UploadForumImage(IFormFile file)
        {
            //var userId = _userManager.GetUserId(User);

            //var filePath = Path.GetTempFileName();

            //var stream = new FileStream(filePath, FileMode.Create);

            //var accessKey = "AKIAISMYGSV5LKLHP25A";
            //var secretKey = "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";

            //var client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.SAEast1);

            //var request = new PutObjectRequest
            //{
            //    BucketName = "bucketmit",
            //    Key = userId,
            //    FilePath = filePath
            //};

            //var url = "https://" + request.BucketName + ".s3.amazonaws.com/" + request.Key;

            //var response = client.PutObjectAsync(request).GetAwaiter().GetResult();

            //_userService.SetProfileImage(userId, new Uri(url));

            //return url;
            return file.ToString();
        }

        private static ForumListingModel BuildForumListing(Post p)
        {
            var forum = p.Forum;

            return BuildForumListing(forum);
        }

        private static ForumListingModel BuildForumListing(Forum forum)
        {
            return new ForumListingModel
            {
                Id = forum.Id,
                Name = forum.Title,
                Description = forum.Description,
                ImageUrl = forum.ImageUrl
            };
        }
    }
}