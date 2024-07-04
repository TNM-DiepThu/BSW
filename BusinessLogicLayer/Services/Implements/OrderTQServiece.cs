using AutoMapper;
using BusinessLogicLayer.Services.Interface;
using BusinessLogicLayer.Viewmodels.OrderM;
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
    public class OrderTQServiece : IOrderTQServiece
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDBContext _dbcontex;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderTQServiece(UserManager<ApplicationUser> userManager, ApplicationDBContext applicationDBContext, IMapper mapper, Cloudinary cloudinary, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _cloudinary = cloudinary;
            _userManager = userManager;
            _dbcontex = applicationDBContext;
                _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<ProductTQVM>> GetProductTQVMs(string seach)
        {
            var prolist = await _dbcontex.ProductDetails
       .Include(pd => pd.Options).ThenInclude(o => o.Sizes)
       .Include(pd => pd.Options).ThenInclude(o => o.Colors)
       .Include(pd => pd.Options)
       .Include(pd => pd.Images)
       .Include(pd => pd.Products)
       .Where(pd => pd.Products.Name.Contains(seach)) 
       .Select(pd => new ProductTQVM
       {
           Id = pd.ID,
           Name = pd.Products.Name,
           MaSp = pd.KeyCode,
           CostPrie = pd.Options.FirstOrDefault() != null ? pd.Options.FirstOrDefault().CostPrice : 0,
           urlImg = pd.Images.FirstOrDefault() != null ? pd.Images.FirstOrDefault().Path : null,
           Size = pd.Options.FirstOrDefault() != null ? pd.Options.FirstOrDefault().Sizes.Name : null,
           Corlor = pd.Options.FirstOrDefault() != null ? pd.Options.FirstOrDefault().Colors.Name : null,
       })
       .ToListAsync();

            return prolist;
        }
    }
}
