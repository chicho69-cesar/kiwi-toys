using KiwiToys.Common;

namespace KiwiToys.Services.Interfaces {
    public interface IApiService {
        Task<Response> GetListAsync<T>(string servicePrefix, string controller);
    }
}