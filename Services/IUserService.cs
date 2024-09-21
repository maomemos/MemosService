﻿using MemosService.Models;

namespace MemosService.Services
{
    public interface IUserService
    {
        Task<User> GetUserById(int userId);
        Task<User> GetUserByUsername(string username);
        Task<User> RegisterUser(Auth auth);
        Task<string> LoginUser(Auth auth);
        Task<Dictionary<int, int>> GetUserAnalysisData(int userId, int year);
        Task<Dictionary<string, int>> GetUserHeatmapData(int userId, int year);
    }
}