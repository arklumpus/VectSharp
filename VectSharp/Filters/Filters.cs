using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VectSharp.Filters
{
    /// <summary>
    /// Represents a filter. Do not implement this interface directly; instead, implement <see cref="ILocationInvariantFilter"/> or <see cref="IFilterWithLocation"/>.
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Determines how much the area of the filter's subject should be expanded on the top-left to accommodate the results of the filter.
        /// </summary>
        Point TopLeftMargin { get; }

        /// <summary>
        /// Determines how much the area of the filter's subject should be expanded on the bottom-right to accommodate the results of the filter.
        /// </summary>
        Point BottomRightMargin { get; }
    }

    /// <summary>
    /// Represents a filter that can be applied to an image regardless of its location on the graphics surface.
    /// </summary>
    public interface ILocationInvariantFilter : IFilter
    {
        /// <summary>
        /// Applies the filter to a <see cref="RasterImage"/>.
        /// </summary>
        /// <param name="image">The <see cref="RasterImage"/> to which the filter will be applied.</param>
        /// <param name="scale">The scale of the image with respect to the filter.</param>
        /// <returns>A new <see cref="RasterImage"/> containing the filtered image. The source <paramref name="image"/> is left unaltered.</returns>
        RasterImage Filter(RasterImage image, double scale);
    }

    /// <summary>
    /// Represents a filter whose results depend on the position of the subject image on the graphics surface.
    /// </summary>
    public interface IFilterWithLocation : IFilter
    {
        /// <summary>
        /// Applies the filter to a <see cref="RasterImage"/>.
        /// </summary>
        /// <param name="image">The <see cref="RasterImage"/> to which the filter will be applied.</param>
        /// <param name="bounds">The region on the graphics surface where the image will be drawn.</param>
        /// <param name="scale">The scale of the image with respect to the filter.</param>
        /// <returns>A new <see cref="RasterImage"/> containing the filtered image. The source <paramref name="image"/> is left unaltered.</returns>
        RasterImage Filter(RasterImage image, Rectangle bounds, double scale);
    }

    /// <summary>
    /// Represents a filter with a parameter that needs to be rasterised at the same resolution as the subject image prior to applying the filter.
    /// The <see cref="FilterWithRasterisableParameter"/> abstract class provides a default implementation of this interface.
    /// </summary>
    public interface IFilterWithRasterisableParameter
    {
        /// <summary>
        /// Rasterises the filter's parameter at the specified scale, using the specified rasterisation method.
        /// </summary>
        /// <param name="rasterisationMethod">The method used to rasterise the image. The first argument of this method is the <see cref="Graphics"/> to be rasterised,
        /// the second is a <see cref="Rectangle"/> representing the region to rasterise, the third is a <see cref="double"/> representing the scale, and the third is
        /// a boolean value indicating whether the resulting <see cref="RasterImage"/> should be interpolated.</param>
        /// <param name="scale">The scale factor at which the parameter is rasterised.</param>
        void RasteriseParameter(Func<Graphics, Rectangle, double, bool, RasterImage> rasterisationMethod, double scale);
    }

    /// <summary>
    /// Represents a filter with a parameter that needs to be rasterised at the same resolution as the subject image prior to applying the filter.
    /// </summary>
    public abstract class FilterWithRasterisableParameter : IFilterWithRasterisableParameter, IDisposable
    {
        /// <summary>
        /// The parameter that needs to be rasterised.
        /// </summary>
        protected virtual Graphics RasterisableParameter { get; }

        /// <summary>
        /// The result of the last rasterisation of the <see cref="RasterisableParameter"/>.
        /// </summary>
        protected RasterImage cachedRasterisation;
        private bool disposedValue;

        /// <summary>
        /// Get a rasterised version of the <see cref="RasterisableParameter"/> at the specified scale. If this has already been computed, the cached result is returned.
        /// Otherwise, the image is rasterised using the default rasterisation method.
        /// </summary>
        /// <param name="scale">The scale at which the <see cref="RasterisableParameter"/> will be rasterised.</param>
        /// <returns>A rasterised version of the <see cref="RasterisableParameter"/> at the specified scale.</returns>
        protected virtual RasterImage GetCachedRasterisation(double scale)
        {
            if (this.cachedRasterisation == null || CachedResolution != scale)
            {
                this.RasteriseParameter(scale);
            }

            return this.cachedRasterisation;
        }

        /// <summary>
        /// The bounds of the last cached rendering of the <see cref="RasterisableParameter"/> on the graphics surface.
        /// </summary>
        protected virtual Rectangle CachedBounds { get; set; }

        /// <summary>
        /// The resolution using which the cached rendering of the <see cref="RasterisableParameter"/> has been computed.
        /// </summary>
        protected virtual double CachedResolution { get; set; } = double.NaN;

        /// <summary>
        /// Create a new <see cref="FilterWithRasterisableParameter"/> with the specified parameter.
        /// </summary>
        /// <param name="rasterisableParameter">The parameter that needs to be rasterised at the same resolution as the subject image prior to applying the filter.</param>
        protected FilterWithRasterisableParameter(Graphics rasterisableParameter)
        {
            this.RasterisableParameter = rasterisableParameter;
        }

        /// <inheritdoc />
        public virtual void RasteriseParameter(Func<Graphics, Rectangle, double, bool, RasterImage> rasterisationMethod, double scale)
        {
            Rectangle bounds = RasterisableParameter.GetBounds();

            this.cachedRasterisation?.Dispose();

            this.cachedRasterisation = rasterisationMethod(this.RasterisableParameter, bounds, scale, true);
            this.CachedResolution = scale;
            this.CachedBounds = bounds;
        }

        /// <summary>
        /// Rasterises the filter's parameter at the specified scale, using the default rasterisation method.
        /// </summary>
        /// <param name="scale">The scale factor at which the parameter is rasterised.</param>
        /// <exception cref="NotImplementedException">This exception is thrown when there is no default rasterisation method. This occurs because neither the VectSharp.Raster
        /// assembly, nor the VectSharp.Raster.ImageSharp assembly have been loaded, and no custom implementation of <see cref="Graphics.RasterisationMethod"/> has been provided.</exception>
        protected virtual void RasteriseParameter(double scale)
        {
            Rectangle bounds = this.RasterisableParameter.GetBounds();

            this.cachedRasterisation?.Dispose();

            if (this.RasterisableParameter.TryRasterise(bounds, scale, true, out RasterImage raster))
            {
                this.cachedRasterisation = raster;
                this.CachedResolution = scale;
                this.CachedBounds = bounds;
            }
            else
            {
                throw new NotImplementedException(@"The filter could not be rasterised! You can avoid this error by doing one of the following:
 • Add a reference to VectSharp.Raster or VectSharp.Raster.ImageSharp (you may also need to add a using directive somewhere to force the assembly to be loaded).
 • Provide your own implementation of Graphics.RasterisationMethod.");
            }
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.cachedRasterisation?.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


}
