using Bookstore.Domain;
using System.IO;
using System.Threading.Tasks;

namespace Bookstore.Data.ImageValidationServices
{
    public interface IImageValidationService
    {
        Task<bool> IsSafeAsync(Stream image);
    }

    public class LocalImageValidationService : IImageValidationService
    {
        public async Task<bool> IsSafeAsync(Stream image)
        {
            return await Task.Run(() => true);
        }
    }
}