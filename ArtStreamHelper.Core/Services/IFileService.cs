using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArtStreamHelper.Core.Services;

public interface IFileService
{
    Task<Stream> PickFileAsync(List<string> fileTypes);
}