using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using DirectShowLib;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using FillMode = Microsoft.DirectX.Direct3D.FillMode;
using Matrix = Microsoft.DirectX.Matrix;

namespace VideoLib
{
    public class Compositor : IVMRImageCompositor9, IDisposable
    {
        private IntPtr _unmanagedDevice;
        private Device _device;
        public bool isDone;
        public CarDrawer _carDrawer;


        public CarInFrameInfo carInFrame = new CarInFrameInfo
        {
            //positionY = 3,
            //positionZ = 6,
            //rotationYaw = 0
        };

        public Compositor()
        {
            Device.IsUsingEventHandlers = false;
        }

        private void FreeResources()
        {
            _device?.Dispose();
        }

        private void SetManagedDevice(IntPtr unmagedDevice)
        {
            FreeResources();

            Marshal.AddRef(unmagedDevice);
            _unmanagedDevice = unmagedDevice;
            _device = new Device(unmagedDevice);
        }



        public int InitCompositionDevice(IntPtr pD3DDevice)
        {
            try
            {
                if (_unmanagedDevice != pD3DDevice) SetManagedDevice(pD3DDevice);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            return 0;
        }
        public int TermCompositionDevice(IntPtr pD3DDevice)
        {
            try
            {
                _unmanagedDevice = IntPtr.Zero;
                FreeResources();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            return 0;
        }
        public int SetStreamMediaType(int dwStrmId, AMMediaType pmt, bool fTexture)
        {
            return 0;
        }

        public int CompositeImage(IntPtr pD3DDevice, IntPtr pddsRenderTarget, AMMediaType pmtRenderTarget, long rtStart, long rtEnd,
            int dwClrBkGnd, VMR9VideoStreamInfo[] pVideoStreamInfo, int cStreams)
        {
            try
            {
                if (_unmanagedDevice != pD3DDevice) SetManagedDevice(pD3DDevice);
                if (carInFrame != null)
                {
                    _carDrawer ??= new(_device);
                    _carDrawer.RenderInSurface(carInFrame, _device);
                }


                Marshal.AddRef(pddsRenderTarget);
                var renderTarget = new Surface(pddsRenderTarget);
                var renderTargetDesc = renderTarget.Description;
                var renderTargetRect = new Rectangle(0, 0, renderTargetDesc.Width, renderTargetDesc.Height);

                Marshal.AddRef(pVideoStreamInfo[0].pddsVideoSurface);
                var surface = new Surface(pVideoStreamInfo[0].pddsVideoSurface);
                var surfaceDesc = surface.Description;
                var surfaceRect = new Rectangle(0, 0, surfaceDesc.Width, surfaceDesc.Height);

                _device.SetRenderTarget(0, renderTarget);
                _device.StretchRectangle(surface, surfaceRect, renderTarget, renderTargetRect, TextureFilter.None);
                try
                {
                    _device.BeginScene();
                }
                catch (Exception)
                {
                    SetManagedDevice(pD3DDevice);
                    _device.SetRenderTarget(0, renderTarget);
                    _device.StretchRectangle(surface, surfaceRect, renderTarget, renderTargetRect, TextureFilter.None);
                }

                if (carInFrame != null  && carInFrame.visibility !=0) _carDrawer.Draw(_device, carInFrame);
                //_device.SetS();

                _device.EndScene();
                surface.Dispose();
                renderTarget.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            isDone = true;
            return 0;
        }


        public void Dispose()
        {
            FreeResources();
        }
    }
}
