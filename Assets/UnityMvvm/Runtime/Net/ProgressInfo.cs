

using System;

namespace Fusion.Mvvm
{
    public enum UNIT
    {
        BYTE,
        KB,
        MB,
        GB
    }

    public class ProgressInfo
    {
        private long totalSize = 0;
        private long completedSize = 0;

        private int totalCount = 0;
        private int completedCount = 0;

        private float speed = 0f;
        private long lastTime = -1;
        private long lastValue = -1;
        private long lastTime2 = -1;
        private long lastValue2 = -1;

        public ProgressInfo() : this(0, 0)
        {
        }

        public ProgressInfo(long totalSize, long completedSize)
        {
            this.totalSize = totalSize;
            this.completedSize = completedSize;

            lastTime = DateTime.UtcNow.Ticks / 10000;
            lastValue = this.completedSize;

            lastTime2 = lastTime;
            lastValue2 = lastValue;
        }

        public long TotalSize
        {
            get => totalSize;
            set => totalSize = value;
        }
        public long CompletedSize
        {
            get => completedSize;
            set
            {
                completedSize = value;
                OnUpdate();
            }
        }

        public int TotalCount
        {
            get => totalCount;
            set => totalCount = value;
        }
        public int CompletedCount
        {
            get => completedCount;
            set => completedCount = value;
        }

        private void OnUpdate()
        {
            long now = DateTime.UtcNow.Ticks / 10000;

            if ((now - lastTime) >= 1000)
            {
                lastTime2 = lastTime;
                lastValue2 = lastValue;

                lastTime = now;
                lastValue = completedSize;
            }

            float dt = (now - lastTime2) / 1000f;
            speed = (completedSize - lastValue2) / dt;
        }

        public virtual float Value
        {
            get
            {
                if (totalSize <= 0)
                    return 0f;

                return completedSize / (float)totalSize;
            }
        }

        public virtual float GetTotalSize(UNIT unit = UNIT.BYTE)
        {
            switch (unit)
            {
                case UNIT.KB:
                    return totalSize / 1024f;
                case UNIT.MB:
                    return totalSize / 1048576f;
                case UNIT.GB:
                    return totalSize / 1073741824f;
                default:
                    return (float)totalSize;
            }
        }

        public virtual float GetCompletedSize(UNIT unit = UNIT.BYTE)
        {
            switch (unit)
            {
                case UNIT.KB:
                    return completedSize / 1024f;
                case UNIT.MB:
                    return completedSize / 1048576f;
                case UNIT.GB:
                    return completedSize / 1073741824f;
                default:
                    return (float)completedSize;
            }
        }

        public virtual float GetSpeed(UNIT unit = UNIT.BYTE)
        {
            switch (unit)
            {
                case UNIT.KB:
                    return speed / 1024f;
                case UNIT.MB:
                    return speed / 1048576f;
                case UNIT.GB:
                    return speed / 1073741824f;
                default:
                    return speed;
            }
        }
    }
}