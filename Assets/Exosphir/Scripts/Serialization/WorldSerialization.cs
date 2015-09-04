﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Edit;
using Serialization.ComponentConverters;
using UnityEngine;


namespace Serialization {
    /// <summary>
    /// Serializes all convertible PlacedItems into a file. See <see cref="IComponentConverter"/>
    /// for creating converters that serialize a component. Example implementations for Transform 
    /// and PlacedItem included in the ComponentConverters folder.
    /// </summary>
    public class WorldSerialization : SingletonBehaviour<WorldSerialization> {
        private static readonly Type ConverterType = typeof (IComponentConverter);

        /// <summary>
        /// The folder to save the created levels.
        /// </summary>
        public static string SavePath {
            //is a property otherwise unity complains it is executed out of context
            get { return Path.Combine(Application.dataPath, "Drafts"); }
        } 
        /// <summary>
        /// A number indicating a save format version, if it ever changes.
        /// </summary>
        public const byte SaveVersion = 1;
        /// <summary>
        /// The file extension for save files.
        /// </summary>
        public const string FileExtension = "exom";

		private bool loadWindowOpen = false;

#if UNITY_EDITOR
        private string _name = "";
        void OnGUI() {
			if (!loadWindowOpen) {
	            GUILayout.BeginArea(new Rect(Screen.width - 100, 0, 100, 150));
	            _name = GUILayout.TextField(_name);
	            if (GUILayout.Button("Save")) {
	                Write(_name);
	            }
	            if (GUILayout.Button("Load")) {
					loadWindowOpen = true;
	            }
	            GUILayout.EndArea();
			} else {
				Rect windowRect = new Rect((Screen.width / 2) - 200, (Screen.height / 2) - 200, 400, 400);
				windowRect = GUILayout.Window(0, windowRect, LoadWindow, "Load Window");
			}
        }

		private Vector2 scrollPosition;
		void LoadWindow (int windowID) {
			_name = GUILayout.TextField(_name);
			List<string> fileSearchResults = new List<string>();

			DirectoryInfo dir = new DirectoryInfo(SavePath);
			FileInfo[] info = dir.GetFiles("*." + FileExtension);
			foreach (FileInfo f in info) {
				string fileName = f.Name;
				// Get rid of the file extension without using string.Replace()
				fileName = fileName.Remove(fileName.Length - (FileExtension.Length + 1));

				// If the user types nothing into search, just list all files
				if (_name.Length == 0) {
					fileSearchResults.Add(fileName);

				} else if (_name.Length > 0) {
					bool matchFound = false;
					int searchMatches = 0;

					// If the search is longer than the name, dont even bother searching
					if (_name.Length > fileName.Length) {
						searchMatches = 0;
					} else {
						for (int offset = 0; offset < fileName.Length; offset++) {
							for (int c = 0; c < _name.Length; c++) {  // lel "c++"

								// Prevent from the 2 variables added together from going out of bounds
								if (c + offset < fileName.Length) {
									string tempFileName = fileName.ToLower();
									string temp_name = _name.ToLower();

									// Check to see if the characters match
									if (tempFileName[c + offset] == temp_name[c]) {
										searchMatches++;

										// If all the characters match with _name, return true
										if (searchMatches == _name.Length) {
											matchFound = true;
										}
									}
								}
							}

							// Reset search matches after before every offset change
							searchMatches = 0;
						}
					}

					if (matchFound) {
						fileSearchResults.Add(fileName);
					}
				}
			}

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			for (int i = 0; i < fileSearchResults.Count; i++) {
				if (GUILayout.Button(fileSearchResults[i])) {
					_name = fileSearchResults[i];
					Read(_name);
					loadWindowOpen = false;
				}
			}

			GUILayout.EndScrollView();

			if (GUILayout.Button("Close")) {
				loadWindowOpen = false;
			}
		}
#endif

        /// <summary>
        /// Serializes the current world into a file in the <see cref="SavePath"/>
        /// with the extenson <see cref="FileExtension"/>
        /// </summary>
        /// <param name="saveName">The save name to use</param>
        public void Write(string saveName) {
            var path = PathForFile(saveName);
            //GetDirectoryName can't be null because we specifically append a directory
            //to the dataPath. So the save file can't ever be on the filesystem root.
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var file = File.OpenWrite(path)) {
                Serialize(file, EditorWorld.GetInstance());
            }
        }

        /// <summary>
        /// Deserializes the given save into the current world. For more details on
        /// the location of this file, see <see cref="Write"/> method documentation.
        /// </summary>
        /// <param name="saveName">The save name to load</param>
        public void Read(string saveName) {
            using (var file = File.OpenRead(PathForFile(saveName))) {
                Deserialize(file, EditorWorld.GetInstance());
            }
        }

        /// <summary>
        /// Serializes the given EditorWorld into the stream.
        /// editorWorld parameter necessary to remember method users
        /// to have a EditorWorld in the scene.
        /// </summary>
        /// <param name="target">The target stream of any type.</param>
        /// <param name="editorWorld">The world to serialize</param>
        public void Serialize(Stream target, EditorWorld editorWorld) {
            var world = new SerializedWorld(SaveVersion);
            foreach (Transform child in editorWorld.Container.transform) {
                var componentList = new List<SerializedComponent>();
                foreach (var component in child.GetComponents<Component>()) {
                    var converter = GetConverterForComponent(component);
                    if (converter != null) {
                        componentList.Add(new SerializedComponent {
                            Id = GetConverterId(converter),
                            RawData = converter.Serialize(component)
                        });
                    }
                }
                world.Objects.Add(new SerializedGameObject {
                    Name = child.gameObject.name,
                    CatalogId = child.GetComponent<PlacedItem>().CatalogEntry.Id,
                    Components = componentList
                });
            }
                var formatter = new BinaryFormatter();
                formatter.Serialize(target, world);
        }

        /// <summary>
        /// Deserializes the given stream into the editorWorld.
        /// See <see cref="Serialize"/> for reasoning of necessity
        /// of the EditorWorld parameter
        /// </summary>
        /// <param name="source">Data source to deserialize from</param>
        /// <param name="editorWorld">The world to deserialize into.</param>
        public void Deserialize(Stream source, EditorWorld editorWorld) {
            var formatter = new BinaryFormatter();
            var world = (SerializedWorld)formatter.Deserialize(source);
            var catalog = Catalog.GetInstance();
            foreach (var obj in world.Objects) {
                var item = catalog.GetItemById(obj.CatalogId);
                var hasModel = item != null || item.Model != null;
                var go = hasModel? Instantiate(item.Model) : new GameObject();
                go.name = obj.Name;

                var transformConverter = new TransformConverter();
                var goTransform = obj.Components.First(c => c.Id == GetConverterId(transformConverter));
                obj.Components.Remove(goTransform);
                transformConverter.DeserializeAndAdd(goTransform.RawData, go);

                foreach (var component in obj.Components) {
                    var converter = GetConverterForId(component.Id);
                    if (converter != null) {
                        converter.DeserializeAndAdd(component.RawData, go);
                    }
                }

                editorWorld.RegisterExistingItem(go.GetComponent<PlacedItem>());
            }
        }

        private static IComponentConverter GetConverterForComponent(Component component) {
            //gets first converter that implements ICC<T, whatever>
            var type = GetAllConverterTypes()
                .FirstOrDefault(t => t
                        .GetCustomAttributes(false) //get attributes of current class
                        .Where(a => a is ConverterFor) //filter only ConverterFors
                        .Cast<ConverterFor>() //cast them; return first whose component is of same type
                        .FirstOrDefault(a => a.Component == component.GetType()) != null);
            if (type == null) return null;
            return (IComponentConverter)Activator.CreateInstance(type);
        }

        private static IComponentConverter GetConverterForId(ulong id) {
            var type = GetAllConverterTypes()
                .FirstOrDefault(t => HashString(t.FullName) == id);
            if (type == null) return null;
            return Activator.CreateInstance(type) as IComponentConverter;
        }

        private static IEnumerable<Type> GetAllConverterTypes() {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetInterfaces().Contains(ConverterType));
        }

        /// <summary>
        /// Gets a unique ID based on the converter's class name.
        /// This means that a class rename will invalidate old saves!
        /// </summary>
        /// <param name="converter">The converter to be identified</param>
        /// <returns></returns>
        private static ulong GetConverterId(IComponentConverter converter) {
            return HashString(converter.GetType().FullName);
        }

        private static ulong HashString(string str) {
            var hasher = SHA1.Create();
            var strBytes = Encoding.UTF8.GetBytes(str);
            var hash = hasher.ComputeHash(strBytes);
            return BitConverter.ToUInt64(hash, 0);
        }

        private static string PathForFile(string saveName) {
            var fileName = string.Format("{0}.{1}", saveName, FileExtension);
            return Path.Combine(SavePath, fileName);
        }
    }
}
