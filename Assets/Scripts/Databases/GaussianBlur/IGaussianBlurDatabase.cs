using Models;

namespace Databases.GaussianBlur
{
    public interface IGaussianBlurDatabase
    {
        GaussianBlurVo DefaultGaussianBlurVo { get; }
        GaussianBlurVo GPUGaussianBlurVo { get; }
    }
}