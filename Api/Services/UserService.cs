using Api.Configs;
using Api.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Services
{
    public class UserService : IDisposable //необходимо уточнить
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;
        private readonly AuthConfig _config;

        public UserService(IMapper mapper, IOptions<AuthConfig> config, DataContext context)
        {
            _mapper = mapper;
            _context = context;
            _config = config.Value;

            Console.WriteLine("us " + Guid.NewGuid());
        }

        public async Task<bool> CheckUserExist(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

        public async Task AddCommentToPost(Guid userId, CommentModel model)
        {
            var user = await _context.Users.Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == userId);
            var post = await _context.Posts.Include(x => x.PostComments).FirstOrDefaultAsync(x => x.Id == model.PostId);
            if (user != null && post != null)
            {
                var comment = new Comment { Author = user, CommentText = model.CommentText, Created = model.Created, PostId = model.PostId };
                post.PostComments.Add(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<GetCommentsRequestModel>> GetCommentsFromPost(Guid postId)
        {
            var post = await _context.Posts.Include(x => x.PostComments).Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == postId);
            List<GetCommentsRequestModel> comments = new List<GetCommentsRequestModel>();
            if (post != null)
            {
                foreach (var comment in post.PostComments)
                {
                    comments.Add(_mapper.Map<GetCommentsRequestModel>(comment));
                }
            }
            return comments;
        }

        public async Task AddPost(Guid userId, PostModel model, string[] filePaths)
        {
            var user = await _context.Users.Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                List<Attach> attaches = new List<Attach>();
                int i = 0;
                foreach (var item in model.PostAttaches) 
                {
                    attaches.Add(new Attach { Author = user, FilePath = filePaths[i], MimeType = item.MimeType, Name = item.Name, Size = item.Size });
                    i++;
                }
                var post = new Post { Author = user, Description = model.Description, PostAttaches = attaches, AttachPaths = filePaths, Created = model.Created };
                user.Posts.Add(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddPost(Guid userId, PostModel model)
        {
            var user = await _context.Users.Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                var post = new Post { Author = user, Description = model.Description};
                user.Posts.Add(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task <List<GetPostRequestModel>> GetPosts(Guid userId)
        {
            var user = await _context.Users.Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == userId);
            List<GetPostRequestModel> posts = new List<GetPostRequestModel>();
            if (user!= null && user.Posts != null)
            {
                foreach (var post in user.Posts)
                {
                    posts.Add(_mapper.Map<GetPostRequestModel>(post));
                }
            }
            return posts;
        }

        public async Task<GetPostRequestModel> GetPostById(Guid userId, Guid postId)
        {
            var user = await _context.Users.Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == userId);
            var temp = await _context.Posts.FirstOrDefaultAsync((x => x.Id == postId));
            var post =_mapper.Map<GetPostRequestModel>(temp);
            return post;
        }

        public async Task AddAvatarToUser(Guid userId, MetadataModel meta, string filePath)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                var avatar = new Avatar { Author = user, MimeType = meta.MimeType, FilePath = filePath, Name = meta.Name, Size = meta.Size };
                user.Avatar = avatar;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<AttachModel> GetUserAvatar(Guid userId)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId);
            var attach = _mapper.Map<AttachModel>(user.Avatar);
            return attach;
        }

        public async Task Delete(Guid id)
        {
            var dbUser = await GetUserById(id);
            if (dbUser != null)
            {
                _context.Users.Remove(dbUser);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Guid> CreateUser(CreateUserModel model)
        {
            var dbUser = _mapper.Map<DAL.Entities.User>(model);
            var temp = await _context.Users.AddAsync(dbUser);
            await _context.SaveChangesAsync();
            return temp.Entity.Id;
        }
        public async Task<List<UserModel>> GetUsers()
        {
            return await _context.Users.AsNoTracking().ProjectTo<UserModel>(_mapper.ConfigurationProvider).ToListAsync();
        }

        private async Task<DAL.Entities.User> GetUserById(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                throw new Exception("user not found");
            }
            return user;
        }

        public async Task<UserModel> GetUser(Guid id)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == id);
            return _mapper.Map<UserModel>(user);
        }

        private TokenModel GenerateTokens(DAL.Entities.UserSession session)
        {
            var dtNow = DateTime.Now;
            if (session.User == null)
            {
                throw new Exception("well...");
            }
            var jwt = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                notBefore: dtNow,
                claims: new Claim[] //в токене нельзя указывать конфиденциальную информацию и тп
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, session.User.Name),
                    new Claim("sessionId", session.Id.ToString()),
                    new Claim("id", session.User.Id.ToString()),
                },
                expires: DateTime.Now.AddMinutes(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var refresh = new JwtSecurityToken(
                notBefore: dtNow,
                claims: new Claim[]
                {
                    new Claim("refreshToken", session.RefreshToken.ToString()),
                },
                expires: DateTime.Now.AddMinutes(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            );
            var encodedRefresh = new JwtSecurityTokenHandler().WriteToken(refresh);
            return new TokenModel(encodedJwt, encodedRefresh);
        }

        private async Task<DAL.Entities.User> GetUserByCredention(string login, string pass)
        {
            //ищем пользователя в таблице Users
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == login.ToLower());
            if (user == null)
            {
                throw new Exception("user not found");
            }
            if (!HashHelper.Verify(pass, user.PasswordHash))
            {
                throw new Exception("incorrect password");
            }
            return user;
        }

        //метод возвращает токен по логину и паролю
        public async Task<TokenModel> GetToken(string login, string password)
        {
            var user = await GetUserByCredention(login, password);
            var session = await _context.UserSessions.AddAsync(new DAL.Entities.UserSession
            {
                User = user,
                RefreshToken = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Id = Guid.NewGuid()
            });
            await _context.SaveChangesAsync();
            return GenerateTokens(session.Entity);
        }

        public async Task<UserSession> GetSessionById(Guid id)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(x => x.Id == id);
            if (session == null)
            {
                throw new Exception("session is not found");
            }
            return session;
        }

        private async Task<UserSession> GetSessionByRefreshToken(Guid id)
        {
            var session = await _context.UserSessions.Include(x => x.User).FirstOrDefaultAsync(x => x.RefreshToken == id);
            if (session == null)
            {
                throw new Exception("session is not found");
            }
            return session;
        }

        public async Task<TokenModel> GetTokenByRefreshToken(string refreshToken)
        {
            var validParams = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = _config.SymmetricSecurityKey()
            };
            //асинхронные методы не имеют возвращаемых out значений
            var principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, validParams, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken 
                || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, 
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("invalid token");
            }

            if (principal.Claims.FirstOrDefault(x => x.Type == "refreshToken")?.Value is String refreshIdString 
                && Guid.TryParse(refreshIdString, out var refreshId))
            {
                var session = await GetSessionByRefreshToken(refreshId);
                if (!session.IsActive)
                {
                    throw new Exception("session is not active");
                }
                session.RefreshToken = Guid.NewGuid();
                await _context.SaveChangesAsync();
                return GenerateTokens(session);
            }
            else
            {
                throw new SecurityTokenException("invalid token");
            }
        }

        public void Dispose() //тоже под вопросом
        {
            _context.Dispose();
        }
    }
}