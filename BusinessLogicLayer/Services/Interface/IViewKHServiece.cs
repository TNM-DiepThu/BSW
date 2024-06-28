using BusinessLogicLayer.Viewmodels;
using BusinessLogicLayer.Viewmodels.OrderM;
using BusinessLogicLayer.Viewmodels.ViewKH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services.Interface
{
    public interface IViewKHServiece
    {
        public Task<List<ProductVKH>> GetAll();
        public Task<List<ProductVKH>> GetAllPro();
        public Task<ProDetailKH> GetProDetail(Guid id);
        public Task<List<ProductVKH>> GetAllNameUp(int a);
        public Task<Response> Logout();
        public Task<List<ProductTQVM>> GetAllTQVMs();


    }
}
