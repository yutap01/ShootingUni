using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 独自のDebugクラス
/// </summary>
public static class MyDebug{

	/// <summary>
	/// ログを出力する
	/// </summary>
	/// <param name="message">メッセージ</param>
	[System.Diagnostics.Conditional("DEBUG")]
	public static void Log(Object message){
		UnityEngine.Debug.Log(message);
	}
	
	/// <summary>
	/// ログを出力する
	/// </summary>
	/// <param name="message">メッセージ</param>
	/// <param name="context">ログを出力したオブジェクト</param>
	[System.Diagnostics.Conditional("DEBUG")]
	public static void Log(Object message, Object context){
		UnityEngine.Debug.Log(message, context);
	}
	
	/// <summary>
	/// 警告ログを出力する
	/// </summary>
	/// <param name="message">メッセージ</param>
	[System.Diagnostics.Conditional("DEBUG")]
	public static void LogWarning(Object message){
		UnityEngine.Debug.LogWarning(message);
	}
	
	/// <summary>
	/// 警告ログを出力する
	/// </summary>
	/// <param name="message">メッセージ</param>
	/// <param name="context">ログを出力したオブジェクト</param>
	[System.Diagnostics.Conditional("DEBUG")]
	public static void LogWarning(Object message, Object context){
		UnityEngine.Debug.LogWarning(message, context);
	}
	
	/// <summary>
	/// エラーログを出力する
	/// </summary>
	/// <param name="message">メッセージ</param>
	[System.Diagnostics.Conditional("DEBUG")]
	public static void LogError(Object message){
		UnityEngine.Debug.LogError(message);
	}
	
	/// <summary>
	/// エラーログを出力する
	/// </summary>
	/// <param name="message">メッセージ</param>
	/// <param name="context">ログを出力したオブジェクト</param>
	[System.Diagnostics.Conditional("DEBUG")]
	public static void LogError(Object message, Object context){
		UnityEngine.Debug.LogError(message, context);
	}


	/// <summary>
	/// Assert 処理は止まらない
	/// </summary>
	/// <param name="message">メッセージ</param>
	/// <param name="context">ログを出力したオブジェクト</param>
	[System.Diagnostics.Conditional( "DEBUG" )]
	public static void Assert( bool condition, string message ) {
		if( !condition ) {
			UnityEngine.Debug.LogError( message );
		}
	}
}