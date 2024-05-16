

using System.IO;

namespace Fusion.Mvvm
{
    public interface IStreamEncryptor : IEncryptor
    {
        Stream Encrypt(Stream input);

    }
}