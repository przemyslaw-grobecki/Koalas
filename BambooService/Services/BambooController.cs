using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BambooService.Models;

namespace BambooService.Services
{
    public class BambooController : IBambooController
    {
        public Task<List<Bamboo>> GetAllBambooAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Bamboo?> GetBambooByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}