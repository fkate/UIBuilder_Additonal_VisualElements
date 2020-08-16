using UnityEngine;
using UnityEngine.UIElements;

class DashedBorderBox : VisualElement {

    // Needs factory to be usable in builder
    public new class UxmlFactory : UxmlFactory<DashedBorderBox, UxmlTraits> { }
 
    // CSS input values
    private static readonly CustomStyleProperty<float> __dashed_top = new CustomStyleProperty<float>("--dashed-top");
    private static readonly CustomStyleProperty<float> __dashed_right = new CustomStyleProperty<float>("--dashed-right");
    private static readonly CustomStyleProperty<float> __dashed_bottom = new CustomStyleProperty<float>("--dashed-bottom");
    private static readonly CustomStyleProperty<float> __dashed_left = new CustomStyleProperty<float>("--dashed-left");
    private static readonly CustomStyleProperty<int> __dashed_solid_count = new CustomStyleProperty<int>("--dashed-solid-count");
    private static readonly CustomStyleProperty<int> __dashed_clear_count = new CustomStyleProperty<int>("--dashed-clear-count");
    private static readonly CustomStyleProperty<Color> __dashed_color = new CustomStyleProperty<Color>("--dashed-color");

    // Parsed values from css
    private float _top = 5;
    private float _right = 5;
    private float _bottom = 5;
    private float _left = 5;
    private int _solid = 4;
    private int _clear = 4;
    private Color _color = Color.black;

    // Cached values
    private Texture2D _texture;
    

    /// <summary> Constructor to register method callbacks </summary></summary>
    public DashedBorderBox() {
        RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        generateVisualContent += OnGenerateVisualContent;
    }
    

    /// <summary> Copy values from spritesheet to class </summary>
    private void OnCustomStyleResolved(CustomStyleResolvedEvent e) {
        ICustomStyle customStyle = e.customStyle;
        if (customStyle.TryGetValue(__dashed_top, out float top)) _top = top;
        if (customStyle.TryGetValue(__dashed_right, out float right)) _right = right;
        if (customStyle.TryGetValue(__dashed_bottom, out float bottom)) _bottom = bottom;
        if (customStyle.TryGetValue(__dashed_left, out float left)) _left = left;
        if (customStyle.TryGetValue(__dashed_solid_count, out int solid)) _solid = solid;
        if (customStyle.TryGetValue(__dashed_solid_count, out int clear)) _clear = clear;
        if (customStyle.TryGetValue(__dashed_color, out Color color)) _color = color;
    }


    /// <summary> Generate the visual mesh (since we want a gradient we overwrite the tint with the stylesheet values) </summary>
    private void OnGenerateVisualContent(MeshGenerationContext context) {
        Rect rect = new Rect(
            contentRect.x - resolvedStyle.paddingLeft,
            contentRect.y - resolvedStyle.paddingTop,
            contentRect.width + resolvedStyle.paddingLeft + resolvedStyle.paddingRight,
            contentRect.height + resolvedStyle.paddingBottom + resolvedStyle.paddingTop
        );

        GenerateTexture();       

        BorderMesh border = new BorderMesh(rect, resolvedStyle);

        border.OverrideBorderWidth(_top, _right, _bottom, _left);
        border.OverrideBorderStyle(_color, false, true, true);

        if (border.GenerateBorder()) {            
            MeshWriteData mesh = context.Allocate(border.Vertices.Length, border.Indices.Length, _texture);

            mesh.SetAllVertices(border.Vertices);
            mesh.SetAllIndices(border.Indices);
        }
    }


    /// <summary> Generate the dashed texture for the longest side </summary>
    private void GenerateTexture() {
        if(!_texture) {
            _texture = new Texture2D(1, 1);
            _texture.filterMode = FilterMode.Point;
        }

        Color32[] c = new Color32[Mathf.FloorToInt(Mathf.Max(layout.width, layout.height))];
        
        int pixel = 0;
        bool flip = true;

        for(int i = 0; i < c.Length; i++) {           
            c[i] = flip ? Color.white : Color.clear;

            pixel++;
                        
            if(pixel >= (flip ? _solid : _clear)) {
                pixel = 0;
                flip = !flip;
                continue;
            }
        }

        if(_texture.width != c.Length) _texture.Resize(Mathf.Max(c.Length, 1), 1);

        if(c.Length > 0) {
            _texture.SetPixels32(c);
            _texture.Apply();
        }
    }

}
