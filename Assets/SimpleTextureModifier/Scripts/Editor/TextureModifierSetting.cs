using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SimpleTextureModifier {
public class TextureModifierSetting {
	[PreferenceItem("TextureSetting")]
    public static void ShowPreference() {
		SimpleTextureModifierSettings.Output = EditorGUILayout.Toggle(TextureModifier.TextureOutput, SimpleTextureModifierSettings.Output);
		SimpleTextureModifierSettings.ForceFormatSetting = EditorGUILayout.Toggle(TextureModifier.FORCEFORMATSETTING, SimpleTextureModifierSettings.ForceFormatSetting);
	
		SimpleTextureModifierSettings.ForceQualitytSetting = EditorGUILayout.Toggle(TextureModifier.FORCEQUALITYSETTING, SimpleTextureModifierSettings.ForceQualitytSetting);
		if( SimpleTextureModifierSettings.ForceQualitytSetting )
			SimpleTextureModifierSettings.CompressQuality = (int)(TextureCompressionQuality)EditorGUILayout.EnumPopup(TextureModifier.COMPRESSIONQUALITY,(TextureCompressionQuality)SimpleTextureModifierSettings.CompressQuality);
		SimpleTextureModifierSettings.ForceSplitAlpha = EditorGUILayout.Toggle(TextureModifier.FORCESPLITALPHA, SimpleTextureModifierSettings.ForceSplitAlpha);

		SimpleTextureModifierSettings.ForceIOSQualitytSetting = EditorGUILayout.Toggle(TextureModifier.FORCIOSQUALITYSETTING, SimpleTextureModifierSettings.ForceIOSQualitytSetting);
		if( SimpleTextureModifierSettings.ForceIOSQualitytSetting )
			SimpleTextureModifierSettings.CompressIOSQuality = (int)(TextureCompressionQuality)EditorGUILayout.EnumPopup(TextureModifier.IOSQUALITY,(TextureCompressionQuality)SimpleTextureModifierSettings.CompressIOSQuality);
		SimpleTextureModifierSettings.ForceIOSSplitAlpha = EditorGUILayout.Toggle(TextureModifier.IOSSPLITALPHA, SimpleTextureModifierSettings.ForceIOSSplitAlpha);

		SimpleTextureModifierSettings.ForceAndroidQualitytSetting = EditorGUILayout.Toggle(TextureModifier.FORCEANDROIDQUALITYSETTING, SimpleTextureModifierSettings.ForceAndroidQualitytSetting);
		if( SimpleTextureModifierSettings.ForceAndroidQualitytSetting )
			SimpleTextureModifierSettings.CompressAndroidQuality = (int)(TextureCompressionQuality)EditorGUILayout.EnumPopup(TextureModifier.ANDROIDQUALITY,(TextureCompressionQuality)SimpleTextureModifierSettings.CompressAndroidQuality);
		SimpleTextureModifierSettings.ForceAndroidSplitAlpha = EditorGUILayout.Toggle(TextureModifier.ANDROIDSPLITALPHA, SimpleTextureModifierSettings.ForceAndroidSplitAlpha);
		SimpleTextureModifierSettings.ChangAndroidAutoCompressSetting = EditorGUILayout.Toggle(TextureModifier.CHANGEANDROIDAUTOCOMPRESSSETTING, SimpleTextureModifierSettings.ChangAndroidAutoCompressSetting);

		if (GUI.changed) {
			SimpleTextureModifierSettings.Save ();
		}
    }
}
}
	