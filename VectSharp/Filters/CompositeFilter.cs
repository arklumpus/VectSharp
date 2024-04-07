/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2022 Giorgio Bianchini

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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace VectSharp.Filters
{
    /// <summary>
    /// Represents a filter that corresponds to applying multiple <see cref="ILocationInvariantFilter"/>s one after the other.
    /// </summary>
    public class CompositeLocationInvariantFilter : ILocationInvariantFilter
    {
        /// <inheritdoc/>
        public Point TopLeftMargin { get; }

        /// <inheritdoc/>
        public Point BottomRightMargin { get; }
        
        /// <summary>
        /// The filters that are applied by this filter.
        /// </summary>
        public ImmutableList<ILocationInvariantFilter> Filters { get; }
        
        /// <summary>
        /// Creates a new <see cref="CompositeLocationInvariantFilter"/> with the specified filters.
        /// </summary>
        /// <param name="filters">The filters that will be applied by the new filter.</param>
        public CompositeLocationInvariantFilter(IEnumerable<ILocationInvariantFilter> filters)
        {
            IEnumerable<ILocationInvariantFilter> flattenedFilters = FlattenFilters(filters);
            this.Filters = ImmutableList.CreateRange(flattenedFilters);

            bool initialised = false;

            foreach (IFilter filter in flattenedFilters)
            {
                if (!initialised)
                {
                    this.TopLeftMargin = filter.TopLeftMargin;
                    this.BottomRightMargin = filter.BottomRightMargin;
                    initialised = true;
                }
                else
                {
                    this.TopLeftMargin = Point.Max(this.TopLeftMargin, filter.TopLeftMargin);
                    this.BottomRightMargin = Point.Max(this.BottomRightMargin, filter.BottomRightMargin);
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="CompositeLocationInvariantFilter"/> with the specified filters.
        /// </summary>
        /// <param name="filters">The filters that will be applied by the new filter.</param>
        public CompositeLocationInvariantFilter(params ILocationInvariantFilter[] filters) : this((IEnumerable<ILocationInvariantFilter>)filters) {  }

        private IEnumerable<ILocationInvariantFilter> FlattenFilters(IEnumerable<ILocationInvariantFilter> filters)
        {
            foreach (ILocationInvariantFilter filter in filters)
            {
                if (filter is CompositeLocationInvariantFilter composite)
                {
                    foreach (ILocationInvariantFilter filter2 in FlattenFilters(composite.Filters))
                    {
                        yield return filter2;
                    }
                }
                else
                {
                    yield return filter;
                }
            }
        }

        /// <inheritdoc/>
        [Pure]
        public RasterImage Filter(RasterImage image, double scale)
        {
            RasterImage currImage = image;

            foreach (ILocationInvariantFilter filter in this.Filters)
            {
                RasterImage prevImage = currImage;
                currImage = filter.Filter(prevImage, scale);

                if (prevImage != image)
                {
                    prevImage.Dispose();
                }
            }

            return currImage;
        }
    }
}
