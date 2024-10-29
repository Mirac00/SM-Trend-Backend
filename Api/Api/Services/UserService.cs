using AutoMapper;
using BCrypt.Net;
using System.Collections.Generic;
using Api.Authorization;
using Api.Entities;
using Api.Helpers;
using Api.Models.Users;
using System.IdentityModel.Tokens.Jwt;

namespace Api.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Register(RegisterRequest model);
        void Update(int id, UpdateRequest model);
        void Delete(int id);
        User GetUserByToken(string token);
        string RefreshToken(string token); // Dodana metoda
    }

    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IJwtUtils _jwtUtils;
        private readonly IMapper _mapper;

        public UserService(
            DataContext context,
            IJwtUtils jwtUtils,
            IMapper mapper)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _mapper = mapper;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _context.Users.SingleOrDefault(x => x.Username == model.Username);

            // Validate
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                throw new AppException("Username or password is incorrect");

            // Authentication successful
            var response = _mapper.Map<AuthenticateResponse>(user);
            response.Token = _jwtUtils.GenerateToken(user);
            return response;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(int id)
        {
            return GetUser(id);
        }

        public void Register(RegisterRequest model)
        {
            // Validate
            if (_context.Users.Any(x => x.Username == model.Username))
                throw new AppException("Username '" + model.Username + "' is already taken");

            // Map model to new user object
            var user = _mapper.Map<User>(model);

            // Hash password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // Save user
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Update(int id, UpdateRequest model)
        {
            var user = GetUser(id);

            if (model.Username != user.Username && _context.Users.Any(x => x.Username == model.Username))
                throw new AppException("Username '" + model.Username + "' is already taken");

            // Hash password if it was provided
            if (!string.IsNullOrEmpty(model.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            _mapper.Map(model, user);
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = GetUser(id);
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public User GetUserByToken(string token)
        {
            // Usuń prefix "Bearer " jeśli istnieje
            var jwtToken = new JwtSecurityToken(token.Replace("Bearer ", ""));
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
            return GetById(userId);
        }

        public string RefreshToken(string token)
        {
            var userId = _jwtUtils.ValidateToken(token);
            if (userId == null)
                return null;

            var user = GetById(userId.Value);
            if (user == null)
                return null;

            var newToken = _jwtUtils.GenerateToken(user);
            return newToken;
        }

        // Helper methods
        private User GetUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) throw new KeyNotFoundException("User not found");
            return user;
        }
    }
}
