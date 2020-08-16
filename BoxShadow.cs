using UnityEngine;
using UnityEngine.UIElements;

class BoxShadow : VisualElement {

    // Needs factory to be usable in builder
    public new class UxmlFactory : UxmlFactory<BoxShadow, UxmlTraits> { }

    // CSS input values
    private static readonly CustomStyleProperty<float> __boxShadow_top = new CustomStyleProperty<float>("--boxShadow-top");
    private static readonly CustomStyleProperty<float> __boxShadow_right = new CustomStyleProperty<float>("--boxShadow-right");
    private static readonly CustomStyleProperty<float> __boxShadow_bottom = new CustomStyleProperty<float>("--boxShadow-bottom");
    private static readonly CustomStyleProperty<float> __boxShadow_left = new CustomStyleProperty<float>("--boxShadow-left");
    private static readonly CustomStyleProperty<bool> __boxShadow_inset = new CustomStyleProperty<bool>("--boxShadow-inset");
    private static readonly CustomStyleProperty<Color> __boxShadow_color = new CustomStyleProperty<Color>("--boxShadow-color");

    // Parsed values from css
    private float _top = 5;
    private float _right = 5;
    private float _bottom = 5;
    private float _left = 5;
    private bool _inset = false;
    private Color _color = Color.black;


    /// <summary> Constructor to register method callbacks </summary>
    public BoxShadow() {
        RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        generateVisualContent += OnGenerateVisualContent;
    }
    

    /// <summary> Copy values from spritesheet to class </summary>
    private void OnCustomStyleResolved(CustomStyleResolvedEvent e) {
        ICustomStyle customStyle = e.customStyle;
        if (customStyle.TryGetValue(__boxShadow_top, out float top)) _top = top;
        if (customStyle.TryGetValue(__boxShadow_right, out float right)) _right = right;
        if (customStyle.TryGetValue(__boxShadow_bottom, out float bottom)) _bottom = bottom;
        if (customStyle.TryGetValue(__boxShadow_left, out float left)) _left = left;
        if (customStyle.TryGetValue(__boxShadow_inset, out bool inset)) _inset = inset;
        if (customStyle.TryGetValue(__boxShadow_color, out Color color)) _color = color;
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

        border.OverrideBorderWidth(_top, _right, _bottom, _left);
        border.OverrideBorderStyle(_color, true, _inset);

        if (border.GenerateBorder()) {            
            MeshWriteData mesh = context.Allocate(border.Vertices.Length, border.Indices.Length);

            mesh.SetAllVertices(border.Vertices);
            mesh.SetAllIndices(border.Indices);
        }
    }

}
