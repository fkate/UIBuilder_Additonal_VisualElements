using UnityEngine;
using UnityEngine.UIElements;

public class BorderMesh {

    private readonly Vector2[] _normal = new Vector2[] {
        new Vector2( 0, -1),
        new Vector2( 1,  0),
        new Vector2( 0,  1),
        new Vector2(-1,  0)
    };

    private float[] _radii;
    private float[] _widths;
    private Vector2[] _corners;
    private Color32[] _colors;
    private int[] _subdivisions;
    private Vector2 _center;
    private bool _fade;
    private bool _inset;
    private bool _sideUV;
    private Rect _rect;

    public Vertex[] Vertices;
    public ushort[] Indices;

    private int _currentVert;
    private int _currentInd;


    /// <summary> Setup all needed arrays for the generation </summary>
    public BorderMesh(Rect rect, IResolvedStyle style) { 
        float xMin = rect.xMin - style.borderLeftWidth;
        float xMax = rect.xMax + style.borderRightWidth;
        float yMin = rect.yMin - style.borderTopWidth;
        float yMax = rect.yMax + style.borderBottomWidth;

        _center = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
        _rect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);

        _radii = new float[] {
            style.borderTopLeftRadius,
            style.borderTopRightRadius,
            style.borderBottomRightRadius,
            style.borderBottomLeftRadius
        };

        _widths = new float[] {
            Mathf.Max(style.borderTopWidth, 0),
            Mathf.Max(style.borderRightWidth, 0),
            Mathf.Max(style.borderBottomWidth, 0),
            Mathf.Max(style.borderLeftWidth, 0)
        };

        _corners = new Vector2[] {
            new Vector2(xMin + _radii[0], yMin + _radii[0]),
            new Vector2(xMax - _radii[1], yMin + _radii[1]),
            new Vector2(xMax - _radii[2], yMax - _radii[2]),
            new Vector2(xMin + _radii[3], yMax - _radii[3])
        };

        _subdivisions = new int[] {
            Mathf.FloorToInt(Mathf.Max(_radii[0] * 2, 1)),
            Mathf.FloorToInt(Mathf.Max(_radii[1] * 2, 1)),
            Mathf.FloorToInt(Mathf.Max(_radii[2] * 2, 1)),
            Mathf.FloorToInt(Mathf.Max(_radii[3] * 2, 1))
        };

        _colors = new Color32[] {
            style.borderTopColor,
            style.borderRightColor,
            style.borderBottomColor,
            style.borderLeftColor
        };

        _sideUV = false;
        _fade = false;
        _inset = true;
    }


    /// <summary> Overwrite the width of the border so that it can be fed seperate values from the main border </summary>
    public void OverrideBorderWidth(float top, float right, float bottom, float left) {
       _widths = new float[] {
            Mathf.Max(top, 0),
            Mathf.Max(right, 0),
            Mathf.Max(bottom, 0),
            Mathf.Max(left, 0)
        };
    }

    
    /// <summary> General settings to overwrite the behaviour of the border generation </summary>
    public void OverrideBorderStyle(Color32 color, bool fade = false, bool inset = true, bool generateSideUV = false) {
       _colors = new Color32[] {
            color,
            color,
            color,
            color
       };

        _fade = fade;
        _inset = inset;
        _sideUV = generateSideUV;
    }


    /// <summary> Generate border vertices and indices with the given settings </summary>
    public bool GenerateBorder() {
        int vertexCount = 0;
        int indexCount = 0;

        // Precalculate points and skip over zero width borders
        int pointCount = 0;
        bool addBorder = false;

        for(int i = 0; i < 4; i++) {
            addBorder = _widths[i] != 0;
            pointCount = _subdivisions[i] + _subdivisions[(i + 1) % 4];

            vertexCount += addBorder ? pointCount * 2 : 0;
            indexCount += addBorder ? (pointCount - 1) * 6 : 0;
        }

        if(vertexCount == 0 || indexCount == 0) return false;

        Vertices = new Vertex[vertexCount];
        Indices = new ushort[indexCount];

        // Calculate the major and minor side for uv squashing
        float xSquash = 1, ySquash = 1;

        if (_sideUV) {
            xSquash = _rect.width > _rect.height ? 1.0f :  _rect.width / _rect.height;
            ySquash = _rect.height > _rect.width ? 1.0f :  _rect.height / _rect.width;
        }

        // Generate the four borders
        for(int side = 0; side < 4; side++) {
            if(_widths[side] == 0) continue;

            int left = side - 1;
            int right = side + 1;
            left = left < 0 ? left + 4 : left;
            right = right >= 4 ? right - 4 : right;

            int start = _currentVert;
            pointCount = AddBorder(left, side, right, true);
            AddBorderIndices(start, pointCount);
            if(_sideUV) AddBorderCoordinates(start, pointCount, side % 2 == 0 ? xSquash : ySquash, side % 2 == 0);

        }

        return true;
    }


    /// <summary> Secondary generation method for generating a filled mesh </summary>
    public bool GenerateFilled() {       
        int vertexCount = 1;
        int indexCount = 0;

        // Precalculate verex count with one extra vertex for the middle point
        for(int i = 0; i < 4; i++) {
            vertexCount += _subdivisions[i] * 2;
            indexCount += (_subdivisions[i] * 2 - 1);
        }

        Vertices = new Vertex[vertexCount];
        Indices = new ushort[indexCount * 3];

        _currentVert = 0;
        _currentInd = 0;

        _widths = new float[] {1, 1, 1, 1};

        // Generate the border but always overwrite the last entry with the first from the next so that no double vertices exist
        for(int side = 0; side < 4; side++) {
            int left = side - 1;
            int right = side + 1;
            left = left < 0 ? left + 4 : left;
            right = right >= 4 ? right - 4 : right;

            AddBorder(left, side, right, false);

            _currentVert--;
        }

        Vertices[_currentVert].position = new Vector3(_center.x, _center.y, 0);
        Vertices[_currentVert].tint = Color.white;

        // Connect all vertices with the middle point
        for(int i = 0; i < indexCount; i++) {
            Indices[_currentInd++] = (ushort) _currentVert;
            Indices[_currentInd++] = (ushort) i;
            Indices[_currentInd++] = (ushort) ((i + 1) % (_currentVert));
        }

        // Set uv from local position
        for(int i = 0; i < vertexCount; i++) {
            Vertices[i].uv = new Vector2((Vertices[i].position.x - _rect.x) / _rect.width, 1 - (Vertices[i].position.y - _rect.y) / _rect.height);
        }

        return true;
    }


    /// <summary> Add a border side </summary>
    public int AddBorder(int left, int center, int right, bool inner) {
        bool isHardCorner = _inset && (_widths[center] >= _radii[center] || _widths[left] >= _radii[center]);

        for(int i = 0; i < _subdivisions[center]; i++) {
            float weight = i / (float) (_subdivisions[center] - 1);
            float ballance = _widths[center] / (_widths[center] + _widths[left]);
            float normalWeight = weight * ballance + (1 - ballance);

            AddBorderSubdivision(center, center, left, normalWeight, inner, isHardCorner);
        }

        isHardCorner = _inset && (_widths[center] >= _radii[right] || _widths[right] >= _radii[right]);

        for(int i = 0; i < _subdivisions[right]; i++) {
            float weight = 1 - (i / (float) (_subdivisions[right] - 1));
            float ballance = _widths[center] / (_widths[center] + _widths[right]);
            float normalWeight = weight * ballance + (1 - ballance);

            AddBorderSubdivision(right, center, right, normalWeight, inner, isHardCorner);
        }

        return (_subdivisions[center] + _subdivisions[right]);
    }


    /// <summary> Add a row of subdivison points to the current border side </summary>
    private void AddBorderSubdivision(int corner, int center, int other, float normalWeight, bool inner, bool hardCorner) {
        Vector2 dir = Vector2.Lerp(_normal[other], _normal[center], normalWeight).normalized;

        Vertices[_currentVert].position = _corners[corner] + dir * _radii[corner];
        Vertices[_currentVert].tint = _colors[center];

        _currentVert++;

        if (inner) {
            if (hardCorner) {
                Vertices[_currentVert].position = _corners[corner] - _normal[other] * (_widths[other] - _radii[corner]) - _normal[center] * (_widths[center] - _radii[corner]);
            } else if (!_inset && _subdivisions[corner] == 1){
                Vertices[_currentVert].position = _corners[corner] + _normal[other] * (_widths[other] + _radii[corner]) + _normal[center] * (_widths[center] + _radii[corner]);          
            } else {
                Vector2 scale = _normal[center] * (_widths[center] / _radii[corner]) + _normal[other] * (_widths[other] / _radii[corner]);               

                dir *= _radii[corner] * new Vector2(-Mathf.Abs(scale.x), -Mathf.Abs(scale.y)) * (_inset ? 1 : -1);

                Vertices[_currentVert].position = Vertices[_currentVert - 1].position + new Vector3(dir.x, dir.y, 0);
            }

            Vertices[_currentVert].tint = _colors[center];
            if(_fade) Vertices[_currentVert].tint.a = (byte) 0;

            _currentVert++;
        }
    }


    /// <summary> Connect the indices for a border </summary>
    private void AddBorderIndices(int start, int length) {

        for(int i = 0; i < length - 1; i++) {
            Indices[_currentInd++] = (ushort) (start);
            Indices[_currentInd++] = (ushort) (start + (_inset ? 2 : 1));
            Indices[_currentInd++] = (ushort) (start + (_inset ? 1 : 2));
            Indices[_currentInd++] = (ushort) (start + 3);
            Indices[_currentInd++] = (ushort) (start + (_inset ? 1 : 2));
            Indices[_currentInd++] = (ushort) (start + (_inset ? 2 : 1));

            start += 2;
        }
    }


    /// <summary> Add coordinates around the border squashed to the side ratio </summary>
    private void AddBorderCoordinates(int start, int length, float squash, bool invert) {
        float totalLength = 0;

        Vector2 lastPosition = Vertices[start].position;
        Vector2 position;

        float[] distStep = new float[length];
        distStep[0] = 0;

        for(int i = 1; i < length; i++) {
            position = Vertices[start + i * 2].position;
            distStep[i] = Vector2.Distance(position, lastPosition);
            totalLength += distStep[i];
            lastPosition = position;
        }

        int vertIndex = start;

        float u = 0;
        float uIn = 0;

        float inSquash = 1;
        float step = 0;

        // for hard edges try to unstretch inner side
        if(length == 2) {
            float innerDist = Vector2.Distance(Vertices[vertIndex + 1].position, Vertices[vertIndex + 3].position);
            inSquash = innerDist / totalLength;
            uIn += ((1 - inSquash) * 0.5f) * squash;
        }

        for(int i = 0; i < length; i++) {
            step = (distStep[i] / totalLength) * squash;
            u += step;
            uIn += step * inSquash;

            Vertices[vertIndex++].uv = new Vector2(invert ? 1 - u : u, 0);
            Vertices[vertIndex++].uv = new Vector2(invert ? 1 - uIn : uIn, 0);
        }
    }

}
