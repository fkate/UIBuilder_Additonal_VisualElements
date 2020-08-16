using UnityEngine;
using UnityEngine.UIElements;

class LinearGradientBox : VisualElement {

    // Needs factory to be usable in builder
    public new class UxmlFactory : UxmlFactory<LinearGradientBox, UxmlTraits> { }

    // CSS input values
    private static readonly CustomStyleProperty<Color> __linearGradient_top_color = new CustomStyleProperty<Color>("--linearGradient-top-color");
    private static readonly CustomStyleProperty<Color> __linearGradient_bottom_color = new CustomStyleProperty<Color>("--linearGradient-bottom-color");
    private static readonly CustomStyleProperty<int> __linearGradient_rotation = new CustomStyleProperty<int>("--linearGradient-rotation");

    // Parsed values from css
    private Color _colorTop = Color.clear;
    private Color _colorBottom = Color.black;
    private float _rotation = 0;


    /// <summary> Constructor to register method callbacks </summary>
    public LinearGradientBox() {
        RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        generateVisualContent += OnGenerateVisualContent;
    }
    

    /// <summary> Copy values from spritesheet to class </summary>
    private void OnCustomStyleResolved(CustomStyleResolvedEvent e) {
        ICustomStyle customStyle = e.customStyle;
        if (customStyle.TryGetValue(__linearGradient_top_color, out Color colorTop)) _colorTop = colorTop;
        if (customStyle.TryGetValue(__linearGradient_bottom_color, out Color colorBottom)) _colorBottom = colorBottom;
        if (customStyle.TryGetValue(__linearGradient_rotation, out int rotation)) _rotation = Mathf.Clamp01(rotation / 90.0f);
    }


    /// <summary> Generate the visual mesh (since we want a gradient we overwrite the tint with the stylesheet values) </summary>
    private void OnGenerateVisualContent(MeshGenerationContext context) {
        Rect rect = new Rect(
            contentRect.x - resolvedStyle.paddingLeft,
            contentRect.y - resolvedStyle.paddingTop,
            contentRect.width + resolvedStyle.paddingLeft + resolvedStyle.paddingRight,
            contentRect.height + resolvedStyle.paddingBottom + resolvedStyle.paddingTop
        );

        BorderMesh border = new BorderMesh(rect, resolvedStyle);

        if (border.GenerateFilled()) {            
            UVToGradient(ref border.Vertices);            
            
            MeshWriteData mesh = context.Allocate(border.Vertices.Length, border.Indices.Length);

            mesh.SetAllVertices(border.Vertices);
            mesh.SetAllIndices(border.Indices);
        }
    }


    /// <summary> Create a simple gradient from input </summary>
    private void UVToGradient(ref Vertex[] vertices) {
        float x, y, weight;

        for(int i = 0; i < vertices.Length;i++) {
            x = vertices[i].uv.x;
            y = vertices[i].uv.y;

            weight = Mathf.Lerp(y, x, _rotation);
            
            vertices[i].tint = Color.Lerp(_colorBottom, _colorTop, weight);
        }
    }

}
