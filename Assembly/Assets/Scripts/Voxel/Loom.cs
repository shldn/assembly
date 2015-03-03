using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

[ExecuteInEditMode]
public class Loom : MonoBehaviour
{	
	public static int maxThreads = 16;
	[SerializeField]
	static int numThreads;
	
	[SerializeField]
	private static Loom _current;
	private int _count;
	public static Loom Current
	{
		get
		{
			Initialize();
			return _current;
		}
	}
	public static bool isPlaying = false;
	private void OnEnable() {
		isPlaying = Application.isPlaying;
	}
	
	void Awake()
	{
		_current = this;
		initialized = true;
		mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
	}
	[SerializeField]
	static bool initialized;
	
	static void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			var g = GameObject.Find("Loom");
			if (g == null) {
				g = new GameObject("Loom");
			}
			_current = g.AddComponent<Loom>();
		}
			
	}
	
	private List<Action> _actions = new List<Action>();
	public struct DelayedQueueItem
	{
		public float time;
		public Action action;
	}
	private List<DelayedQueueItem> _delayed = new  List<DelayedQueueItem>();

	List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();
	
	public static void QueueOnMainThread(Action action)
	{
		QueueOnMainThread( action, 0f);
	}
	public static void QueueOnMainThread(Action action, float time)
	{
//		if (!isPlaying) {
//			action();
//			return;
//		}
#if UNITY_EDITOR
		try {
#endif
		if(time != 0)
		{
			lock(Current._delayed)
			{
				Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action});
			}
		}
		else
		{
			lock (Current._actions)
			{
				Current._actions.Add(action);
			}
		}
#if UNITY_EDITOR
		} catch (NullReferenceException) {
			// this gets triggered as the play mode exits and scripts/threads are transitioning to edit mode
		}
#endif
	}
	
	/// <summary>
	/// The Low Priority Queue: execute one item on main thread per update
	/// </summary>
	private List<Action> _lpq = new List<Action>();
	public static void RunLowPriority(Action action)
	{
		lock (Current._lpq)
		{
			Current._lpq.Add(action);
		}
	}
	
//	private List<Action> waitQueue = new List<Action>();
	public static Thread RunAsync(Action a)
	{
//		if (!Application.isPlaying) {
//			a();
//			return null;
//		}
		Initialize();
		if (isPlaying) {
			while(numThreads >= maxThreads)
			{
				Thread.Sleep(0);
			}
		}
		Interlocked.Increment(ref numThreads);
		ThreadPool.QueueUserWorkItem(RunAction, a);
		return null;
	}
	public static IEnumerator RunAsyncCoroutine(Action a)
	{
		if (!Application.isPlaying) {
			a();
			yield break;
		}
		Initialize();
		if (isPlaying) {
			while(numThreads >= maxThreads)
			{
				yield return new WaitForSeconds(0.0001f);
			}
		}
		Interlocked.Increment(ref numThreads);
		ThreadPool.QueueUserWorkItem(RunAction, a);
		yield break;
	}
	
	private static void RunAction(object action)
	{
		try
		{
			((Action)action)();
		}
		catch
		{
		}
		finally
		{
			Interlocked.Decrement(ref numThreads);
		}
			
	}
	
	
	void OnDisable()
	{
		if (_current == this)
		{
			_current = null;
		}
	}
	
	List<Action> _currentActions = new List<Action>();
	List<Action> _currentLPQ = new List<Action>();
	
	// Update is called once per frame
	public void Update()
	{
//		_currentActions.Clear ();
//		lock (waitQueue) {
//			int dif = maxThreads - numThreads;
//			while(dif-- > 0 && waitQueue.Count > 0) {
//				_currentActions.Add(waitQueue[0]);
//				waitQueue.RemoveAt(0);
//			}
//		}
//		foreach(var a in _currentActions) {
//			RunAsync(a);
//		}
		lock (_actions)
		{
			_currentActions.Clear();
			_currentActions.AddRange(_actions);
			_actions.Clear();
		}
		foreach(var a in _currentActions)
		{
			a();
		}
		lock(_delayed)
		{
			_currentDelayed.Clear();
			_currentDelayed.AddRange(_delayed.Where(d=>d.time <= Time.time));
			foreach(var item in _currentDelayed)
				_delayed.Remove(item);
		}
		foreach(var delayed in _currentDelayed)
		{
			delayed.action();
		}
		lock (_lpq)
		{
			_currentLPQ.Clear();
			int count = (Application.isPlaying ? (int)Mathf.Max(1, 0.0667f / Time.deltaTime) : 1);
			while (_lpq.Count > 0 && count-- > 0) {
				_currentLPQ.Add(_lpq[0]);
				_lpq.RemoveAt(0);
			}
		}
		foreach(var a in _currentLPQ)
		{
			a();
		}
	}
	
	private static int mainThreadId;
	public static bool IsMainThread
	{
	    get { return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId; }
	}
//#if UNITY_EDITOR
//	void OnSceneGUI() {
//		Update ();
//	}
//#endif
}

