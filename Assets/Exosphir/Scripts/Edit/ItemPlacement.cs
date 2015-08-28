using System.Linq;
using UnityEngine;
using Extensions;

namespace EditMode {
    [RequireComponent(typeof(CameraMovement))]
    public sealed class ItemPlacement : SingletonBehaviour<ItemPlacement> {
        public CatalogInterface CatalogInterface;
        public ItemCursor Cursor;
        public Grid Grid;
        public float CursorDamping = 20f;
        public float FloorSwitchDamping = 10f;
        public float FastFloorMultiplier = 10;
        public float FineFloorMultiplier = 0.1f;
        public float FineRotationStep = 5f;
        public float RotationDamping = 10f;
        public float ScaleDamping = 10f;
        public float ScaleStep = 0.2f;
        public float MinScale = 0.4f;
        public float MaxScale = 5f;

        [HideInInspector]
        public float Floor;

        [HideInInspector]
        public Vector3 MouseInGrid;
        [HideInInspector]
        public Vector3 ItemRotationEuler;
        [HideInInspector]
        public float Scale;

        private bool _zoom, _snap;
        private bool _hasRotated;
        private CatalogItem _currentItem;
        private Vector2 _oldMouse;
        private ConfigurableInput _input;
        private CameraMovement _camera;
        private EditorWorld _world;

        public float CurrentFloorHeight { get; private set; }

        public Vector3? ScreenPointToGrid(Vector2 point) {
            var ray = _camera.Camera.ScreenPointToRay(point);
            var plane = new Plane(Vector3.up, Vector3.up * CurrentFloorHeight);

            float distance;
            if (plane.Raycast(ray, out distance)) {
                var hit = ray.GetPoint(distance);
                if (Grid.Contains(hit)) {
                    return hit;
                }
            }
            return null;
        }

        void Start() {
            _input = ConfigurableInput.GetInstance();
            _world = EditorWorld.GetInstance();
            _camera = GetComponent<CameraMovement>();
            Scale = 1f;
        }

        void Update() {
            _zoom = true;
            _snap = !Input.GetKey(_input.turnOffSnap);
            var mouseInGame = this.PointerOnWorld();

            UpdateMouse();
            UpdateCursor();
            if (mouseInGame && Input.GetMouseButton(0)) {
                PlaceCurrentItem();
            }
            if (mouseInGame && Input.GetMouseButton(1)) {
                RemoveItemsAtMouse();
            }

            UpdateFloors();

            _camera.EnableZoom = _zoom;
        }

        private void RemoveItemsAtMouse() {
            foreach (var obj in _world.GetObjectsInCell(MouseInGrid)) {
                _world.Remove(obj);
            }
        }

        private void PlaceCurrentItem() {
            var here = _world.GetObjectsInCell(MouseInGrid).ToArray();
            //dont place if there is a UniqueInSlot item here already
            if (here.Any() && here.First().UniqueInSlot) {
                return;
            }
            var placed = _world.PlaceItemAt(
                item: CatalogInterface.CurrentItem, 
                position: MouseInGrid, 
                rotation: Quaternion.Euler(ItemRotationEuler), 
                scale: Scale);
            placed.UniqueInSlot = _snap;
        }

        private void UpdateMouse() {
            //update mouse in grid only when it actually is on the grid.
            var mousePosWorld = ScreenPointToGrid(Input.mousePosition);
            if (mousePosWorld.HasValue) {
                MouseInGrid = _snap ? Grid.Snap(mousePosWorld.Value) : mousePosWorld.Value;
            }
        }

        private void UpdateCursor() {
            var newCurrent = CatalogInterface.CurrentItem;
            if (newCurrent != _currentItem && newCurrent != null) {
                _currentItem = newCurrent;
                Cursor.Selected = newCurrent.Model;
                Scale = 1f;
                ItemRotationEuler = Vector3.zero;
            }
            if (_currentItem == null) return;

            if (_currentItem.Rotatable) {
                if (Input.GetKey(_input.rotateKey)) {
                    _zoom = false;
                    var angle = _input.scroll * FineRotationStep;
                    if (Mathf.Abs(angle) > 0.1) {
                        if (Input.GetKey(_input.secondaryRotate)) {
                            ItemRotationEuler.x += angle;
                        } else {
                            ItemRotationEuler.y += angle;
                        }
                        _hasRotated = true;
                    }
                }
                if (Input.GetKeyUp(_input.rotateKey)) {
                    if (_hasRotated) {
                        _hasRotated = false;
                    } else {
                        ItemRotationEuler.y += 90;
                    }
                }
            }
            if (_currentItem.Scalable && Input.GetKey(_input.scaleKey)) {
                _zoom = false;
                var delta = _input.scroll * ScaleStep;
                Scale = Mathf.Clamp(Scale + delta, MinScale, MaxScale);
            }
            Cursor.transform.position = Vector3.Lerp(Cursor.transform.position, MouseInGrid, CursorDamping * Time.deltaTime);
            Cursor.transform.rotation = Quaternion.Lerp(Cursor.transform.rotation, Quaternion.Euler(ItemRotationEuler), RotationDamping * Time.deltaTime);
            Cursor.transform.localScale = Vector3.Lerp(Cursor.transform.localScale, Vector3.one * Scale, ScaleDamping * Time.deltaTime);
        }

        private void UpdateFloors() {
            //sum key states in opposite directions
            var floorStepDirection = (Input.GetKeyDown(_input.upFloor) ? 1f : 0f) -
                                     (Input.GetKeyDown(_input.downFloor) ? 1f : 0f);
            var fast = Input.GetKey(_input.fastMovementKey);
            if (fast) floorStepDirection *= FastFloorMultiplier;
            Floor += Mathf.Round(floorStepDirection);

            if (Input.GetKey(_input.fineFloor)) {
                _zoom = false;
                Floor += FineFloorMultiplier * _input.scroll;
            }

            CurrentFloorHeight = Mathf.Lerp(CurrentFloorHeight, Floor, FloorSwitchDamping * Time.deltaTime);
            Grid.transform.position = Vector3.up * Floor;
        }
    }
}
