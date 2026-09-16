namespace RayGui_cs
{
    /// <summary>Gui control state.</summary>
    public enum GuiState
    {
        Normal = 0,
        Focused,
        Pressed,
        Disabled,
    }

    /// <summary>Horizontal text alignment inside the control text bounds.</summary>
    public enum GuiTextAlignment
    {
        Left = 0,
        Center,
        Right,
    }

    /// <summary>Vertical text alignment inside the control text bounds.</summary>
    public enum GuiTextAlignmentVertical
    {
        Top = 0,
        Middle,
        Bottom,
    }

    /// <summary>Text wrap mode inside the control text bounds.</summary>
    public enum GuiTextWrapMode
    {
        None = 0,
        Char,
        Word,
    }

    /// <summary>Side of a control on which its scroll bar is drawn.</summary>
    public enum GuiScrollBarSide
    {
        Left = 0,
        Right,
    }

    /// <summary>Gui controls, used to scope style properties.</summary>
    public enum GuiControl
    {
        /// <summary>Setting a style on Default applies it to every control.</summary>
        Default = 0,
        /// <summary>Also used by LabelButton.</summary>
        Label = 1,
        Button = 2,
        /// <summary>Also used by ToggleGroup.</summary>
        Toggle = 3,
        /// <summary>Also used by SliderBar and ToggleSlider.</summary>
        Slider = 4,
        ProgressBar = 5,
        CheckBox = 6,
        ComboBox = 7,
        DropdownBox = 8,
        TextBox = 9,
        ValueBox = 10,
        TabBar = 11,
        ListView = 12,
        ColorPicker = 13,
        ScrollBar = 14,
        StatusBar = 15,
    }

    /// <summary>Base style properties shared by every control.</summary>
    public enum GuiControlProperty
    {
        BorderColorNormal = 0,
        BaseColorNormal = 1,
        TextColorNormal = 2,
        BorderColorFocused = 3,
        BaseColorFocused = 4,
        TextColorFocused = 5,
        BorderColorPressed = 6,
        BaseColorPressed = 7,
        TextColorPressed = 8,
        BorderColorDisabled = 9,
        BaseColorDisabled = 10,
        TextColorDisabled = 11,
        /// <summary>Border size, 0 for no border.</summary>
        BorderWidth = 12,
        /// <summary>Text padding, not considering border.</summary>
        TextPadding = 13,
        /// <summary>A <see cref="GuiTextAlignment"/> value.</summary>
        TextAlignment = 14,
    }

    /// <summary>Global style properties, only settable on <see cref="GuiControl.Default"/>.</summary>
    public enum GuiDefaultProperty
    {
        /// <summary>Text size (glyphs max height).</summary>
        TextSize = 16,
        /// <summary>Text spacing between glyphs.</summary>
        TextSpacing = 17,
        /// <summary>Line control color.</summary>
        LineColor = 18,
        /// <summary>Background color.</summary>
        BackgroundColor = 19,
        /// <summary>Text spacing between lines.</summary>
        TextLineSpacing = 20,
        /// <summary>A <see cref="GuiTextAlignmentVertical"/> value.</summary>
        TextAlignmentVertical = 21,
        /// <summary>A <see cref="GuiTextWrapMode"/> value.</summary>
        TextWrapMode = 22,
    }

    /// <summary>Style properties for <see cref="GuiControl.Toggle"/>.</summary>
    public enum GuiToggleProperty
    {
        /// <summary>ToggleGroup separation between toggles.</summary>
        GroupPadding = 16,
        /// <summary>ToggleGroup bounds width considers all items: 0 width per item, 1 full width.</summary>
        GroupWidthFull = 17,
    }

    /// <summary>Style properties for <see cref="GuiControl.Slider"/>.</summary>
    public enum GuiSliderProperty
    {
        /// <summary>Size of the internal slider bar.</summary>
        SliderWidth = 16,
        /// <summary>Internal slider bar padding.</summary>
        SliderPadding = 17,
    }

    /// <summary>Style properties for <see cref="GuiControl.ProgressBar"/>.</summary>
    public enum GuiProgressBarProperty
    {
        /// <summary>Internal padding.</summary>
        ProgressPadding = 16,
        /// <summary>Increment side: 0 left to right, 1 right to left.</summary>
        ProgressSide = 17,
    }

    /// <summary>Style properties for <see cref="GuiControl.ScrollBar"/>.</summary>
    public enum GuiScrollBarProperty
    {
        ArrowsSize = 16,
        ArrowsVisible = 17,
        ScrollSliderPadding = 18,
        ScrollSliderSize = 19,
        ScrollPadding = 20,
        ScrollSpeed = 21,
    }

    /// <summary>Style properties for <see cref="GuiControl.CheckBox"/>.</summary>
    public enum GuiCheckBoxProperty
    {
        /// <summary>Internal check padding.</summary>
        CheckPadding = 16,
    }

    /// <summary>Style properties for <see cref="GuiControl.ComboBox"/>.</summary>
    public enum GuiComboBoxProperty
    {
        /// <summary>Width of the right button.</summary>
        ComboButtonWidth = 16,
        /// <summary>Button separation.</summary>
        ComboButtonSpacing = 17,
    }

    /// <summary>Style properties for <see cref="GuiControl.DropdownBox"/>.</summary>
    public enum GuiDropdownBoxProperty
    {
        /// <summary>Arrow separation from border and items.</summary>
        ArrowPadding = 16,
        /// <summary>Items separation.</summary>
        DropdownItemsSpacing = 17,
        /// <summary>Arrow hidden.</summary>
        DropdownArrowHidden = 18,
        /// <summary>Roll direction: 0 roll down, 1 roll up.</summary>
        DropdownRollUp = 19,
    }

    /// <summary>Style properties for <see cref="GuiControl.TextBox"/>.</summary>
    public enum GuiTextBoxProperty
    {
        /// <summary>Read-only mode: 0 editable, 1 read-only.</summary>
        TextReadOnly = 16,
    }

    /// <summary>Style properties for <see cref="GuiControl.ValueBox"/>.</summary>
    public enum GuiValueBoxProperty
    {
        /// <summary>Width of the spinner left/right buttons.</summary>
        SpinnerButtonWidth = 16,
        /// <summary>Spinner button separation.</summary>
        SpinnerButtonSpacing = 17,
    }

    /// <summary>Style properties for <see cref="GuiControl.TabBar"/>.</summary>
    public enum GuiTabBarProperty
    {
        /// <summary>Width of the tab items.</summary>
        TabItemsWidth = 16,
        /// <summary>Tab close button: 0 hidden, 1 shown.</summary>
        TabCloseButton = 17,
        /// <summary>Tabs side: 0 bottom, 1 top.</summary>
        TabLineSide = 18,
    }

    /// <summary>Style properties for <see cref="GuiControl.ListView"/>.</summary>
    public enum GuiListViewProperty
    {
        ListItemsHeight = 16,
        ListItemsSpacing = 17,
        /// <summary>Scroll bar size (usually width).</summary>
        ScrollBarWidth = 18,
        /// <summary>A <see cref="GuiScrollBarSide"/> value.</summary>
        ScrollBarSide = 19,
        /// <summary>Items border enabled in normal state.</summary>
        ListItemsBorderNormal = 20,
        ListItemsBorderWidth = 21,
    }

    /// <summary>Style properties for <see cref="GuiControl.ColorPicker"/>.</summary>
    public enum GuiColorPickerProperty
    {
        ColorSelectorSize = 16,
        /// <summary>Width of the right hue bar.</summary>
        HueBarWidth = 17,
        /// <summary>Hue bar separation from the panel.</summary>
        HueBarPadding = 18,
        HueBarSelectorHeight = 19,
        HueBarSelectorOverflow = 20,
    }

    /// <summary>Built-in raygui icons.</summary>
    public enum GuiIconName
    {
        None = 0,
        FolderFileOpen = 1,
        FileSaveClassic = 2,
        FolderOpen = 3,
        FolderSave = 4,
        FileOpen = 5,
        FileSave = 6,
        FileExport = 7,
        FileAdd = 8,
        FileDelete = 9,
        FiletypeText = 10,
        FiletypeAudio = 11,
        FiletypeImage = 12,
        FiletypePlay = 13,
        FiletypeVideo = 14,
        FiletypeInfo = 15,
        FileCopy = 16,
        FileCut = 17,
        FilePaste = 18,
        CursorHand = 19,
        CursorPointer = 20,
        CursorClassic = 21,
        Pencil = 22,
        PencilBig = 23,
        BrushClassic = 24,
        BrushPainter = 25,
        WaterDrop = 26,
        ColorPicker = 27,
        Rubber = 28,
        ColorBucket = 29,
        TextT = 30,
        TextA = 31,
        Scale = 32,
        Resize = 33,
        FilterPoint = 34,
        FilterBilinear = 35,
        Crop = 36,
        CropAlpha = 37,
        SquareToggle = 38,
        Symmetry = 39,
        SymmetryHorizontal = 40,
        SymmetryVertical = 41,
        Lens = 42,
        LensBig = 43,
        EyeOn = 44,
        EyeOff = 45,
        FilterTop = 46,
        Filter = 47,
        TargetPoint = 48,
        TargetSmall = 49,
        TargetBig = 50,
        TargetMove = 51,
        CursorMove = 52,
        CursorScale = 53,
        CursorScaleRight = 54,
        CursorScaleLeft = 55,
        Undo = 56,
        Redo = 57,
        Reredo = 58,
        Mutate = 59,
        Rotate = 60,
        Repeat = 61,
        Shuffle = 62,
        Emptybox = 63,
        Target = 64,
        TargetSmallFill = 65,
        TargetBigFill = 66,
        TargetMoveFill = 67,
        CursorMoveFill = 68,
        CursorScaleFill = 69,
        CursorScaleRightFill = 70,
        CursorScaleLeftFill = 71,
        UndoFill = 72,
        RedoFill = 73,
        ReredoFill = 74,
        MutateFill = 75,
        RotateFill = 76,
        RepeatFill = 77,
        ShuffleFill = 78,
        EmptyboxSmall = 79,
        Box = 80,
        BoxTop = 81,
        BoxTopRight = 82,
        BoxRight = 83,
        BoxBottomRight = 84,
        BoxBottom = 85,
        BoxBottomLeft = 86,
        BoxLeft = 87,
        BoxTopLeft = 88,
        BoxCenter = 89,
        BoxCircleMask = 90,
        Pot = 91,
        AlphaMultiply = 92,
        AlphaClear = 93,
        Dithering = 94,
        Mipmaps = 95,
        BoxGrid = 96,
        Grid = 97,
        BoxCornersSmall = 98,
        BoxCornersBig = 99,
        FourBoxes = 100,
        GridFill = 101,
        BoxMultisize = 102,
        ZoomSmall = 103,
        ZoomMedium = 104,
        ZoomBig = 105,
        ZoomAll = 106,
        ZoomCenter = 107,
        BoxDotsSmall = 108,
        BoxDotsBig = 109,
        BoxConcentric = 110,
        BoxGridBig = 111,
        OkTick = 112,
        Cross = 113,
        ArrowLeft = 114,
        ArrowRight = 115,
        ArrowDown = 116,
        ArrowUp = 117,
        ArrowLeftFill = 118,
        ArrowRightFill = 119,
        ArrowDownFill = 120,
        ArrowUpFill = 121,
        Audio = 122,
        Fx = 123,
        Wave = 124,
        WaveSinus = 125,
        WaveSquare = 126,
        WaveTriangular = 127,
        CrossSmall = 128,
        PlayerPrevious = 129,
        PlayerPlayBack = 130,
        PlayerPlay = 131,
        PlayerPause = 132,
        PlayerStop = 133,
        PlayerNext = 134,
        PlayerRecord = 135,
        Magnet = 136,
        LockClose = 137,
        LockOpen = 138,
        Clock = 139,
        Tools = 140,
        Gear = 141,
        GearBig = 142,
        Bin = 143,
        HandPointer = 144,
        Laser = 145,
        Coin = 146,
        Explosion = 147,
        OneUp = 148,
        Player = 149,
        PlayerJump = 150,
        Key = 151,
        Demon = 152,
        TextPopup = 153,
        GearEx = 154,
        Crack = 155,
        CrackPoints = 156,
        Star = 157,
        Door = 158,
        Exit = 159,
        Mode2D = 160,
        Mode3D = 161,
        Cube = 162,
        CubeFaceTop = 163,
        CubeFaceLeft = 164,
        CubeFaceFront = 165,
        CubeFaceBottom = 166,
        CubeFaceRight = 167,
        CubeFaceBack = 168,
        Camera = 169,
        Special = 170,
        LinkNet = 171,
        LinkBoxes = 172,
        LinkMulti = 173,
        Link = 174,
        LinkBroke = 175,
        TextNotes = 176,
        Notebook = 177,
        Suitcase = 178,
        SuitcaseZip = 179,
        Mailbox = 180,
        Monitor = 181,
        Printer = 182,
        PhotoCamera = 183,
        PhotoCameraFlash = 184,
        House = 185,
        Heart = 186,
        Corner = 187,
        VerticalBars = 188,
        VerticalBarsFill = 189,
        LifeBars = 190,
        Info = 191,
        Crossline = 192,
        Help = 193,
        FiletypeAlpha = 194,
        FiletypeHome = 195,
        LayersVisible = 196,
        Layers = 197,
        Window = 198,
        Hidpi = 199,
        FiletypeBinary = 200,
        Hex = 201,
        Shield = 202,
        FileNew = 203,
        FolderAdd = 204,
        Alarm = 205,
        Cpu = 206,
        Rom = 207,
        StepOver = 208,
        StepInto = 209,
        StepOut = 210,
        Restart = 211,
        BreakpointOn = 212,
        BreakpointOff = 213,
        BurgerMenu = 214,
        CaseSensitive = 215,
        RegExp = 216,
        Folder = 217,
        File = 218,
        SandTimer = 219,
        Warning = 220,
        HelpBox = 221,
        InfoBox = 222,
        Priority = 223,
        LayersIso = 224,
        Layers2 = 225,
        Mlayers = 226,
        Maps = 227,
        Hot = 228,
        Label = 229,
        NameId = 230,
        Slicing = 231,
        ManualControl = 232,
        Collision = 233,
        CircleAdd = 234,
        CircleAddFill = 235,
        CircleWarning = 236,
        CircleWarningFill = 237,
        BoxMore = 238,
        BoxMoreFill = 239,
        BoxMinus = 240,
        BoxMinusFill = 241,
        Union = 242,
        Intersection = 243,
        Difference = 244,
        Sphere = 245,
        Cylinder = 246,
        Cone = 247,
        Ellipsoid = 248,
        Capsule = 249,
        FiletypeFont = 250,
        Filetype3D = 251,
        FiletypeCodeXml = 252,
        FiletypeCodeC = 253,
        FiletypeCodePython = 254,
        FiletypeCodeJs = 255,
        FiletypeIcon = 256,
    }
}
