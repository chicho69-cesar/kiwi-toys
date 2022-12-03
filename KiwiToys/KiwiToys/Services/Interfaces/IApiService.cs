using KiwiToys.Common;

namespace KiwiToys.Services {
    public interface IApiService {
        Task<Response> GetListAsync<T>(string servicePrefix, string controller);
    }
}