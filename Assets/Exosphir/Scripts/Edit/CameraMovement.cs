using UnityEngine;

namespace Edit {
    public class CameraMovement : MonoBehaviour {
        public Camera Camera;
        public bool EnableZoom = true;
        public float PanSpeed = 1f;
        public float FastMoveMultiplier = 1.5f;
        public float RotationSpeed = 1f;
        public float RotationMaxDelta = 1f;
        public float MaxPitch = 90f;
        public float MinPitch = -90f;
        public float ZoomSpeed = 1f;
        public float MinDistance = .5f;
        public float MaxDistance = 10f;

        private ConfigurableInput _input;
        private ItemPlacement _placement;
        private Vector2 _anglePitchYaw;

        void Start() {
            _input = ConfigurableInput.GetInstance();
            _placement = ItemPlacement.GetInstance();
        }

        void LateUpdate() {
            MoveCamera(_input.horizontal, _input.vertical);

            if (Input.GetButton(_input.orbitKey) && Input.GetMouseButton(0) || Input.GetMouseButton(2)) {
                RotateCamera(_input.mouse);
            }

            if (EnableZoom) {
                ZoomCamera(_input.scroll);
            }
        }

        private void MoveCamera(float horizontal, float vertical) {
            var multiplier = Input.GetKey(_input.fastMovementKey) ? FastMoveMultiplier : 1f;
            var direction = Camera.transform.TransformDirection(new Vector3 {
                x = horizontal,
                y = 0,
                z = vertical
            });
            direction.y = 0;
            //since we discard y the camera speed could be slower if it looks topdown or bottom-up
            //normalize to always have a direction of length 1
            direction.Normalize();
            var delta = direction * (PanSpeed * multiplier * Time.deltaTime); //parentheses so we get 2 float mult and a vector mult instead of 3 vector mult
            var newPosition = transform.position + delta;
            newPosition.y = _placement.CurrentFloorHeight;
            transform.position = newPosition;
        }

        private void RotateCamera(Vector2 mouse) {
            _anglePitchYaw += new Vector2 {
                x = -ClampAngle(mouse.y, MinPitch, MaxPitch),
                y = mouse.x
            };
            transform.rotation = Quaternion.Euler(_anglePitchYaw.x, _anglePitchYaw.y, 0f);
        }

        private void ZoomCamera(float scroll) {
            var newLocalPosition = Camera.transform.localPosition;

            var distance = newLocalPosition.z;
            var delta = scroll * ZoomSpeed * Time.deltaTime;
            var finalDistance = distance + delta;

            newLocalPosition.z = -Mathf.Abs(finalDistance);
            Camera.transform.localPosition = newLocalPosition;
        }

        static private float ClampAngle(float degrees, float min, float max) {
            if (degrees < -360)
                degrees += 360;
            if (degrees > 360)
                degrees -= 360;
            return Mathf.Clamp(degrees, min, max);
        }
    }
}
