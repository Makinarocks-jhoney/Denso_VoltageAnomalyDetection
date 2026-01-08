using MvCameraControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MVP_Voltage.Services
{
    internal sealed class RawFrame:IDisposable
    {
        public required byte[] Buffer { get; init; }
        public required int Width { get; init; }
        public required int Height { get; init; }
        public required int Stride { get; init; }
        public required MvGvspPixelType Format { get; init; }
        public long Timestamp { get; init; }

        public required int Length { get; init; }

        private Action<byte[]>? _return;
        public void SetReturn(Action<byte[]> ret) => _return = ret;

        public void Dispose()
        {
            var r = _return;
            _return = null;
            if (r != null) r(Buffer);
        }
    }

    
    internal sealed record FramePacket(
    long Seq,
    long TimestampTicks,  // Stopwatch.GetTimestamp()
    RawFrame Frame        // byte[] + w/h/stride/format
);
    internal sealed class FrameHub
    {

        // UI는 보통 30~60fps면 충분: 작은 버퍼
        private readonly Channel<FramePacket> _uiCh;

        // Processing은 좀 더 살리고 싶으니 버퍼를 더 줌
        private readonly Channel<FramePacket> _procCh;

        private long _seq;

        public ChannelReader<FramePacket> UiReader => _uiCh.Reader;
        public ChannelReader<FramePacket> ProcReader => _procCh.Reader;

        // "허용 지연" (예: 200ms) 넘으면 드롭 시작
        private readonly TimeSpan _maxLatency;

        public FrameHub(int uiCapacity, int procCapacity, TimeSpan maxLatency)
        {
            _maxLatency = maxLatency;

            _uiCh = Channel.CreateBounded<FramePacket>(new BoundedChannelOptions(uiCapacity)
            {
                SingleWriter = true,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.DropOldest // 핵심: 오래된 것부터 버림
            });

            _procCh = Channel.CreateBounded<FramePacket>(new BoundedChannelOptions(procCapacity)
            {
                SingleWriter = true,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.DropOldest // 오래된 것부터 버림(최신만 고정 아님)
            });
        }
        public void Publish(RawFrame frame)
        {
            var pkt = new FramePacket(
                Seq: Interlocked.Increment(ref _seq),
                TimestampTicks: Stopwatch.GetTimestamp(),
                Frame: frame);

            // 채널이 가득 차면 DropOldest에 의해 "필요한 만큼만" 자동 드롭
            _uiCh.Writer.TryWrite(pkt);
            _procCh.Writer.TryWrite(pkt);
        }

        public bool IsTooLate(FramePacket pkt)
        {
            var now = Stopwatch.GetTimestamp();
            var dt = TimeSpan.FromSeconds((now - pkt.TimestampTicks) / (double)Stopwatch.Frequency);
            return dt > _maxLatency;
        }
    }
}
