using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lab {
    public class SmoothMove : MonoBehaviour {
        public Handle handle;
        public Transform obj;

        public Camera camUi;
        public Camera camMain;

        private void Awake() {
            handle.core = this;
            var sp = camMain.WorldToScreenPoint(obj.position);
            handle.Move(sp);
        }

        public void OnMove(Vector2 screenPos) {
            var wp = camMain.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, obj.transform.position.z));
            obj.transform.position = wp;
        }
    
        private void Update() {
        
        }
    }
}