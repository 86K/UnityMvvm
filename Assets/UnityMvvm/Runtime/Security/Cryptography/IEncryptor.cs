

namespace Fusion.Mvvm
{
    public interface IEncryptor
    {
        string AlgorithmName { get; }

        byte[] Encrypt(byte[] buffer);

    }
}
