using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TMusicStreaming.Data;
using TMusicStreaming.DTOs.Genre;
using TMusicStreaming.DTOs.Paginate;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        private readonly TMusicStreamingContext _context;
        private readonly ILogger<GenreRepository> _logger;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;

        public GenreRepository(ILogger<GenreRepository> logger, TMusicStreamingContext context, IMapper mapper, ICloudinaryService cloudinaryService)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<List<GenreDTO>> GetAllGenresAsync()
        {
            try
            {
                var genres = await _context.Genres!
                    .OrderByDescending(g => g.CreatedAt)
                    .Select(g => new GenreDTO
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Image = g.Image,
                        CreatedAt = g.CreatedAt,
                        SongCount = g.SongGenres.Count
                    })
                    .ToListAsync();

                return genres;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã có lỗi xảy ra khi lấy danh sách thể loại");
                throw new Exception("Lỗi: ", ex);
            }
        }

        public async Task<PagedResponse<GenreAdminDTO>> GetGenresPaginateForAdminAsync(int page, int pageSize)
        {
            try
            {
                var totalItems = await _context.Genres!.CountAsync();

                var genres = await _context.Genres!
                    .OrderByDescending(g => g.CreatedAt) // Sắp xếp theo ngày tạo mới nhất
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(g => new GenreAdminDTO
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Image = g.Image,
                        CreatedAt = g.CreatedAt,
                        SongCount = g.SongGenres.Count
                    })
                    .ToListAsync();

                var lastPage = (int)Math.Ceiling((double)totalItems / pageSize);

                return new PagedResponse<GenreAdminDTO>
                {
                    Data = genres,
                    Pagination = new PaginationInfo
                    {
                        TotalItems = totalItems,
                        CurrentPage = page,
                        PerPage = pageSize,
                        LastPage = lastPage
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã có lỗi xảy ra khi lấy danh sách thể loại với phân trang");
                throw new Exception("Lỗi: ", ex);
            }
        }

        public async Task<GenreDTO> GetGenreAsync(int id)
        {
            try
            {
                var genre = await _context.Genres!
                    .Where(g => g.Id == id)
                    .Select(g => new GenreDTO
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Image = g.Image,
                        CreatedAt = g.CreatedAt,
                        SongCount = g.SongGenres.Count
                    })
                    .FirstOrDefaultAsync();

                if (genre == null)
                    throw new Exception("Không tìm thấy thể loại");

                return genre;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã có lỗi xảy ra khi lấy thông tin thể loại");
                throw;
            }
        }

        public async Task<List<GenreForFilterDTO>> GenreForFilterDTOAsync(string query)
        {
            try
            {
                IQueryable<Genre> genresQuery = _context.Genres!;
                if (!string.IsNullOrEmpty(query))
                {
                    string loweredQuery = query.ToLower();
                    genresQuery = genresQuery
                        .Where(g => g.Name.ToLower().Contains(loweredQuery));
                }
                return await genresQuery
                    .Select(g => new GenreForFilterDTO { Genre = g.Name })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã có lỗi xảy ra khi lấy danh sách thể loại bằng từ khóa tìm kiếm");
                throw;
            }
        }

        public async Task<bool> CreateGenreAsync(CreateGenreDTO createGenreDto)
        {
            try
            {
                // Kiểm tra xem genre đã tồn tại chưa
                var existingGenre = await _context.Genres!
                    .FirstOrDefaultAsync(g => g.Name.ToLower() == createGenreDto.Name.ToLower());

                if (existingGenre != null)
                    throw new Exception("Thể loại đã tồn tại");

                string? imageUrl = null;

                // Upload image nếu có
                if (createGenreDto.Image != null)
                {
                    imageUrl = await _cloudinaryService.UploadFileAsync(createGenreDto.Image, "TMusicStreaming/genres/images");
                    if (string.IsNullOrEmpty(imageUrl))
                        throw new Exception("Không thể upload ảnh thể loại");
                }

                var genre = new Genre
                {
                    Name = createGenreDto.Name,
                    Image = imageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Genres!.Add(genre);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã có lỗi xảy ra khi tạo thể loại mới");
                throw;
            }
        }

        public async Task<bool> UpdateGenreAsync(int id, UpdateGenreDTO updateGenreDto)
        {
            try
            {
                var genre = await _context.Genres!.FindAsync(id);
                if (genre == null)
                    throw new Exception("Không tìm thấy thể loại");

                var existingGenre = await _context.Genres!
                    .FirstOrDefaultAsync(g => g.Name.ToLower() == updateGenreDto.Name.ToLower() && g.Id != id);

                if (existingGenre != null)
                    throw new Exception("Tên thể loại đã tồn tại");

                // Xử lý upload ảnh mới nếu có
                if (updateGenreDto.Image != null)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(genre.Image))
                    {
                        var oldImagePublicId = ExtractPublicIdFromUrl(genre.Image);
                        if (!string.IsNullOrEmpty(oldImagePublicId))
                        {
                            await _cloudinaryService.DeleteFileAsync(oldImagePublicId);
                        }
                    }

                    // Upload ảnh mới
                    var newImageUrl = await _cloudinaryService.UploadFileAsync(updateGenreDto.Image, "TMusicStreaming/genres/images");
                    if (string.IsNullOrEmpty(newImageUrl))
                        throw new Exception("Không thể upload ảnh thể loại");

                    genre.Image = newImageUrl;
                }

                genre.Name = updateGenreDto.Name;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã có lỗi xảy ra khi cập nhật thể loại");
                throw;
            }
        }

        public async Task<bool> DeleteGenreAsync(int id)
        {
            try
            {
                var genre = await _context.Genres!
                    .Include(g => g.SongGenres)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (genre == null)
                    throw new Exception("Không tìm thấy thể loại");

                if (genre.SongGenres.Any())
                    throw new Exception("Không thể xóa thể loại này vì đang có bài hát sử dụng");

                // Xóa ảnh nếu có
                if (!string.IsNullOrEmpty(genre.Image))
                {
                    var imagePublicId = ExtractPublicIdFromUrl(genre.Image);
                    if (!string.IsNullOrEmpty(imagePublicId))
                    {
                        await _cloudinaryService.DeleteFileAsync(imagePublicId);
                    }
                }

                _context.Genres!.Remove(genre);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã có lỗi xảy ra khi xóa thể loại");
                throw;
            }
        }

        private string ExtractPublicIdFromUrl(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var pathWithoutExtension = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
                var pathSegments = uri.AbsolutePath.Split('/');

                var uploadIndex = Array.IndexOf(pathSegments, "upload");
                if (uploadIndex >= 0 && uploadIndex < pathSegments.Length - 2)
                {
                    var folderAndFile = string.Join("/", pathSegments.Skip(uploadIndex + 2));
                    return Path.GetFileNameWithoutExtension(folderAndFile);
                }

                return pathWithoutExtension;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}