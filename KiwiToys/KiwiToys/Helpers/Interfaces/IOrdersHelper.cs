using KiwiToys.Common;
using KiwiToys.Models;

namespace KiwiToys.Helpers {
    public interface IOrdersHelper {
        Task<Response> ProcessOrderAsync(ShowCartViewModel model);
        Task<Response> CancelOrderAsync(int id);
    }
}