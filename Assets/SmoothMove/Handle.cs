using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lab {
    public class Handle : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {
        public SmoothMove core;
    
        private RectTransform _rtrans;
        private RectTransform _parent;
        private bool _dragging;
        private Vector2 _velocity;
        private Vector3 _prevPosition;
    
        private void Awake() {
            _rtrans = GetComponent<RectTransform>();
            _parent = _rtrans.parent.GetComponent<RectTransform>();
        }
    
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            _dragging = true;
            Move(eventData.position);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            Move(eventData.position);
            _dragging = false;
        }
    
        void IDragHandler.OnDrag(PointerEventData eventData) {
            Move(eventData.position);
        }
    
        private static float RubberDelta(float overStretching, float viewSize) {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        public void Move(Vector3 screenPos) {
            var cam = core.camUi;
            if (core == null) {
                return;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, screenPos, cam, out var p);
            const float max = 50;
            var pr = _parent.rect;
            var clamped = new Vector2(Mathf.Clamp(p.x, pr.xMin, pr.xMax), Mathf.Clamp(p.y, pr.yMin, pr.yMax));
            var offset = CalcOffset(p);
            if (offset.x != 0) {
                clamped.x += RubberDelta(offset.x, max);
            }
        
            if (offset.y != 0) {
                clamped.y += RubberDelta(offset.y, max);
            }
            SetPosition(clamped);
        }

        private Vector2 CalcOffset(Vector3 p) {
            var offset = Vector2.zero;
            var pr = _parent.rect;
            if (p.x < pr.xMin) {
                offset.x = p.x - pr.xMin;
            }
            if (p.x > pr.xMax) {
                offset.x = p.x - pr.xMax;
            }
        
            if (p.y < pr.yMin) {
                offset.y = p.y - pr.yMin;
            }
            if (p.y > pr.yMax) {
                offset.y = p.y - pr.yMax;
            }

            return offset;
        }

        private void SetPosition(Vector3 p) {
            _rtrans.localPosition = p;
            core.OnMove(RectTransformUtility.WorldToScreenPoint(core.camUi, _rtrans.position));
        }
    
        private void LateUpdate() {
            var deltaTime = Time.unscaledDeltaTime;
            if (!_dragging && _velocity != Vector2.zero) {
                var position = _rtrans.localPosition;
                var offset = CalcOffset(_rtrans.localPosition);
                for (int axis = 0; axis < 2; axis++) {
                    if (offset[axis] != 0) {
                        var speed = _velocity[axis];
                        var smoothTime = 0.1f;
                        position[axis] = Mathf.SmoothDamp(_rtrans.localPosition[axis],
                            _rtrans.localPosition[axis] - offset[axis], ref speed, smoothTime, Mathf.Infinity,
                            deltaTime);
                        if (Mathf.Abs(speed) < 1)
                            speed = 0;
                        _velocity[axis] = speed;
                    } else {
                        _velocity[axis] *= Mathf.Pow(0.135f, deltaTime);
                        if (Mathf.Abs(_velocity[axis]) < 1)
                            _velocity[axis] = 0;
                        position[axis] += _velocity[axis] * deltaTime;
                    }
                }
                SetPosition(position);
            }
        
            if (_dragging) {
                var newVelocity = (_rtrans.localPosition - _prevPosition) / deltaTime;
                _velocity = Vector3.Lerp(_velocity, newVelocity, deltaTime * 10);
            }

            _prevPosition = _rtrans.localPosition;
        }
    }
}