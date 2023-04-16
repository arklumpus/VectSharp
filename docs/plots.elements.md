---
layout: default
nav_order: 13
parent: Plots
has_children: true
---

# Plot elements

Plot elements can be combined to create custom plots. Each plot element represents an individual element that is added to the plot, such as an axis, a set of labels, a trendline, and son on. Plot elements implement the `IPlotElement` interface and can be added to the plot using the `AddPlotElement` and `AddPlotElements` methods of the `Plot` class.

If you have a `Plot` object, you can enumerate through its plot elements using the `ImmutableList<IPlotElement> PlotElements` property, or you can use the `GetFirst<T>` and `GetAll<T>` methods, which return the first or all the plot elements of the specified type `T` (e.g., `plot.GetFirst<ContinuousAxis>()` returns the first plot element of type `ContinuousAxis` in the plot). If `T` is a coordinate system time (e.g., `LinearCoordinateSystem` or `LogarithmicCoordinateSystem`), a coordinate system is returned instead.

Note that plot elements are added to the plot in the order they are drawn, thus the first element that is drawn will appear behind all the following ones.

The rest of this section describes individual plot elements. If you wish to have more detailed information about the members of each class, please have a look at the XML documentation (which should be shown while writing code e.g. by Visual Studio) or at the [API documentation](api).
