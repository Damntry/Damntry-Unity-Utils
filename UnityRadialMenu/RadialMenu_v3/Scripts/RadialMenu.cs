using Damntry.UtilsUnity;
using Damntry.UtilsUnity.Components.InputManagement.Model;
using Damntry.UtilsUnity.Rendering;
using Damntry.UtilsUnity.UI.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-04-26 PM 2:47:40
// 작성자 : Rito

namespace Rito.RadialMenu_v3
{
    [DisallowMultipleComponent]
    public partial class RadialMenu : MonoBehaviour {

        private const string HotkeyObjName = "HotkeyBackground";

        /// <summary>The ratio intended to be used when measuring the center range</summary>
        private const float wheelScreenRatio = 16f/9f;

        private static RadialMenu instance;

        private float lastResolutionWidth;
        private float lastResolutionHeight;

        private static int pieceCount = 8; // 조각 개수


        private static float appearanceDuration = .3f; // 등장에 걸리는 시간

        private static float disppearanceDuration = .3f; // 소멸에 걸리는 시간
        private static float pieceDist = 180f; // 중앙으로부터 각 조각의 거리

        private static float centerRange = 0.1f; // 커서 인식 안되는 중앙 범위

        private static GameObject pieceSample; // 복제될 조각 게임오브젝트
        private static RectTransform arrow;    // 화살표 이미지의 부모 트랜스폼
        private static RectTransform centerX;

        private static AppearanceType appearanceType;
        private static EasingType appearanceEasing;

        private static bool quickKeysEnabled;
        private static TextSettings textSettings;

        private static MainType mainType;

        private static AppearanceType disappearanceType;
        private static EasingType disappearanceEasing;


        //[Header("Options")]
        //[Range(2, 16)]
        [SerializeField] private int _pieceCount = 8; // 조각 개수

        //[Range(0.2f, 1f)]
        [SerializeField] private float _appearanceDuration = .3f; // 등장에 걸리는 시간
        //[Range(0.2f, 1f)]
        [SerializeField] private float _disppearanceDuration = .3f; // 소멸에 걸리는 시간
        [SerializeField] private float _pieceDist = 180f; // 중앙으로부터 각 조각의 거리

        //[Range(0.01f, 0.5f)]
        [SerializeField] private float _centerRange = 0.1f; // 커서 인식 안되는 중앙 범위
        private float _defaultRatioCenterRange;

        private bool _quickKeysEnabled;
        private static Dictionary<int, WheelKeyBind> _quickKeysData;
        private TextSettings _textSettings;

        //[Header("Objects")]
        [SerializeField] private GameObject _pieceSample; // 복제될 조각 게임오브젝트
        [SerializeField] private RectTransform _arrow;    // 화살표 이미지의 부모 트랜스폼
        private RectTransform _centerX;

        private float angleRangePerPiece;

        // 복제된 조각들
        private Image[] _pieceImages;
        private RectTransform[] _pieceRects;
        private Vector2[] _pieceDirections; // 각 조각이 위치할 방향의 벡터

        private float _arrowRotationZ;

        //[Header("Debug"), Space(20)]
        [SerializeField]
        private int _selectedIndex = -1;

        private const float NotSelectedPieceAlpha = 0.5f;
        private static readonly Color SelectedPieceColor    = new Color(1f, 1f, 1f, 1f);
        private static readonly Color NotSelectedPieceColor = new Color(1f, 1f, 1f, NotSelectedPieceAlpha);

        private static Texture2D hotkeyBackgroundTexture;

        public event Action OnDestroyEvent;

        private void OnDestroy() => OnDestroyEvent?.Invoke();

        public static void InitializeProperties(int pieceCount, GameObject pieceSample, RectTransform arrow,
                RectTransform centerX, bool quickKeysEnabled, KeyCode[] modifiers, TextSettings textSettings,
                out Dictionary<int, WheelKeyBind> quickKeysData,
                float pieceDist = 180f,
                float centerRange = 0.1f,
                MainType mainType = MainType.AlphaAndScaleChange,
                float appearanceDuration = .3f,
                AppearanceType appearanceType = AppearanceType.None,
                EasingType appearanceEasing = EasingType.None,
                float disppearanceDuration = .3f,
                AppearanceType disappearanceType = AppearanceType.None,
                EasingType disappearanceEasing = EasingType.None) {

            if (appearanceDuration < 0.1f || disppearanceDuration < 0.1f) {
                throw new ArgumentException("appearanceDuration/disppearanceDuration must be at least 0.1f seconds.");
            }
            if (centerRange < 0.01f || centerRange > 0.5f) {
                throw new ArgumentException("centerRange must be between 0.01f and 0.5f.");
            }
            
            RadialMenu.pieceCount = pieceCount;
            RadialMenu.pieceSample = pieceSample;
            RadialMenu.arrow = arrow;
            RadialMenu.centerX = centerX;
            RadialMenu.quickKeysEnabled = quickKeysEnabled;
            RadialMenu.textSettings = textSettings;
            RadialMenu.pieceDist = pieceDist;
            RadialMenu.centerRange = centerRange;
            RadialMenu.mainType = mainType;
            RadialMenu.appearanceType = appearanceType;
            RadialMenu.appearanceDuration = appearanceDuration;
            RadialMenu.appearanceEasing = appearanceEasing;
            RadialMenu.disppearanceDuration = disppearanceDuration;
            RadialMenu.disappearanceType = disappearanceType;
            RadialMenu.disappearanceEasing = disappearanceEasing;

            quickKeysData = UpdateQuickKeyData(quickKeysEnabled, pieceCount, modifiers);
        }

        private void InitializeEditorValues() {       

            angleRangePerPiece = 360f / pieceCount;
            
            _pieceCount = pieceCount;
            _pieceSample = pieceSample;
            _arrow = arrow;
            _centerX = centerX;
            _quickKeysEnabled = quickKeysEnabled;
            _textSettings = textSettings;
            _pieceDist = pieceDist;
            _centerRange = centerRange;
            _defaultRatioCenterRange = centerRange;
            _mainType = mainType;
            _appearanceType = appearanceType;
            _appearanceDuration = appearanceDuration;
            _appearanceEasing = appearanceEasing;
            _disppearanceDuration = disppearanceDuration;
            _disappearanceType = disappearanceType;
            _disappearanceEasing = disappearanceEasing;
        }

        public bool IsPieceSelected() => _selectedIndex >= 0;

        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .

        private void Awake() {
            try {
                instance = this;

                InitializeEditorValues();

                InitPieceImages();
                InitPieceDirections();
                InitStateDicts();

                UpdateQuickKeys();
            } finally { 
                HideGameObject(); 
            }
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        /// <summary> 조각 샘플 복제하여 조각들 생성 </summary>
        private void InitPieceImages()
        {
            _pieceSample.SetActive(true);

            _pieceImages = new Image[_pieceCount];
            _pieceRects = new RectTransform[_pieceCount];

            for (int i = 0; i < _pieceCount; i++)
            {
                // 조각 복제
                var clone = Instantiate(_pieceSample, transform);
                clone.name = $"Piece {i}";

                // Image, RectTransform 가져와 배열에 초기화
                _pieceImages[i] = clone.GetComponent<Image>();
                _pieceRects[i] = _pieceImages[i].rectTransform;
            }

            _pieceSample.SetActive(false);
        }

        private void GenerateKeyBindObject(WheelKeyBind keyBind, Transform parentT) {
            //Opaque background
            GameObject hotkeyBackground = new (HotkeyObjName, typeof(RectTransform));
            RectTransform rectT = hotkeyBackground.GetComponent<RectTransform>();
            rectT.SetAnchor(textSettings.Anchor, textSettings.AnchorOffsets.x, textSettings.AnchorOffsets.y);
            rectT.sizeDelta = textSettings.BackgroundSize;

            Image image = hotkeyBackground.AddComponent<Image>();
            image.sprite = AssetLoading.GetSpriteFromTexture(GetHotkeyBackgroundTexture(), Vector2.zero);
            //image.color = textSettings.BackgroundColor;
            
            hotkeyBackground.transform.SetParent(parentT, false);

            //Text showing the hotkey
            GameObject hotkeyText = new ("HotkeyText", typeof(RectTransform));
            rectT = hotkeyText.GetComponent<RectTransform>();
            rectT.SetAnchor(AnchorPresets.MiddleCenter);

            Text text = hotkeyText.AddComponent<Text>();
            text.fontStyle = textSettings.FontStyle;
            text.fontSize = textSettings.FontSize;
            text.font = textSettings.Font;
            text.color = textSettings.FontColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            text.text = KeyCodeUtils.KeyCombinationToString(useLiterals: false, 
                keyBind.KeyCode, modifiers: keyBind.Modifiers);

            hotkeyText.transform.SetParent(hotkeyBackground.transform, false);
        }

        
        private Texture2D GetHotkeyBackgroundTexture() {
            if (!hotkeyBackgroundTexture) {
                hotkeyBackgroundTexture = TextureUtils.GenerateRectRoundCorners(width: 256, height: 42,
                    radius: 14f, edgeSoftness: 1.75f, fillColor: textSettings.BackgroundColor);
            }

            return hotkeyBackgroundTexture;
        }

        /// <summary> 시계 극좌표계를 이용해 각 조각들의 방향벡터 계산 </summary>
        private void InitPieceDirections()
        {
            _pieceDirections = new Vector2[_pieceCount];

            float angle = 360f / _pieceCount;

            for (int i = 0; i < _pieceCount; i++)
            {
                _pieceDirections[i] = new ClockwisePolarCoord(1f, angle * i).ToVector2();
            }
        }

        public void UpdateResolutionRatio(float width, float height) {
            if (width == lastResolutionWidth && height == lastResolutionHeight) {
                return;
            }

            lastResolutionWidth = width; 
            lastResolutionHeight = height;

            float screenRatio = width / height;

            float ratioDiff = wheelScreenRatio / screenRatio;

            _centerRange = _defaultRatioCenterRange / ratioDiff;
        }

        private void ShowGameObject()
        {
            UpdateResolutionRatio(Screen.currentResolution.width, Screen.currentResolution.height);

            gameObject.SetActive(true);
        }

        private void HideGameObject()
        {
            gameObject.SetActive(false);
        }

        public static void UpdateQuickTools(bool enable, KeyCode[] modifiers, 
                out Dictionary<int, WheelKeyBind> quickKeysData) {

            quickKeysData = UpdateQuickKeyData(enable, pieceCount, modifiers);

            if (instance) {
                instance._quickKeysEnabled = enable;

                instance.UpdateQuickKeys();
            } else {
                quickKeysEnabled = enable;
            }
        }

        private void UpdateQuickKeys() {
            int counter = 0;

            foreach (Transform obj in transform) {
                if (!obj.name.StartsWith("Piece")) {
                    continue;
                }

                Transform hotkeyObj = obj.Find(HotkeyObjName);

                //Always delete existing hotkey in case its modifier has been updated.
                if (hotkeyObj) {
                    DestroyImmediate(hotkeyObj.gameObject);
                }

                if (_quickKeysEnabled && _quickKeysData.TryGetValue(counter++, out WheelKeyBind keyBind)) {
                    //Generate hotkey visualization
                    GenerateKeyBindObject(keyBind, obj);
                }         
            }
        }
        
        private static Dictionary<int, WheelKeyBind> UpdateQuickKeyData(bool enabled, int toolWheelCount, KeyCode[] modifiers) {
            if (quickKeysEnabled) {
                _quickKeysData = GenerateQuickKeyData(pieceCount, modifiers);
            } else {
                _quickKeysData = null;
            }
            return _quickKeysData;
        }

        private static Dictionary<int, WheelKeyBind> GenerateQuickKeyData(int toolWheelCount, KeyCode[] modifiers) {
            const int MaxNumberKeys = 10;
            Dictionary<int, WheelKeyBind> quickKeysData = new();

            int maxKeyCount = Math.Min(toolWheelCount, MaxNumberKeys);

            for (int i = 1; i <= maxKeyCount; i++) {
                int alphaKeyIndex = i == MaxNumberKeys ? 0 : i;

                KeyCode mainKeyCode = KeyCode.Alpha0 + alphaKeyIndex;
                quickKeysData.Add(i - 1, new(mainKeyCode, modifiers));
            }

            return quickKeysData;
        }

        public bool IsRadialShowing() => gameObject.activeSelf;


        /// <summary> 지정한 이미지의 알파값 변경 </summary>
        private void SetPieceAlpha(int index, float alpha)
        {
            _pieceImages[index].color = new Color(1f, 1f, 1f, alpha);
        }

        /// <summary> 지정한 이미지의 중심으로부터의 거리 변경 </summary>
        private void SetPieceDistance(int index, float distance)
        {
            _pieceRects[index].anchoredPosition = _pieceDirections[index] * distance;
        }

        /// <summary> 해당 인덱스의 조각 크기 변경 </summary>
        private void SetPieceScale(int index, float scale)
        {
            _pieceRects[index].localScale = new Vector3(scale, scale, 1f);
        }

        /// <summary> 모든 조각을 중심으로부터 지정 거리만큼 이동 </summary>
        private void SetAllPieceDistance(float distance)
        {
            for (int i = 0; i < _pieceCount; i++)
            {
                _pieceRects[i].anchoredPosition = _pieceDirections[i] * distance;
            }
        }

        /// <summary> 모든 조각 이미지의 알파값 변경 </summary>
        private void SetAllPieceAlpha(float alpha, int selectedIndex = -1)
        {
            for (int i = 0; i < _pieceCount; i++)
            {
                if (i != selectedIndex) {
                    _pieceImages[i].color = new Color(1f, 1f, 1f, alpha);
                }
            }
        }

        /// <summary> 모든 조각의 크기 변경 </summary>
        private void SetAllPieceScale(float scale, int selectedIndex = -1)
        {
            for (int i = 0; i < _pieceCount; i++)
            {
                if (i != selectedIndex) {
                    _pieceRects[i].localScale = new Vector3(scale, scale, 1f);
                }
            }
        }

        private void SetAllPieceImageEnabled(bool enabled)
        {
            for (int i = 0; i < _pieceCount; i++)
            {
                _pieceImages[i].enabled = enabled;
            }
        }

        /// <summary> 화살표 이미지 게임오브젝트 활성화 여부, 회전 설정 </summary>
        private void SetArrow(bool show)
        {
            _arrow.gameObject.SetActive(show);

            if (show)
            {
                _arrow.eulerAngles = Vector3.forward * _arrowRotationZ;
            }
        }

        private void SetCenterX(bool show) {
            _centerX.gameObject.SetActive(show);
        }
        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .

        /// <summary> 등장 </summary>
        public bool Show()
        {
            return ForceToEnterAppearanceState();
        }

        /// <summary> 사라지면서 인덱스 리턴 </summary>
        public int Hide()
        {
            ForceToEnterDisappearanceState();
            SetArrow(false);
            SetCenterX(false);

            return _selectedIndex;
        }

        /// <summary> 각각 피스 이미지(스프라이트) 등록 </summary>
        public void SetPieceImageSprites(Sprite[] sprites)
        {
            int i = 0;
            int len = sprites.Length;
            for (; i < _pieceCount && i < len; i++)
            {
                if (sprites[i] != null)
                {
                    _pieceImages[i].sprite = sprites[i];
                }
            }
        }

        #endregion
    }

    public record struct TextSettings(Font Font, int FontSize, FontStyle FontStyle, Color FontColor, Color BackgroundColor, AnchorPresets Anchor, Vector2 AnchorOffsets, Vector2 BackgroundSize);

    public record struct WheelKeyBind(KeyCode KeyCode, KeyCode[] Modifiers);

}