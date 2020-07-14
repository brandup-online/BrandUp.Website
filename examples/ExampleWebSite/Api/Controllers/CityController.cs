using ExampleWebSite.Api.Models;
using ExampleWebSite.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExampleWebSite.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        readonly CityRepository cityRepository;

        public CityController(CityRepository cityRepository)
        {
            this.cityRepository = cityRepository ?? throw new ArgumentNullException(nameof(cityRepository));
        }

        public List<CityModel> OnGet()
        {
            return cityRepository.Cities.OrderBy(it => it.Title).Select(it => new CityModel { Name = it.Name, Title = it.Title }).ToList();
        }
    }
}