using UnityEngine;

namespace CoDArchipelago.Messaging
{
    public static class Helpers {
        public static RectTransform CreatePaddedTransform(
            GameObject gameObject,
            Vector2? topLeftPadding = null,
            Vector2? bottomRightPadding = null,
            Vector2? anchorMin = null,
            Vector2? anchorMax = null,
            Vector2? pivot = null
        ) {
            var _topLeftPadding     = topLeftPadding ?? Vector2.zero;
            var _bottomRightPadding = bottomRightPadding ?? Vector2.zero;
            var _anchorMin          = anchorMin ?? Vector2.zero;
            var _anchorMax          = anchorMax ?? Vector2.one;
            var _pivot              = pivot ?? new(0.5f, 0.5f);

            var tr = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            tr.anchorMin = _anchorMin;
            tr.anchorMax = _anchorMax;

            tr.anchoredPosition = (_bottomRightPadding - _topLeftPadding) / 2;
            tr.sizeDelta = (_bottomRightPadding + _topLeftPadding) * -1;
            // tr.anchoredPosition = _topLeftPadding;
            // tr.sizeDelta = (_topLeftPadding + _bottomRightPadding) * -1;

            tr.pivot = _pivot;

            return tr;
        }
    }
}
