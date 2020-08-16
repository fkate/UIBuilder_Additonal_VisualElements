![Additional elements](https://github.com/fkate/UIBuilder-VisualElements-Additions/blob/master/extraStyles.png)

<h3>Summary</h3>
A few custom visual elements for Unitys UIBuilder focused on css elements that are not (yet) officially supported. <br>
They make use of the inherited "OnGenerateVisualContent()" method of the VisualElement class to draw another mesh layer intop of the container. <br>
The available styling options can only be set via Stylesheet at the current state the UI Builder package is at. <br>
Css syntax might slightly differ from the Web Browser as there are limitations on what kind of values can be defined (no pixel or percentage values and no array inputs). <br>
<br>
Available styling options are:
<br>
<br>
<h3>BoxShadow</h3>
  <h6>Border widths</h6>
    --boxShadow-top (float|int) <br>
    --boxShadow-right (float|int) <br>
    --boxShadow-bottom (float|int) <br>
    --boxShadow-left (float|int) <br>

  <h6>Inside or outside shadow</h6>
    --boxShadow-inset (true|false) <br>

  <h6>Shadow color</h6>
    --boxShadow-color (rgb|rgba) <br>

<br>

<h3>DashedBorderBox</h3>
  <h6>Border widths</h6>
    --dashed-top (float|int) <br>
    --dashed-right (float|int) <br>
    --dashed-bottom (float|int) <br>
    --dashed-left (float|int) <br>

  <h6>Dash distribution</h6>
    --dashed-solid-count (int) <br>
    --dashed-clear-cout (int) <br>

  <h6>Dash color</h6>
    --dashed-color (rgb|rgba) <br>

<br>

<h3>LinearGradientBox</h3>
  <h6>Gradient colors</h6>
    --linearGradient-top-color (rgb|rgba) <br>
    --linearGradient-bottom-color (rgb|rgba) <br>

  <h6>Rotation</h6>
    --linearGradient-rotation: (int|range[0-90]) <br>
