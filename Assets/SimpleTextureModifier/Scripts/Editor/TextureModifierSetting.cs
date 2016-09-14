using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SimpleTextureModifier {
public class TextureModifierSetting {
	static readonly string TextureOutput = "Texture Output Enable";
	static readonly string FORCEFORMATSETTING = "Force Format Setting";
	static readonly string FORCEFILENAMEINDICATOR = "Force FileName Indicator";
	static readonly string BUILDSPLITALPHAMATERIA = "Build SplitAlpha Material";

	static readonly string FORCEQUALITYSETTING = "Force Quality Setting";
	static readonly string COMPRESSIONQUALITY = "Compression Quality";

	static readonly string FORCIOSQUALITYSETTING = "Force IOS Setting";
	static readonly string IOSQUALITY = "IOS Compression Quality";

	static readonly string FORCEANDROIDQUALITYSETTING = "Force Android Setting";
	static readonly string ANDROIDQUALITY = "Android Compression Quality";
	static readonly string CHANGEANDROIDAUTOCOMPRESSSETTING = "Android AutoCompress Setting";

	[PreferenceItem("TextureSetting")]
    public static void ShowPreference() {
		SimpleTextureModifierSettings.Output = EditorGUILayout.Toggle(TextureOutput, SimpleTextureModifierSettings.Output);
		SimpleTextureModifierSettings.ForceFormatSetting = EditorGUILayout.Toggle(FORCEFORMATSETTING, SimpleTextureModifierSettings.ForceFormatSetting);
		SimpleTextureModifierSettings.ForceFileNameIndicator = EditorGUILayout.Toggle(FORCEFILENAMEINDICATOR, SimpleTextureModifierSettings.ForceFileNameIndicator);
		SimpleTextureModifierSettings.BuildSplitAlphaMaterial = EditorGUILayout.Toggle(BUILDSPLITALPHAMATERIA, SimpleTextureModifierSettings.BuildSplitAlphaMaterial);
	
		SimpleTextureModifierSettings.ForceQualitytSetting = EditorGUILayout.Toggle(FORCEQUALITYSETTING, SimpleTextureModifierSettings.ForceQualitytSetting);
		EditorGUI.DisabledScope disabledScope0 = new EditorGUI.DisabledScope(!SimpleTextureModifierSettings.ForceQualitytSetting);
		try {
				SimpleTextureModifierSettings.CompressQuality = (int)(TextureCompressionQuality)EditorGUILayout.EnumPopup (COMPRESSIONQUALITY, (TextureCompressionQuality)SimpleTextureModifierSettings.CompressQuality);
		} finally {
				disabledScope0.Dispose();
		}
		SimpleTextureModifierSettings.ForceIOSQualitytSetting = EditorGUILayout.Toggle(FORCIOSQUALITYSETTING, SimpleTextureModifierSettings.ForceIOSQualitytSetting);
		EditorGUI.DisabledScope disabledScope1 = new EditorGUI.DisabledScope(!SimpleTextureModifierSettings.ForceIOSQualitytSetting);
		try {
			SimpleTextureModifierSettings.CompressIOSQuality = (int)(TextureCompressionQuality)EditorGUILayout.EnumPopup (IOSQUALITY, (TextureCompressionQuality)SimpleTextureModifierSettings.CompressIOSQuality);
		} finally {
			disabledScope1.Dispose();
		}
		SimpleTextureModifierSettings.ForceAndroidQualitytSetting = EditorGUILayout.Toggle(FORCEANDROIDQUALITYSETTING, SimpleTextureModifierSettings.ForceAndroidQualitytSetting);
		EditorGUI.DisabledScope disabledScope2 = new EditorGUI.DisabledScope(!SimpleTextureModifierSettings.ForceAndroidQualitytSetting);
		try {
			SimpleTextureModifierSettings.CompressAndroidQuality = (int)(TextureCompressionQuality)EditorGUILayout.EnumPopup (ANDROIDQUALITY, (TextureCompressionQuality)SimpleTextureModifierSettings.CompressAndroidQuality);
		} finally {
			disabledScope2.Dispose();
		}

		SimpleTextureModifierSettings.ChangAndroidAutoCompressSetting = EditorGUILayout.Toggle(CHANGEANDROIDAUTOCOMPRESSSETTING, SimpleTextureModifierSettings.ChangAndroidAutoCompressSetting);
		if (GUI.changed) {
			SimpleTextureModifierSettings.Save ();
		}
    }
}
}
	