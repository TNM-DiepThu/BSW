using AutoMapper;
using BusinessLogicLayer.Services.Interface;
using BusinessLogicLayer.Viewmodels;
using BusinessLogicLayer.Viewmodels.OrderM;
using BusinessLogicLayer.Viewmodels.ViewKH;
using CloudinaryDotNet;
using DataAccessLayer.Application;
using DataAccessLayer.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services.Implements
{
    public class ViewKHServiece : IViewKHServiece
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDBContext _dbcontex;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ViewKHServiece(ApplicationDBContext applicationDBContext, IMapper mapper, Cloudinary cloudinary)
        {
            _dbcontex = applicationDBContext;
            _mapper = mapper;
            _cloudinary = cloudinary;
        }
        public async Task<List<ProductVKH>> GetAll()
        {

            var productVKHListUp = await _dbcontex.ProductDetails
         .Include(pd => pd.Options)
         .Include(pd => pd.Images)
         .Include(pd => pd.Products)
         .Select(pd => new ProductVKH
         {
             Id = pd.ID,
             Name = pd.Products.Name,
             CostPrie = pd.Options.FirstOrDefault() != null ? pd.Options.FirstOrDefault().CostPrice : 0,
             urlImg = pd.Images.FirstOrDefault() != null ? pd.Images.FirstOrDefault().Path : null
         })
         .OrderBy(p => p.Name)
         .ToListAsync();

            return productVKHListUp;



        }

        public async Task<List<ProductVKH>> GetAllNameUp(int a)
        {
            if (a == 1)
            {
                var productVKHList = await _dbcontex.ProductDetails
             .Include(pd => pd.Options)
             .Include(pd => pd.Images)
             .Include(pd => pd.Products)
             .Select(pd => new ProductVKH
             {
                 Id = pd.ID,
                 Name = pd.Products.Name,
                 CostPrie = pd.Options.FirstOrDefault() != null ? pd.Options.FirstOrDefault().CostPrice : 0,
                 urlImg = pd.Images.FirstOrDefault() != null ? pd.Images.FirstOrDefault().Path : null
             })
             .OrderBy(p => p.Name)
             .ToListAsync();

                return productVKHList;


            }
            else if (a == 2)
            {
                var productVKHList = await _dbcontex.ProductDetails
             .Include(pd => pd.Options)
             .Include(pd => pd.Images)
             .Include(pd => pd.Products)
             .Select(pd => new ProductVKH
             {
                 Id = pd.ID,
                 Name = pd.Products.Name,
                 CostPrie = pd.Options.FirstOrDefault() != null ? pd.Options.FirstOrDefault().CostPrice : 0,
                 urlImg = pd.Images.FirstOrDefault() != null ? pd.Images.FirstOrDefault().Path : null
             })
             .OrderByDescending(p => p.Name)
             .ToListAsync();

                return productVKHList;


            }
            else if (a == 3)
            {
                var productVKHList = await _dbcontex.ProductDetails
             .Include(pd => pd.Options)
             .Include(pd => pd.Images)
             .Include(pd => pd.Products)
             .Select(pd => new ProductVKH
             {
                 Id = pd.ID,
                 Name = pd.Products.Name,
                 CostPrie = pd.Options.FirstOrDefault() != null ? pd.Options.FirstOrDefault().CostPrice : 0,
                 urlImg = pd.Images.FirstOrDefault() != null ? pd.Images.FirstOrDefault().Path : null
             })
             .OrderBy(p => p.CostPrie)
             .ToListAsync();

                return productVKHList;


            }
            else if (a == 4)
            {
                var productVKHList = await _dbcontex.ProductDetails
             .Include(pd => pd.Options)
             .Include(pd => pd.Images)
             .Include(pd => pd.Products)
             .Select(pd => new ProductVKH
             {
                 Id = pd.ID,
                 Name = pd.Products.Name,
                 CostPrie = pd.Options.FirstOrDefault() != null ? pd.Options.FirstOrDefault().CostPrice : 0,
                 urlImg = pd.Images.FirstOrDefault() != null ? pd.Images.FirstOrDefault().Path : null
             })
             .OrderByDescending(p => p.CostPrie)
             .ToListAsync();

                return productVKHList;


            }
            else
            {

                var productVKHList = await _dbcontex.ProductDetails
               .Include(pd => pd.Options)
               .Include(pd => pd.Images)
               .Include(pd => pd.Products)
               .Select(pd => new ProductVKH
               {
                   Id = pd.ID,
                   Name = pd.Products.Name,
                   CostPrie = pd.Options.FirstOrDefault() != null ? pd.Options.FirstOrDefault().CostPrice : 0,
                   urlImg = pd.Images.FirstOrDefault() != null ? pd.Images.FirstOrDefault().Path : null
               })
       .ToListAsync(); return productVKHList;

            }
        }

        public Task<List<ProductVKH>> GetAllPro()
        {
            throw new NotImplementedException();
        }
        public async Task<List<ProductTQVM>> GetAllTQVMs()
        {

            var prolist = await _dbcontex.ProductDetails
         .Include(pd => pd.Options).ThenInclude(o => o.Sizes)
         .Include(pd => pd.Options).ThenInclude(o => o.Colors)
         .Include(pd => pd.Options)
         .Include(pd => pd.Images)
         .Include(pd => pd.Products)
         .Select(pd => new ProductTQVM
         {

             Id = pd.ID,
             Name = pd.Products.Name,
             MaSp = pd.KeyCode,
             CostPrie = pd.Options.FirstOrDefault() != null ? pd.Options.FirstOrDefault().CostPrice : 0,
             urlImg = pd.Images.FirstOrDefault() != null ? pd.Images.FirstOrDefault().Path : null,
             Size = pd.Options.FirstOrDefault().Sizes.Name,
             Corlor = pd.Options.FirstOrDefault().Colors.Name,
         })
         .ToListAsync();

            return prolist;
        }


        public async Task<ProDetailKH> GetProDetail(Guid id)
        {

            var productDetails = await _dbcontex.ProductDetails
        .Where(pd => pd.ID == id)
        .Include(pd => pd.Products)
        .Include(pd => pd.Options)
            .ThenInclude(o => o.Colors)
        .Include(pd => pd.Options)
            .ThenInclude(o => o.Sizes)
        .FirstOrDefaultAsync();

            if (productDetails == null)
            {
                throw new KeyNotFoundException("Không tìm thấy sản phẩm.");
            }

            var option = productDetails.Options.FirstOrDefault();
            var sizes = productDetails.Options.Select(o => o.Sizes.Name).Distinct().ToList();
            var colors = productDetails.Options.Select(o => o.Colors.Name).Distinct().ToList();
            var image = option?.ImageURL ?? "N/A";

            var price = option?.CostPrice ?? 0;
            var description = productDetails.Description;

            return new ProDetailKH
            {
                ID = productDetails.ID,
                Name = productDetails.Products.Name,
                Price = price,
                Description = description,
                Size = sizes,
                Color = colors,
                UrlImg = image
            };
        }

        public async Task<Response> Logout()
        {
            throw  new NotImplementedException();
        }
    }
}
