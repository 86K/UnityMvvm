

#if !NETFX_CORE && !UNITY_WSA && !UNITY_WSA_10_0
using System;
using System.Security.Cryptography;

namespace Fusion.Mvvm
{
    public class AesCTRSymmetricAlgorithm : SymmetricAlgorithm
    {
        private readonly RijndaelManaged rijndael;

        public AesCTRSymmetricAlgorithm(byte[] key, byte[] iv)
        {
            int blockSize = 128;
            BlockSizeValue = blockSize;
            ModeValue = CipherMode.ECB;
            PaddingValue = PaddingMode.None;

            KeyValue = key;
            IVValue = iv;

            rijndael = new RijndaelManaged()
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.None,
                KeySize = 128,
                BlockSize = blockSize
            };
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return new AesCTRCryptoTransform(rijndael, rgbKey, rgbIV);
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return new AesCTRCryptoTransform(rijndael, rgbKey, rgbIV);
        }

        public override void GenerateIV()
        {
            rijndael.GenerateIV();
        }

        public override void GenerateKey()
        {
            rijndael.GenerateKey();
        }
    }

    public class AesCTRCryptoTransform : ICryptoTransform
    {
        private readonly byte[] key;
        private readonly byte[] iv;
        private readonly ICryptoTransform transform;
        private readonly int blockSize;

        private long position;
        private uint counter;
        private int index;
        private readonly byte[] masks;
        public AesCTRCryptoTransform(SymmetricAlgorithm algorithm, byte[] key, byte[] iv)
        {
            this.key = key;
            this.iv = iv;
            blockSize = algorithm.BlockSize / 8;
            transform = algorithm.CreateEncryptor(this.key, new byte[blockSize]);

            masks = new byte[blockSize];
            counter = 0;
            index = 0;
            CalculateMask(counter);
        }

        public bool CanTransformMultipleBlocks => true;
        public bool CanReuseTransform => false;
        public int InputBlockSize => blockSize;
        public int OutputBlockSize => blockSize;

        protected uint Counter
        {
            get => counter;
            set
            {
                if (counter == value)
                    return;

                counter = value;
                CalculateMask(counter);
            }
        }

        public long Position
        {
            get => position;
            set
            {
                if (position == value)
                    return;

                position = value;
                Counter = (uint)(position / blockSize);
                index = (int)(position % blockSize);
            }
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            for (var i = 0; i < inputCount; i++)
            {
                byte mask = masks[index];
                outputBuffer[outputOffset + i] = (byte)(inputBuffer[inputOffset + i] ^ mask);

                position++;
                index++;
                if (index == blockSize)
                {
                    Counter++;
                    index = 0;
                }
            }
            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var outputBuffer = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            return outputBuffer;
        }

        private void CalculateMask(uint counter)
        {
            byte[] data = BitConverter.GetBytes(counter);
            Array.Copy(data, 0, iv, 12, 4);
            transform.TransformBlock(iv, 0, iv.Length, masks, 0);
        }

        public void Dispose()
        {
        }
    }
}
#endif