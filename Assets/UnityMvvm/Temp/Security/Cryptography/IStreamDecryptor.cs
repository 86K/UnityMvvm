

using System.IO;

namespace Fusion.Mvvm
{
    public interface IStreamDecryptor : IDecryptor
    {
        Stream Decrypt(Stream input);

    }
}