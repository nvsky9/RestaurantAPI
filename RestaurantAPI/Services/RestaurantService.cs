using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Authorization;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using RestaurantAPI.Models;
using System.Linq.Expressions;
using System.Security.Claims;

namespace RestaurantAPI.Services
{
    public interface IRestaurantService
    {
        int Create(CreateRestaurantDto dto);
        PageResult<RestaurantDto> GetAll(RestaurantQuery query);
        RestaurantDto GetById(int id);
        void Delete(int id);
        void Modify(int id, ModifyRestaurantDto dto);
    }

    public class RestaurantService : IRestaurantService
    {
        private readonly RestaurantDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextService _userContextService;

        public RestaurantService(RestaurantDbContext dbContext, IMapper mapper, ILogger<RestaurantService> logger
            ,IAuthorizationService authorizationService, IUserContextService userContextService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _authorizationService = authorizationService;
            _userContextService = userContextService;
        }

        public void Modify(int id, ModifyRestaurantDto dto)
        {
            var restaurant = _dbContext
                .Restaurants
                .FirstOrDefault(r => r.Id == id);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found");

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, restaurant,
                new ResourceOperationRequirement(ResourceOperation.Modify)).Result;
            if(!authorizationResult.Succeeded)
            {
                throw new ForbidException();
            }

            restaurant.Name = dto.Name ?? restaurant.Name;
            restaurant.Description = dto.Description ?? restaurant.Description;
            restaurant.HasDelivery = dto.HasDelivery ?? restaurant.HasDelivery;

            _dbContext.SaveChanges();


        }


        public void Delete(int id)
        {
            _logger.LogWarning($"Restaurant with id: {id} DELETE action invoked");
            var restaurant = _dbContext
                .Restaurants
                .FirstOrDefault(r => r.Id == id);
            if (restaurant == null)
                throw new NotFoundException("Restaurant not found");
            
            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, restaurant,
                new ResourceOperationRequirement(ResourceOperation.Delete)).Result;
            if (!authorizationResult.Succeeded)
            {
                throw new ForbidException();
            }

            _dbContext.Restaurants.Remove(restaurant);
            _dbContext.SaveChanges();


        }


        public RestaurantDto GetById(int id)
        {
            var restaurant = _dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .FirstOrDefault(r => r.Id == id);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found");


            var result = _mapper.Map<RestaurantDto>(restaurant);
            return result;
        }

        public PageResult<RestaurantDto> GetAll(RestaurantQuery query)
        {
            var baseQuery = _dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .Where(r => query.SeatchPhrase == null || (r.Name.ToLower().Contains(query.SeatchPhrase.ToLower())
                || r.Description.ToLower().Contains(query.SeatchPhrase.ToLower())));

            if(!string.IsNullOrEmpty(query.SortBy))
            {
                var columnsSelectors = new Dictionary<string, Expression<Func<Restaurant, object>>>
                {
                    { nameof(Restaurant.Name), r => r.Name },
                    { nameof(Restaurant.Description), r => r.Description },
                    { nameof(Restaurant.Category), r => r.Category },

                };

                var selectedCoulumn = columnsSelectors[query.SortBy];
                baseQuery = query.SortDirection == SortDirection.Ascending
                    ? baseQuery.OrderBy(selectedCoulumn)
                    : baseQuery.OrderByDescending(selectedCoulumn);
            }


            var restaurants = baseQuery
                .Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .ToList();

            var totalItemsCount = baseQuery.Count();

            var restaurantsDtos = _mapper.Map<List<RestaurantDto>>(restaurants);

            var result = new PageResult<RestaurantDto>(restaurantsDtos, totalItemsCount, query.PageSize, query.PageNumber);
            return result;
        }

        public int Create(CreateRestaurantDto dto)
        {
            var restaurant = _mapper.Map<Restaurant>(dto);
            restaurant.CreatedById = _userContextService.GetUserId;
            _dbContext.Restaurants.Add(restaurant);
            _dbContext.SaveChanges();

            return restaurant.Id;
        }
    }
}