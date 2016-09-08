using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SimpleTextureModifier {
public class SimpleTextureModifierSettings : EditorScriptableSingleton<SimpleTextureModifierSettings> {
	[SerializeField]
	bool m_Output;
	static public bool Output {
		get { return instance.m_Output; }
		set { instance.m_Output = value; }}
	[SerializeField]
	bool m_ForceFormatSetting;
	static public bool ForceFormatSetting {
		get { return instance.m_ForceFormatSetting; }
		set { instance.m_ForceFormatSetting = value; }}

	[SerializeField]
	bool m_ForceQualitytSetting;
	static public bool ForceQualitytSetting {
		get { return instance.m_ForceQualitytSetting; }
		set { instance.m_ForceQualitytSetting = value; }}
	[SerializeField]
	int m_CompressQuality;
	static public int CompressQuality {
		get { return instance.m_CompressQuality; }
		set { instance.m_CompressQuality = value; }}
	[SerializeField]
	bool m_ForceSplitAlpha;
	static public bool ForceSplitAlpha {
		get { return instance.m_ForceSplitAlpha; }
		set { instance.m_ForceSplitAlpha = value; }}

	[SerializeField]
	bool m_ForceIOSQualitytSetting;
	static public bool ForceIOSQualitytSetting {
		get { return instance.m_ForceIOSQualitytSetting; }
		set { instance.m_ForceIOSQualitytSetting = value; }}
	[SerializeField]
	int m_CompressIOSQuality;
	static public int CompressIOSQuality {
		get { return instance.m_CompressIOSQuality; }
		set { instance.m_CompressIOSQuality = value; }}
	[SerializeField]
	bool m_ForceIOSSplitAlpha;
	static public bool ForceIOSSplitAlpha {
		get { return instance.m_ForceIOSSplitAlpha; }
		set { instance.m_ForceIOSSplitAlpha = value; }}


	[SerializeField]
	bool m_ForceAndroidQualitytSetting;
	static public bool ForceAndroidQualitytSetting {
		get { return instance.m_ForceAndroidQualitytSetting; }
		set { instance.m_ForceAndroidQualitytSetting = value; }}
	[SerializeField]
	int m_CompressAndroidQuality;
	static public int CompressAndroidQuality {
		get { return instance.m_CompressAndroidQuality; }
		set { instance.m_CompressAndroidQuality = value; }}
	[SerializeField]
	bool m_ForceAndroidSplitAlpha;
	static public bool ForceAndroidSplitAlpha {
		get { return instance.m_ForceAndroidSplitAlpha; }
		set { instance.m_ForceAndroidSplitAlpha = value; }}
	bool m_ChangAndroidAutoCompressSetting;
	static public bool ChangAndroidAutoCompressSetting {
		get { return instance.m_ChangAndroidAutoCompressSetting; }
		set { instance.m_ChangAndroidAutoCompressSetting = value; }}


	protected override void OnCreateInstance() {
		m_Output = true;
		m_ForceFormatSetting = true;

		m_ForceQualitytSetting = false;
		m_CompressQuality = (int)TextureCompressionQuality.Normal;
		m_ForceSplitAlpha = false;;

		m_ForceIOSQualitytSetting = false;
		m_CompressIOSQuality = (int)TextureCompressionQuality.Normal;;
		m_ForceIOSSplitAlpha = false;;

		m_ForceAndroidQualitytSetting = false;
		m_CompressAndroidQuality = (int)TextureCompressionQuality.Normal;;
		m_ForceAndroidSplitAlpha = false;;
		m_ChangAndroidAutoCompressSetting = false;;
	}
}
}