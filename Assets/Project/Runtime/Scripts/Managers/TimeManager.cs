using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TimeManager
{
	private static List<object> pauseHolders = new List<object>();

	public static event System.Action<bool> OnPauseStateChanged = null;
	public static event System.Action<float> OnTimeScaleChanged = null;

	public static bool IsPaused { get { return pauseHolders.Count > 0; } }

	private static float m_timeScale = 1f;
	public static float TimeScale
	{
		get
		{
			return m_timeScale;
		}
		set
		{
			if( value >= 0f && m_timeScale != value )
			{
				m_timeScale = value;

				if( pauseHolders.Count == 0 )
				{
					Time.timeScale = m_timeScale;

					if( OnTimeScaleChanged != null )
						OnTimeScaleChanged( m_timeScale );
				}
			}
		}
	}

	static TimeManager()
	{
		SceneManager.sceneUnloaded += OnSceneChanged;
	}

	private static void OnSceneChanged( Scene arg )
	{
		for( int i = pauseHolders.Count - 1; i >= 0; i-- )
		{
			if( pauseHolders[i] == null || pauseHolders[i].Equals( null ) )
				pauseHolders.RemoveAt( i );
		}

		if( pauseHolders.Count == 0 )
			Unpause();
	}

	public static void Pause( object pauseHolder )
	{
		if( !pauseHolders.Contains( pauseHolder ) )
		{
			pauseHolders.Add( pauseHolder );
			
			if( pauseHolders.Count == 1 )
				Pause();
		}
	}

	public static void Unpause( object pauseHolder )
	{
		if( pauseHolders.Remove( pauseHolder ) && pauseHolders.Count == 0 )
			Unpause();
	}

	public static bool IsPausedBy( object pauseHolder )
	{
		return pauseHolders.Contains( pauseHolder );
	}

	private static void Pause()
	{
		Time.timeScale = 0f;

		if( OnPauseStateChanged != null )
			OnPauseStateChanged( true );

		if( OnTimeScaleChanged != null )
			OnTimeScaleChanged( 0f );
	}

	private static void Unpause()
	{
		Time.timeScale = m_timeScale;

		if( OnPauseStateChanged != null )
			OnPauseStateChanged( false );

		if( OnTimeScaleChanged != null )
			OnTimeScaleChanged( m_timeScale );
	}
}