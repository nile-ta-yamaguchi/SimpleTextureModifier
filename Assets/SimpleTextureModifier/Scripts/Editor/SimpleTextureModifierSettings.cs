using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SimpleTextureModifier {
public class SimpleTextureModifierSettings	
	: EditorScriptableSingleton<SimpleTextureModifierSettings> {
	[SerializeField]
	bool m_Key = false;
	static public bool Key {
		get { return instance.m_Key; }
		set { instance.m_Key = value; }}
	[SerializeField]
	bool m_ForceSTMSetting = false;
	static public bool ForceSTMSetting {
		get { return instance.m_ForceSTMSetting; }
		set { instance.m_ForceSTMSetting = value; }}
}
}