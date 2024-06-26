﻿using GetGo.Domain.Models;
using GetGo.Domain.Payload.Response.Locations;

namespace GetGo_BE.Services.Interfaces
{
    public interface ILocationService
    {
        public Task<List<Location>> GetTourismLocationList();
        public Task<Location> GetTourismLocationById(int id);
        public Task<List<Location>> GetTrendLocations();
        public Task<List<Location>> GetTopYearLocations();
        public Task<List<Comment>> GetComment(int id);
        public Task<List<Location>> SearchLocation(string searchValue);
        public Task UpdateRatings();

        public Task<List<Location>> GetCityLocation(string city);
    }
}
