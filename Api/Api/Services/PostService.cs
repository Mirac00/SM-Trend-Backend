using System;
using System.Collections.Generic;
using System.Linq;
using Api.Entities;
using Api.Helpers;
using Api.Models.Posts;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IPostService
    {
        void Create(CreatePostRequest model, int userId);
        IEnumerable<PostResponse> GetAllWithUser();
        PostResponse GetById(int id);
        IEnumerable<PostResponse> GetPostsByUser(int userId);  // Nowa metoda
        IEnumerable<PostResponse> GetLikedPostsByUser(int userId); // Nowa metoda
        void Update(int id, UpdatePostRequest model, int userId);
        void Delete(int id, int userId);
        void AddFileToPost(int postId, PostFileRequest model);
        void RemoveFileFromPost(int postId, int fileId);
        PostFile GetFile(int postId, int fileId);
        IEnumerable<PostResponse> GetFilteredPosts(string fileType, string searchTerm);
        void LikePost(PostLikeDislikeRequest model);
        void DislikePost(PostLikeDislikeRequest model);
        IEnumerable<PostResponse> GetTopLikedPosts();
    }

    public class PostService : IPostService
    {
        private readonly DataContext _context;

        public PostService(DataContext context)
        {
            _context = context;
        }

        public void Create(CreatePostRequest model, int userId)
        {
            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                UserId = userId,
                Files = model.Files?.Select(f => new PostFile
                {
                    FileName = f.FileName,
                    FileType = f.FileType,
                    FileContent = Convert.FromBase64String(f.FileContent),
                    FileUrl = GenerateFileUrl(new PostFile { PostId = 0, Id = 0 }) // Placeholder URL
                }).ToList()
            };

            _context.Posts.Add(post);
            _context.SaveChanges();

            foreach (var file in post.Files)
            {
                file.FileUrl = GenerateFileUrl(file); // Aktualizacja URL po zapisaniu
                _context.PostFiles.Update(file);
            }
            _context.SaveChanges();
        }

        public IEnumerable<PostResponse> GetAllWithUser()
        {
            var posts = _context.Posts.Include(p => p.User).Include(p => p.Files).ToList();
            var postResponses = posts.Select(p => new PostResponse
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                UserId = p.UserId,
                User = new UserResponse
                {
                    Id = p.User.Id,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    Username = p.User.Username
                },
                Files = p.Files.Select(f => new PostFileResponse
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileType = f.FileType,
                    FileUrl = GenerateFileUrl(f),
                    PostId = f.PostId
                }).ToList(),
                Likes = _context.PostLikeDislikes.Count(l => l.PostId == p.Id && l.IsLike),
                Dislikes = _context.PostLikeDislikes.Count(l => l.PostId == p.Id && !l.IsLike)
            }).ToList();

            return postResponses;
        }

        public PostResponse GetById(int id)
        {
            var post = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Files)
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
                throw new KeyNotFoundException("Post not found");

            var postResponse = new PostResponse
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                UserId = post.UserId,
                User = new UserResponse
                {
                    Id = post.User.Id,
                    FirstName = post.User.FirstName,
                    LastName = post.User.LastName,
                    Username = post.User.Username  // Poprawione: tutaj używamy `post.User.Username`
                },
                Files = post.Files.Select(f => new PostFileResponse
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileType = f.FileType,
                    FileUrl = GenerateFileUrl(f),
                    PostId = f.PostId
                }).ToList(),
                Likes = _context.PostLikeDislikes.Count(l => l.PostId == post.Id && l.IsLike),
                Dislikes = _context.PostLikeDislikes.Count(l => l.PostId == post.Id && !l.IsLike)
            };

            return postResponse;
        }


        public IEnumerable<PostResponse> GetPostsByUser(int userId)
        {
            var posts = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Files)
                .Where(p => p.UserId == userId)
                .ToList();

            return posts.Select(p => new PostResponse
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                UserId = p.UserId,
                User = new UserResponse
                {
                    Id = p.User.Id,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    Username = p.User.Username
                },
                Files = p.Files.Select(f => new PostFileResponse
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileType = f.FileType,
                    FileUrl = GenerateFileUrl(f),
                    PostId = f.PostId
                }).ToList(),
                Likes = _context.PostLikeDislikes.Count(l => l.PostId == p.Id && l.IsLike),
                Dislikes = _context.PostLikeDislikes.Count(l => l.PostId == p.Id && !l.IsLike)
            }).ToList();
        }

        public IEnumerable<PostResponse> GetLikedPostsByUser(int userId)
        {
            var likedPostIds = _context.PostLikeDislikes
                .Where(ld => ld.UserId == userId && ld.IsLike)
                .Select(ld => ld.PostId)
                .ToList();

            var posts = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Files)
                .Where(p => likedPostIds.Contains(p.Id))
                .ToList();

            return posts.Select(p => new PostResponse
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                UserId = p.UserId,
                User = new UserResponse
                {
                    Id = p.User.Id,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    Username = p.User.Username
                },
                Files = p.Files.Select(f => new PostFileResponse
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileType = f.FileType,
                    FileUrl = GenerateFileUrl(f),
                    PostId = f.PostId
                }).ToList(),
                Likes = _context.PostLikeDislikes.Count(l => l.PostId == p.Id && l.IsLike),
                Dislikes = _context.PostLikeDislikes.Count(l => l.PostId == p.Id && !l.IsLike)
            }).ToList();
        }

        public IEnumerable<PostResponse> GetTopLikedPosts()
        {
            var posts = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Files)
                .OrderByDescending(p => _context.PostLikeDislikes.Count(l => l.PostId == p.Id && l.IsLike))
                .Take(10)
                .ToList();

            var postResponses = posts.Select(p => new PostResponse
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                UserId = p.UserId,
                User = new UserResponse
                {
                    Id = p.User.Id,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    Username = p.User.Username
                },
                Files = p.Files.Select(f => new PostFileResponse
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileType = f.FileType,
                    FileUrl = GenerateFileUrl(f),
                    PostId = f.PostId
                }).ToList(),
                Likes = _context.PostLikeDislikes.Count(l => l.PostId == p.Id && l.IsLike),
                Dislikes = _context.PostLikeDislikes.Count(l => l.PostId == p.Id && !l.IsLike)
            }).ToList();

            return postResponses;
        }

        public void Update(int id, UpdatePostRequest model, int userId)
        {
            var post = GetPost(id);

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to update this post");

            post.Title = model.Title;
            post.Content = model.Content;

            if (model.Files != null)
            {
                post.Files = model.Files.Select(f => new PostFile
                {
                    FileName = f.FileName,
                    FileType = f.FileType,
                    FileContent = Convert.FromBase64String(f.FileContent),
                    PostId = id,
                    FileUrl = GenerateFileUrl(new PostFile { PostId = id, Id = 0 }) // Placeholder URL
                }).ToList();
            }

            _context.Posts.Update(post);
            _context.SaveChanges();

            foreach (var file in post.Files)
            {
                file.FileUrl = GenerateFileUrl(file); // Aktualizacja URL po zapisaniu
                _context.PostFiles.Update(file);
            }
            _context.SaveChanges();
        }

        public void Delete(int id, int userId)
        {
            var post = GetPost(id);

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to delete this post");

            _context.Posts.Remove(post);
            _context.SaveChanges();
        }

        public void AddFileToPost(int postId, PostFileRequest model)
        {
            var post = GetPost(postId);
            var file = new PostFile
            {
                FileName = model.FileName,
                FileType = model.FileType,
                FileContent = Convert.FromBase64String(model.FileContent),
                PostId = postId,
                FileUrl = GenerateFileUrl(new PostFile { PostId = postId, Id = 0 }) // Placeholder URL
            };

            _context.PostFiles.Add(file);
            _context.SaveChanges();

            file.FileUrl = GenerateFileUrl(file); // Aktualizacja URL po zapisaniu
            _context.PostFiles.Update(file);
            _context.SaveChanges();
        }

        public void RemoveFileFromPost(int postId, int fileId)
        {
            var file = _context.PostFiles.FirstOrDefault(f => f.PostId == postId && f.Id == fileId);
            if (file == null) throw new KeyNotFoundException("File not found");

            _context.PostFiles.Remove(file);
            _context.SaveChanges();
        }

        public PostFile GetFile(int postId, int fileId)
        {
            var file = _context.PostFiles.FirstOrDefault(f => f.PostId == postId && f.Id == fileId);
            if (file == null)
                throw new KeyNotFoundException("File not found");

            file.FileUrl = GenerateFileUrl(file); // Ustaw URL pliku
            return file;
        }

        public IEnumerable<PostResponse> GetFilteredPosts(string fileType, string searchTerm)
        {
            var query = _context.Posts.Include(p => p.User).Include(p => p.Files).AsQueryable();

            if (!string.IsNullOrEmpty(fileType))
            {
                query = query.Where(p => p.Files.Any(f => f.FileType.Contains(fileType)));
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm) ||
                                         p.Content.Contains(searchTerm) ||
                                         p.Files.Any(f => f.FileName.Contains(searchTerm)));
            }

            var posts = query.ToList();
            var postResponses = posts.Select(p => new PostResponse
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                UserId = p.UserId,
                User = new UserResponse
                {
                    Id = p.User.Id,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    Username = p.User.Username
                },
                Files = p.Files.Select(f => new PostFileResponse
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileType = f.FileType,
                    FileUrl = GenerateFileUrl(f),
                    PostId = f.PostId
                }).ToList(),
                Likes = _context.PostLikeDislikes.Count(l => l.PostId == p.Id && l.IsLike),
                Dislikes = _context.PostLikeDislikes.Count(l => l.PostId == p.Id && !l.IsLike)
            }).ToList();

            return postResponses;
        }

        public void LikePost(PostLikeDislikeRequest model)
        {
            var existingLike = _context.PostLikeDislikes
                .FirstOrDefault(l => l.PostId == model.PostId && l.UserId == model.UserId);

            if (existingLike != null)
            {
                if (existingLike.IsLike) return;

                existingLike.IsLike = true;
            }
            else
            {
                var like = new PostLikeDislike
                {
                    PostId = model.PostId,
                    UserId = model.UserId,
                    IsLike = true
                };

                _context.PostLikeDislikes.Add(like);
            }

            _context.SaveChanges();

            // Aktualizacja liczników like'ów i dislike'ów
            UpdatePostLikeDislikeCounts(model.PostId);
        }

        public void DislikePost(PostLikeDislikeRequest model)
        {
            var existingLike = _context.PostLikeDislikes
                .FirstOrDefault(l => l.PostId == model.PostId && l.UserId == model.UserId);

            if (existingLike != null)
            {
                if (!existingLike.IsLike) return;

                existingLike.IsLike = false;
            }
            else
            {
                var dislike = new PostLikeDislike
                {
                    PostId = model.PostId,
                    UserId = model.UserId,
                    IsLike = false
                };

                _context.PostLikeDislikes.Add(dislike);
            }

            _context.SaveChanges();

            // Aktualizacja liczników like'ów i dislike'ów
            UpdatePostLikeDislikeCounts(model.PostId);
        }

        private Post GetPost(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null)
                throw new KeyNotFoundException("Post not found");

            return post;
        }

        private void UpdatePostLikeDislikeCounts(int postId)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == postId);
            if (post != null)
            {
                post.Likes = _context.PostLikeDislikes.Count(l => l.PostId == postId && l.IsLike);
                post.Dislikes = _context.PostLikeDislikes.Count(l => l.PostId == postId && !l.IsLike);
                _context.Posts.Update(post);
                _context.SaveChanges();
            }
        }

        private string GenerateFileUrl(PostFile file)
        {
            // Przykład generowania URL - dostosuj go do swoich potrzeb
            return $"https://localhost:44352/Posts/{file.PostId}/files/{file.Id}";
        }
    }
}
