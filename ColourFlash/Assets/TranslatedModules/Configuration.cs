using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using UnityEngine;

public class Configuration<T> {

	readonly string SettingsPath = null;
	static readonly object settingsFileLock = new object();
	const string deprecatedMsg = "This configuration file is deprecated. Please use the new one. You can safely delete this one.";

	public Configuration(string filename, bool appendWithJson = true) {
		if (appendWithJson) {
			filename += ".json";
		}
		SettingsPath = Path.Combine(Path.Combine(Application.persistentDataPath, "Modsettings"), filename);
	}

	public string SerializeSettings(T settings) {
		return JsonConvert.SerializeObject(settings, Formatting.Indented, new StringEnumConverter());
	}

	public T Settings {
		get {
			try {
				lock (settingsFileLock) {
					if (!File.Exists(SettingsPath)) {
						File.WriteAllText(SettingsPath, SerializeSettings(Activator.CreateInstance<T>()));
					}

					T deserialized = JsonConvert.DeserializeObject<T>(
						File.ReadAllText(SettingsPath),
						new JsonSerializerSettings { Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) => args.ErrorContext.Handled = true }
					);
					return deserialized != null ? deserialized : Activator.CreateInstance<T>();
				}
			}
			catch (Exception e) {
				Debug.LogException(e);
				return Activator.CreateInstance<T>();
			}
		}
		set {
			if (value.GetType() == typeof(T)) {
				lock (settingsFileLock) {
					File.WriteAllText(SettingsPath, SerializeSettings(value));
				}
			}
		}
	}

	public T OldSettings {
		get {
			try {
				lock (settingsFileLock) {
					if (!File.Exists(SettingsPath)) {
						return default(T);
					}
					if (File.ReadAllText(SettingsPath) == deprecatedMsg) {
						return default(T);
					}

					T deserialized = JsonConvert.DeserializeObject<T>(
						File.ReadAllText(SettingsPath),
						new JsonSerializerSettings { Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) => args.ErrorContext.Handled = true }
					);
					return deserialized != null ? deserialized : default(T);
				}
			}
			catch (Exception e) {
				Debug.LogException(e);
				return default(T);
			}
		}
	}

	public void ClearFile() {
		try {
			lock (settingsFileLock) {
				if (File.Exists(SettingsPath)) {
					File.WriteAllText(SettingsPath, deprecatedMsg);
				}
			}
		}
		catch (Exception e) {
			Debug.LogException(e);
		}
	}

	public override string ToString() {
		return SerializeSettings(Settings);
	}
}
