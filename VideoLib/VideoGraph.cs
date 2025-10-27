using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using DirectShowLib;
// ReSharper disable SuspiciousTypeConversion.Global


namespace VideoLib
{
    public class VideoGraph : IDisposable
    {
        public readonly string fileName;
        private readonly DirectShowLib.IFilterGraph2 _graphBuilder;
        private readonly DirectShowLib.IBaseFilter _vmr9;
        //private readonly IVMRAspectRatioControl9 _wmrAspectRatioControl9;
        private readonly DirectShowLib.IMediaPosition mediaPosition;
        private readonly ISampleGrabber _sampleGrabber;
        private readonly DirectShowLib.IBaseFilter _sampleGrabberFilter;
        //private readonly VideoInfoHeader _header;
        public Compositor _compositor;
        private readonly IVMRWindowlessControl9 _windowlessCtrl;
        public readonly double duration;
        public readonly int framesInSecond;
        public VideoGraph(string fileName, IntPtr handle)
        {
            this.fileName = fileName;
            _graphBuilder = (DirectShowLib.IFilterGraph2)new FilterGraph();
            mediaPosition = (DirectShowLib.IMediaPosition)_graphBuilder;
            _sampleGrabber = (ISampleGrabber)new SampleGrabber();
            _sampleGrabberFilter = (DirectShowLib.IBaseFilter)_sampleGrabber;
            var mediaType = new AMMediaType
            {
                majorType = MediaType.Video,
                formatType = FormatType.VideoInfo
            };
            _sampleGrabber.SetMediaType(mediaType);
            DsUtils.FreeAMMediaType(mediaType);

            _vmr9 = (DirectShowLib.IBaseFilter)new VideoMixingRenderer9();

            var filterConfig = (DirectShowLib.IVMRFilterConfig9)_vmr9;

            _compositor = new Compositor();
            var hr = filterConfig.SetNumberOfStreams(1);
            DsError.ThrowExceptionForHR(hr);

            hr = filterConfig.SetImageCompositor(_compositor);
            DsError.ThrowExceptionForHR(hr);

            hr = filterConfig.SetRenderingMode(VMR9Mode.Windowless);
            DsError.ThrowExceptionForHR(hr);

            _windowlessCtrl = (IVMRWindowlessControl9)_vmr9;


            hr = _windowlessCtrl.SetVideoClippingWindow(handle);
            DsError.ThrowExceptionForHR(hr);

            hr = _windowlessCtrl.SetAspectRatioMode(VMR9AspectRatioMode.None);
            DsError.ThrowExceptionForHR(hr);

            ((IVMRMixerControl9)_vmr9).SetMixingPrefs(VMR9MixerPrefs.RenderTargetRGB);

            hr = _graphBuilder.AddFilter(_vmr9, "Video Mixing Renderer 9 ");
            DsError.ThrowExceptionForHR(hr);

            hr = _graphBuilder.AddFilter(_sampleGrabberFilter, "Sample Grabber");
            DsError.ThrowExceptionForHR(hr);

            hr = _graphBuilder.RenderFile(fileName, null);
            DsError.ThrowExceptionForHR(hr);

            _sampleGrabber.SetOneShot(true);

            _sampleGrabber.GetConnectedMediaType(mediaType);
            var videoInfoHeader = (DirectShowLib.VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(DirectShowLib.VideoInfoHeader));
            framesInSecond = (int)Math.Round(10000000d / videoInfoHeader.AvgTimePerFrame);

            _windowlessCtrl.GetNativeVideoSize(out var videoWidth, out var videoHeight, out var proportionalWidth,
                                              out var proportionalHeight);

            mediaPosition.get_Duration(out duration);


            ((DirectShowLib.IMediaControl)_graphBuilder).Run();
            ((DirectShowLib.IMediaControl)_graphBuilder).Pause();

            ShowFrame(0);
        }

        private bool _timeFormatHasSet;
        void SetTimeFormat()
        {
            if (_timeFormatHasSet) return;
            var mediaSeeking = (DirectShowLib.IMediaSeeking)_graphBuilder;
            mediaSeeking.SetTimeFormat(TimeFormat.Frame);
            mediaSeeking.GetTimeFormat(out var format);
            _timeFormatHasSet = format == TimeFormat.Frame;
        }
        public void ShowFrame(double frameTime)
        {
            SetTimeFormat();
            _compositor.isDone = false;
            var frameNumber = (int)Math.Round(frameTime);// * 25
            ((DirectShowLib.IMediaSeeking)_graphBuilder).SetPositions(
                 new DsLong(frameNumber),
                 AMSeekingSeekingFlags.AbsolutePositioning,
                 new DsLong(frameNumber),
                 AMSeekingSeekingFlags.AbsolutePositioning);
            //mediaPosition.put_CurrentPosition(frameTime);
            ((IMediaEvent)_graphBuilder).WaitForCompletion(40, out var doEvent);
            var isDone = _compositor.isDone;
            var s = Stopwatch.StartNew();
            while (!_compositor.isDone && s.ElapsedMilliseconds < 40)
            {
                Thread.Sleep(1);
            }
            if (isDone) Thread.Sleep(4);
        }

        public void Dispose()
        {
            try
            {
                (_graphBuilder as DirectShowLib.IMediaControl)?.Stop();
            }
            catch (Exception)
            {
                //ignored
            }
            Marshal.ReleaseComObject(_vmr9);
            Marshal.ReleaseComObject(_graphBuilder);
            Marshal.ReleaseComObject(_sampleGrabberFilter);
        }

        public void SetSize(int width, int height)
        {
            var hr = _windowlessCtrl.SetVideoPosition(null, new DsRect(0, 0, width, height));
            DsError.ThrowExceptionForHR(hr);
        }
    }
}
