using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace Edit {
    public class DraftsInterface : MonoBehaviour {
        public enum Mode {
            Load,
            Save
        }

        public RectTransform ListItemPrefab;
        public InputField SearchField;
        public InputField NameField;
        public RectTransform ListContainer;
        public Button ActionButton;
        public Mode State;

        private IEnumerable<FileInfo> _files;
        private IEnumerable<string> _visibleDrafts;
        private WorldSerialization _worldSerial;

        public void OpenSave() {
            State = Mode.Save;
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public void OpenLoad() {
            State = Mode.Load;
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        void Start() {
            _worldSerial = WorldSerialization.GetInstance();
            ActionButton.onClick.AddListener(DraftAction);
        }

        private void DraftAction() {
            switch (State) {
                case Mode.Load:
                    _worldSerial.Read(NameField.text);
                    break;
                case Mode.Save:
                    _worldSerial.Write(NameField.text);
                    break;
            }
            gameObject.SetActive(false);
        }

        void OnEnable() {
            var directory = new DirectoryInfo(WorldSerialization.SavePath);
            _files = directory.GetFiles("*." + WorldSerialization.FileExtension,
                                                             SearchOption.AllDirectories);

            SearchField.onValueChange.AddListener(UpdateDrafts);
            ActionButton.GetComponentInChildren<Text>().text = State.ToString();

            UpdateDrafts();
        }

        void OnDisable() {
            SearchField.onValueChange.RemoveListener(UpdateDrafts);
        }

        public void UpdateDrafts(string filterString = null) {
            foreach (RectTransform child in ListContainer.transform) {
                Destroy(child.gameObject);
            }

            var files = _files;
            if (!string.IsNullOrEmpty(filterString)) {
                files = files.Where(f => f.Name.ToLower().Contains(filterString.ToLower()));
            }

            var draftNames = files.Select(f => Path.GetFileNameWithoutExtension(f.Name));
            foreach (var draft in draftNames) {
                AddDraftEntry(draft);
            }
        }

        private void AddDraftEntry(string draft) {
            var listItem = Instantiate(ListItemPrefab);
            listItem.name = draft;
            listItem.transform.SetParent(ListContainer, false);
            var button = listItem.GetComponentInChildren<Button>();
            var text = button.GetComponentInChildren<Text>();

            text.text = draft;
            switch (State) {
                case Mode.Load:
                    button.onClick.AddListener(() => SetDraft(draft));
                    break;
                case Mode.Save:
                    button.onClick.AddListener(() => SetDraft(draft));
                    break;
            }
        }

        private void SetDraft(string draft) {
            NameField.text = draft;
        }
    }
}
