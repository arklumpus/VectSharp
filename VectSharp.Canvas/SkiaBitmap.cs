/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2023 Giorgio Bianchini, University of Bristol

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace VectSharp.Canvas
{
    internal class SkiaBitmap : IImage, IDisposable
    {
        public SKCanvas SKCanvas { get; }
        public SKBitmap Bitmap { get; }
        public RenderTargetBitmap AvaloniaBitmap { get; set; }
        public Avalonia.Size Size { get; }
        public int Width { get; }
        public int Height { get; }
        public object Lock { get; }


        static Func<RenderTargetBitmap, SKBitmap> GetBitmap;
        static Func<RenderTargetBitmap, object> GetLock;

        static SkiaBitmap()
        {
            ParameterExpression bitmapParameter = Expression.Parameter(typeof(RenderTargetBitmap));

            PropertyInfo platformImplProperty = typeof(RenderTargetBitmap).GetProperty("PlatformImpl", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            MemberExpression getPlatformImpl = Expression.Property(bitmapParameter, platformImplProperty);
            MemberExpression getItem = Expression.Property(getPlatformImpl, "Item");

            Type renderTargetImpl = typeof(Avalonia.Skia.SkiaPlatform).Assembly.GetType("Avalonia.Skia.WriteableBitmapImpl");

            MemberExpression getBitmap = Expression.Field(Expression.Convert(getItem, renderTargetImpl), "_bitmap");
            MemberExpression getLock = Expression.Field(Expression.Convert(getItem, renderTargetImpl), "_lock");

            GetBitmap = Expression.Lambda<Func<RenderTargetBitmap, SKBitmap>>(getBitmap, bitmapParameter).Compile();
            GetLock = Expression.Lambda<Func<RenderTargetBitmap, object>>(getLock, bitmapParameter).Compile();
        }

        public SkiaBitmap(int width, int height)
        {
            this.AvaloniaBitmap = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
            this.Lock = GetLock(this.AvaloniaBitmap);
            this.Bitmap = GetBitmap(this.AvaloniaBitmap);

            this.SKCanvas = new SKCanvas(this.Bitmap);
            this.Size = new Avalonia.Size(width, height);
            this.Width = width;
            this.Height = height;
        }

        private bool disposedValue;

        public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
        {
            context.DrawImage(this.AvaloniaBitmap, sourceRect, destRect);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.SKCanvas?.Dispose();
                    this.AvaloniaBitmap?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Indispose()
        {
            if (!disposedValue)
            {
                this.SKCanvas?.Dispose();
                this.disposedValue = true;
            }
        }
    }
}
